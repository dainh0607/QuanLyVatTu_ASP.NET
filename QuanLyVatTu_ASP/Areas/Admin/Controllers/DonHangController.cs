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

        public DonHangController(IDonHangService donHangService, INhanVienService nhanVienService)
        {
            _donHangService = donHangService;
            _nhanVienService = nhanVienService;
        }

        private async Task LoadDropdownData(int? selectedKhachHangId = null, int? selectedNhanVienId = null)
        {
            var khList = await _donHangService.GetKhachHangLookupAsync();
            var nvList = await _donHangService.GetNhanVienLookupAsync();

            ViewBag.KhachHangList = new SelectList(khList, "ID", "HoTen", selectedKhachHangId);
            ViewBag.NhanVienList = new SelectList(nvList, "ID", "HoTen", selectedNhanVienId);
        }

        [HttpGet("", Name = "AdminDonHang")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            var model = await _donHangService.GetAllPagingAsync(keyword, page, 15);
            ViewBag.Keyword = keyword;
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
                var success = await _donHangService.UpdateAsync(id, model);
                var donHangGoc = await _donHangService.GetByIdForEditAsync(id);
                if (donHangGoc == null) return NotFound();
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
            await _donHangService.DeleteAsync(id);
            TempData["Success"] = "Đã xóa đơn hàng";
            return RedirectToAction(nameof(Index));
        }
    }
}