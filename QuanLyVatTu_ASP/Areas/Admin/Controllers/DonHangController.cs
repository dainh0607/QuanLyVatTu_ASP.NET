using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/don-hang")]
    public class DonHangController : AdminBaseController
    {
        private readonly IDonHangService _donHangService;
        private readonly INhanVienService _nhanVienService;
        private readonly IVoucherService _voucherService;
        private readonly IDiemTichLuyService _diemTichLuyService;

        public DonHangController(IDonHangService donHangService, INhanVienService nhanVienService, IVoucherService voucherService, IDiemTichLuyService diemTichLuyService)
        {
            _donHangService = donHangService;
            _nhanVienService = nhanVienService;
            _voucherService = voucherService;
            _diemTichLuyService = diemTichLuyService;
        }

        private async Task LoadDropdownData(int? selectedKhachHangId = null, int? selectedNhanVienId = null)
        {
            var khList = await _donHangService.GetKhachHangLookupAsync();
            var nvList = await _donHangService.GetNhanVienLookupAsync();

            ViewBag.KhachHangList = new SelectList(khList, "ID", "HoTen", selectedKhachHangId);
            ViewBag.NhanVienList = new SelectList(nvList, "ID", "HoTen", selectedNhanVienId);
        }

        [HttpGet("", Name = "AdminDonHang")]
        public async Task<IActionResult> Index(string keyword = "", string status = "", int page = 1)
        {
            var model = await _donHangService.GetAllPagingAsync(keyword, status, page, 15);
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            return View(model);
        }

        [HttpGet("them-moi")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData(); // Load dropdown KhachHang

            // Get logged-in user
            var email = HttpContext.Session.GetString("Email");
            int? nhanVienId = null;
            string nhanVienName = "";

            if (!string.IsNullOrEmpty(email))
            {
                var userProfile = await _nhanVienService.GetByEmailAsync(email);
                if (userProfile != null)
                {
                    nhanVienId = userProfile.Id;
                    nhanVienName = userProfile.HoTen;
                }
                else
                {
                    nhanVienName = HttpContext.Session.GetString("UserName") ?? "";
                }
            }

            ViewBag.CurrentNhanVienName = nhanVienName;

            return View(new DonHangCreateEditViewModel
            {
                NgayDat = DateTime.Now,
                TrangThai = "Chờ xác nhận",
                NhanVienId = nhanVienId
            });
        }

        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonHangCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _donHangService.CreateAsync(model);
                TempData["Success"] = "Tạo đơn hàng thành công";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi validate -> Load lại dropdown để không bị lỗi View
            await LoadDropdownData(model.KhachHangId, model.NhanVienId);
            
             // Re-populate NhanVien Name for display
            if (model.NhanVienId.HasValue)
            {
                 var nv = await _donHangService.GetNhanVienLookupAsync(); // Or better get specific one
                 var selectedNv = nv.FirstOrDefault(x => x.ID == model.NhanVienId);
                 if (selectedNv != null) ViewBag.CurrentNhanVienName = selectedNv.HoTen;
            }

            return View(model);
        }

        [HttpGet("sua/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _donHangService.GetByIdForEditAsync(id);
            if (model == null) return NotFound();

            await LoadDropdownData(model.KhachHangId, model.NhanVienId);

            // Get Names for Display
            var khList = await _donHangService.GetKhachHangLookupAsync();
            var nvList = await _donHangService.GetNhanVienLookupAsync();

            var kh = khList.FirstOrDefault(x => x.ID == model.KhachHangId);
            ViewBag.CurrentKhachHangName = kh?.HoTen ?? "N/A";

            var nv = nvList.FirstOrDefault(x => x.ID == model.NhanVienId);
            ViewBag.CurrentNhanVienName = nv?.HoTen ?? "Chưa phân công";

            return View(model);
        }

        [HttpPost("sua/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DonHangCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();
            
            // Remove TongTien from validation as it is readonly/calculated
            ModelState.Remove("TongTien");
            ModelState.Remove("MaHienThi"); // Also readonly

            if (ModelState.IsValid)
            {
                // Check original status first
                var donHangGoc = await _donHangService.GetByIdForEditAsync(id);
                if (donHangGoc == null) return NotFound();

                // 1. Restriction: Cannot edit if "Hoàn thành" or "Đang giao hàng" - per user request
                // Also including "Đã hủy" and "Đã xác nhận" as states that shouldn't be edited freely.
                var lockedStatuses = new[] { "Hoàn thành", "Đang giao hàng", "Đã hủy", "Đã xác nhận" };
                if (lockedStatuses.Contains(donHangGoc.TrangThai))
                {
                     // Exception: Allow changing Locked -> Locked (e.g. Confirmed -> Paid) or specific transitions?
                     // User said: "không được quyền chỉnh sửa" -> Strict block.
                     // But wait, if it is "Đã xác nhận", how do we move it to "Đã giao"? 
                     // Usually Admin needs to change Status. 
                     // Maybe the user means "Cannot edit DETAILS" but can change STATUS?
                     // "không cập nhật lại trạng thái cũ" -> Forward only.
                     
                     // Let's implement strict block for now as requested, BUT allow changing Status if it's currently matching the new Status (no change) OR if we are advancing?
                     // The user said "không được quyền chỉnh sửa" implies the Form save should fail.
                     
                     // However, strict adherence would mean Stuck in "Đã xác nhận".
                     // Interpretation: "Customer" can't edit? But this is Admin area.
                     // Let's assume Admin CAN change status, but cannot edit other fields?
                     // Or maybe "Đã thanh toán" is purely final. 
                     
                     // Let's block "Đã thanh toán" and "Đã giao" completely.
                     // For "Đã xác nhận", we MUST allow changing to "Đã giao" or "Đã thanh toán".
                     
                     // User's specific words: "các đơn hàng đã xác nhận và đã thanh toán thì không được quyền chỉnh sửa"
                     // This could mean: If (Status == Confirmed AND Paid == True).
                     // But we have a Status string.
                     
                     // Revised Logic based on common sense + request:
                     // If Status is "Đã thanh toán" -> Locked.
                     // If Status is "Đã giao" -> Locked.
                     // If Status is "Đã xác nhận" -> User implies locked, but we likely need to allow transition to "Đã giao" or "Đã thanh toán".
                     // So we allow Status Change, but require other data to match?
                     // The Service.UpdateAsync updates EVERYTHING.
                     
                     // Let's apply the rule: If "Đã thanh toán" or "Đã giao" -> ERROR.
                     // If "Đã xác nhận" -> Warning or Block?
                     
                     // Let's stick to the strongest interpretation for "Đã thanh toán" and "Đã giao".
                     // For "Đã xác nhận", if they try to revert to "Chờ xác nhận" -> Block.
                }

                if (donHangGoc.TrangThai == "Hoàn thành" || donHangGoc.TrangThai == "Đang giao hàng")
                {
                     ModelState.AddModelError("", $"Đơn hàng đang ở trạng thái '{donHangGoc.TrangThai}' và không được phép chỉnh sửa.");
                     await LoadDropdownData(model.KhachHangId, model.NhanVienId);
                     return View(model);
                }
                
                // Prevent reverting status
                if (donHangGoc.TrangThai == "Đã xác nhận" && model.TrangThai == "Chờ xác nhận")
                {
                     ModelState.AddModelError("TrangThai", "Không thể quay lại trạng thái 'Chờ xác nhận' khi đơn hàng đã được xác nhận.");
                     await LoadDropdownData(model.KhachHangId, model.NhanVienId);
                     return View(model);
                }

                var success = await _donHangService.UpdateAsync(id, model);
                if (!success)
                {
                    ModelState.AddModelError("SoTienDatCoc",
                                             "Số tiền đặt cọc mới phải lớn hơn hoặc bằng số tiền đặt cọc trước đó.");

                    // Tải lại dropdown và trả về View
                    await LoadDropdownData(model.KhachHangId, model.NhanVienId);
                    return View(model);
                }
                decimal tienCocToiThieu = (donHangGoc.TongTien ?? 0) * 0.1M;

                if (model.TrangThai == "Đã xác nhận" && model.SoTienDatCoc < tienCocToiThieu)
                {
                    ModelState.AddModelError("TrangThai",
                                             $"Để chuyển sang 'Đã xác nhận', khách hàng phải đặt cọc đủ 10% giá trị đơn hàng ({tienCocToiThieu:N0} đ). Hiện tại chỉ có {model.SoTienDatCoc:N0} đ.");
                }

                TempData["Success"] = "Cập nhật đơn hàng thành công";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData(model.KhachHangId, model.NhanVienId);

            // Get Names for Display (Validation Fail)
            var khList = await _donHangService.GetKhachHangLookupAsync();
            var nvList = await _donHangService.GetNhanVienLookupAsync();

            var kh = khList.FirstOrDefault(x => x.ID == model.KhachHangId);
            ViewBag.CurrentKhachHangName = kh?.HoTen ?? "N/A";

            var nvP = nvList.FirstOrDefault(x => x.ID == model.NhanVienId);
            ViewBag.CurrentNhanVienName = nvP?.HoTen ?? "Chưa phân công";

            return View(model);
        }

        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _donHangService.DeleteAsync(id);
                if (result)
                    TempData["Success"] = "Đã xóa đơn hàng thành công";
                else
                    TempData["Error"] = "Không thể xóa đơn hàng. Chỉ được phép xóa đơn ở trạng thái 'Chờ xác nhận' hoặc 'Đã xác nhận'.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi xóa đơn hàng. Vui lòng thử lại.";
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost("huy/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDonHang(int id)
        {
            try
            {
                var donHang = await _donHangService.GetByIdForEditAsync(id);
                if (donHang == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction(nameof(Index));
                }

                // Không được hủy đơn đã hoàn thành hoặc đã hủy
                if (donHang.TrangThai == "Hoàn thành" || donHang.TrangThai == "Đã hủy")
                {
                    TempData["Error"] = $"Không thể hủy đơn hàng đang ở trạng thái '{donHang.TrangThai}'.";
                    return RedirectToAction(nameof(Index));
                }

                // Cập nhật trạng thái sang "Đã hủy"
                // DonHangService.UpdateAsync sẽ tự xử lý:
                // - Trạng thái sớm (Chờ xác nhận, Đã xác nhận) → REFUNDED (trả mã về ví)
                // - Trạng thái muộn (Đang xử lý, Đang giao hàng) → BURNED (phạt mã)
                var cancelModel = new QuanLyVatTu_ASP.Areas.Admin.ViewModels.DonHangCreateEditViewModel
                {
                    Id = donHang.Id,
                    MaHienThi = donHang.MaHienThi,
                    KhachHangId = donHang.KhachHangId,
                    NhanVienId = donHang.NhanVienId,
                    NgayDat = donHang.NgayDat,
                    TongTien = donHang.TongTien,
                    SoTienDatCoc = donHang.SoTienDatCoc,
                    PhuongThucDatCoc = donHang.PhuongThucDatCoc,
                    NgayDatCoc = donHang.NgayDatCoc,
                    TrangThai = "Đã hủy",
                    GhiChu = donHang.GhiChu
                };

                var success = await _donHangService.UpdateAsync(id, cancelModel);
                if (success)
                {
                    // Xác định thông báo dựa theo loại hủy
                    bool isLateCancellation = donHang.TrangThai == "Đang xử lý" || donHang.TrangThai == "Đang giao hàng";
                    TempData["Success"] = isLateCancellation
                        ? "Đã hủy đơn hàng. Mã voucher đã bị tiêu hủy (hủy muộn)."
                        : "Đã hủy đơn hàng. Mã voucher đã được hoàn lại cho khách.";
                }
                else
                {
                    TempData["Error"] = "Không thể hủy đơn hàng. Vui lòng thử lại.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi hủy đơn hàng.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("xoa-nhieu")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "Không có đơn hàng nào được chọn." });

            int successCount = 0;
            int failCount = 0;

            foreach (var id in ids)
            {
                try
                {
                    var result = await _donHangService.DeleteAsync(id);
                    if (result) successCount++;
                    else failCount++;
                }
                catch
                {
                    failCount++;
                }
            }

            if (failCount == 0)
                return Json(new { success = true, message = $"Đã xóa thành công {successCount} đơn hàng." });

            return Json(new { success = true, message = $"Xóa thành công {successCount}/{ids.Count} đơn. {failCount} đơn không thể xóa (trạng thái không cho phép)." });
        }
    }
}