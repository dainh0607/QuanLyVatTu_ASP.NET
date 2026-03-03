using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Models.ViewModels;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Repositories.Implementations;
using QuanLyVatTu_ASP.Repositories.Interfaces;
using QuanLyVatTu_ASP.Models;
using QuanLyVatTu_ASP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace QuanLyVatTu_ASP.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DataAccess.AppDbContext _context;
        private readonly IVoucherService _voucherService;
        private readonly IDiemTichLuyService _diemService;
        private readonly IDonHangService _donHangService;
        private readonly IThongBaoService _thongBaoService;

        public CustomerController(IUnitOfWork unitOfWork, DataAccess.AppDbContext context,
            IVoucherService voucherService, IDiemTichLuyService diemService, IDonHangService donHangService, IThongBaoService thongBaoService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _voucherService = voucherService;
            _diemService = diemService;
            _donHangService = donHangService;
            _thongBaoService = thongBaoService;
        }
        public async Task<IActionResult> Profile()
        {
            // Kiểm tra đăng nhập
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để xem thông tin tài khoản.";
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin khách hàng từ database
            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tài khoản.";
                return RedirectToAction("Login", "Account");
            }

            // Lấy đơn hàng của khách hàng
            var donHangs = await _unitOfWork.DonHangRepository.GetDonHangByKhachHangAsync(khachHang.ID);

            var viewModel = new ProfileViewModel
            {
                KhachHang = khachHang,
                DonHangs = donHangs,
                SoSanPhamDaMua = donHangs?.Sum(d => d.ChiTietDonHangs?.Sum(ct => ct.SoLuong) ?? 0) ?? 0
            };

            return View(viewModel);
        }
        public async Task<IActionResult> History()
        {
            // Kiểm tra đăng nhập
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để xem lịch sử mua hàng.";
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin khách hàng
            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tài khoản.";
                return RedirectToAction("Login", "Account");
            }

            var orders = await _unitOfWork.DonHangRepository.GetDonHangByKhachHangAsync(khachHang.ID);

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            // 1. Validate Model Validation (Data Annotations)
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            // 2. Security Check: Ensure User is modifying their own account
            var sessionEmail = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(sessionEmail))
            {
                 return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
            }

            var khachHang = await _context.KhachHangs.FindAsync(model.Id);
            if (khachHang == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khách hàng" });
            }

            if (khachHang.Email != sessionEmail)
            {
                 return Json(new { success = false, message = "Bạn không có quyền thay đổi mật khẩu này." });
            }

            // 3. Verify old password
            bool isPasswordCorrect = false;
            try {
                if (!string.IsNullOrEmpty(khachHang.MatKhau))
                {
                    isPasswordCorrect = BCrypt.Net.BCrypt.Verify(model.MatKhauCu, khachHang.MatKhau);
                }
            } catch {
                // Fallback for legacy plain text passwords
                if (khachHang.MatKhau == model.MatKhauCu) isPasswordCorrect = true;
            }

            // Fallback explicit check
            if (!isPasswordCorrect && khachHang.MatKhau == model.MatKhauCu) isPasswordCorrect = true;

            if (!isPasswordCorrect)
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không chính xác" });
            }

            // 4. Update Password (Directly on Context, similar to Register)
            khachHang.MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhauMoi);
            
            _context.KhachHangs.Update(khachHang);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Đổi mật khẩu thành công"
            });
        }

        /// <summary>
        /// Cập nhật thông tin hồ sơ khách hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request, [FromServices] IWebHostEnvironment _webHostEnvironment)
        {
            try
            {
                // Lấy khách hàng theo ID hoặc email từ session
                KhachHang? khachHang = null;
                
                if (request.Id > 0)
                {
                    khachHang = await _unitOfWork.KhachHangRepository.GetByIdAsync(request.Id);
                }
                else
                {
                    var email = HttpContext.Session.GetString("Email");
                    if (!string.IsNullOrEmpty(email))
                    {
                        khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
                    }
                }

                if (khachHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng" });
                }

                // Cập nhật thông tin
                if (!string.IsNullOrWhiteSpace(request.HoTen))
                    khachHang.HoTen = request.HoTen.Trim();
                
                if (!string.IsNullOrWhiteSpace(request.SoDienThoai))
                    khachHang.SoDienThoai = request.SoDienThoai.Trim();
                
                if (!string.IsNullOrWhiteSpace(request.SoNhaTenDuong))
                    khachHang.SoNhaTenDuong = request.SoNhaTenDuong.Trim();
                if (!string.IsNullOrWhiteSpace(request.PhuongXa))
                    khachHang.PhuongXa = request.PhuongXa.Trim();
                if (!string.IsNullOrWhiteSpace(request.TinhThanhPho))
                    khachHang.TinhThanhPho = request.TinhThanhPho.Trim();

                // Xử lý upload ảnh
                if (request.AnhDaiDienFile != null)
                {
                    // Xóa ảnh cũ nếu có (logic này nên check kỹ hơn để tránh xóa default placeholder)
                    // ...

                    // Upload ảnh mới
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + request.AnhDaiDienFile.FileName;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/khachhang");
                    
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.AnhDaiDienFile.CopyToAsync(fileStream);
                    }

                    // Lưu đường dẫn vào DB (relative path)
                    khachHang.AnhDaiDien = "/images/khachhang/" + uniqueFileName;
                }

                await _unitOfWork.KhachHangRepository.UpdateAsync(khachHang);

                // Cập nhật session username nếu đổi tên
                if (!string.IsNullOrWhiteSpace(request.HoTen))
                {
                    HttpContext.Session.SetString("UserName", request.HoTen.Trim());
                }
                
                // Cập nhật session avatar nếu đổi ảnh
                if (!string.IsNullOrEmpty(khachHang.AnhDaiDien))
                {
                    HttpContext.Session.SetString("AvatarUrl", khachHang.AnhDaiDien);
                }

                return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        public IActionResult About()
        {

            return View();
        }
        public IActionResult PurchasePolicy()
        {

            return View();
        }

        /// <summary>
        /// Tra cứu đơn hàng theo mã
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TrackOrder(int? id)
        {
            if (!id.HasValue)
            {
                return View();
            }

            var donHang = await _unitOfWork.DonHangRepository.GetDonHangByIdAsync(id.Value);
            
            if (donHang == null)
            {
                ViewBag.Error = "Không tìm thấy đơn hàng với mã này";
                return View();
            }

            return View(donHang);
        }



        /// <summary>
        /// Hiển thị danh sách đơn hàng của khách hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            // Kiểm tra đăng nhập
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin khách hàng
            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách đơn hàng
            var donHangs = await _unitOfWork.DonHangRepository.GetDonHangByKhachHangAsync(khachHang.ID);
            
            ViewBag.KhachHang = khachHang;
            return View(donHangs);
        }
        [HttpGet]
        public async Task<IActionResult> GetAddresses()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return NotFound();

            var addresses = await _context.DiaChiNhanHangs
                .Where(x => x.KhachHangId == khachHang.ID)
                .OrderByDescending(x => x.MacDinh)
                .ThenByDescending(x => x.ID)
                .Select(x => new AddressViewModel
                {
                    Id = x.ID,
                    HoTen = x.HoTen,
                    SoDienThoai = x.SoDienThoai,
                    DiaChi = x.DiaChi,
                    LoaiDiaChi = x.LoaiDiaChi,
                    MacDinh = x.MacDinh,
                    KinhDo = x.KinhDo,
                    ViDo = x.ViDo
                })
                .ToListAsync();

            return Json(new { success = true, data = addresses });
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody] AddressViewModel model)
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return NotFound();

            // Validate
            if (string.IsNullOrEmpty(model.DiaChi)) return Json(new { success = false, message = "Vui lòng nhập địa chỉ" });

            try 
            {
                // Logic: If Default is true, update Customer's Address field and unset other defaults
                if (model.MacDinh)
                {
                    var existingDefaults = await _context.DiaChiNhanHangs
                        .Where(x => x.KhachHangId == khachHang.ID && x.MacDinh)
                        .ToListAsync();
                    
                    foreach(var item in existingDefaults)
                    {
                        item.MacDinh = false;
                    }
                    
                    // Địa chỉ mặc định thay đổi → không cần sync sang profile nữa (đã tách 3 trường)
                    _context.KhachHangs.Update(khachHang);
                }

                var newAddress = new DiaChiNhanHang
                {
                    KhachHangId = khachHang.ID,
                    HoTen = model.HoTen ?? khachHang.HoTen,
                    SoDienThoai = model.SoDienThoai ?? khachHang.SoDienThoai ?? "",
                    DiaChi = model.DiaChi,
                    KinhDo = model.KinhDo,
                    ViDo = model.ViDo,
                    LoaiDiaChi = model.LoaiDiaChi,
                    MacDinh = model.MacDinh
                };
                
                // If this is the first address, make it default automatically?
                var count = await _context.DiaChiNhanHangs.CountAsync(x => x.KhachHangId == khachHang.ID);
                if (count == 0)
                {
                    newAddress.MacDinh = true;
                    // DiaChi profile đã tách thành 3 trường, không cần sync
                }

                _context.DiaChiNhanHangs.Add(newAddress);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm địa chỉ thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return NotFound();

            var address = await _context.DiaChiNhanHangs
                .FirstOrDefaultAsync(x => x.ID == id && x.KhachHangId == khachHang.ID);
            
            if (address == null) return Json(new { success = false, message = "Không tìm thấy địa chỉ" });

            // Unset old defaults
            var oldDefaults = await _context.DiaChiNhanHangs
                .Where(x => x.KhachHangId == khachHang.ID && x.MacDinh)
                .ToListAsync();
            foreach(var item in oldDefaults) item.MacDinh = false;

            // Set new default
            address.MacDinh = true;
            
            // Profile address đã tách 3 trường riêng, không cần sync
            _context.KhachHangs.Update(khachHang);

            await _context.SaveChangesAsync();

            return Json(new { success = true, newAddress = address.DiaChi });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return NotFound();

            var address = await _context.DiaChiNhanHangs
                .FirstOrDefaultAsync(x => x.ID == id && x.KhachHangId == khachHang.ID);
            
            if (address == null) return Json(new { success = false, message = "Không tìm thấy địa chỉ" });
            
            _context.DiaChiNhanHangs.Remove(address);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa địa chỉ" });
        }


        [HttpGet]
        public async Task<IActionResult> GetVATInvoices()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return NotFound();

            try
            {
                var rawInvoices = await _context.HoaDons
                    .Where(h => h.MaKhachHang == khachHang.ID && h.IsVATInvoice)
                    .OrderByDescending(h => h.NgayLap)
                    .Select(h => new
                    {
                        h.ID,
                        SoHoaDon = h.SoHoaDonVAT,
                        h.TenCongTy,
                        h.MaSoThue,
                        h.DiaChiDKKD,
                        h.EmailNhanHoaDon,
                        h.TongTienTruocThue,
                        h.ThueSuat,
                        TienThue = h.TienThueGTGT,
                        h.TongTienSauThue,
                        h.NgayLap,
                        h.TrangThai,
                        MaDonHang = h.DonHang != null ? h.DonHang.MaHienThi : ""
                    })
                    .ToListAsync();

                // Format dates client-side (ToString with format can't be translated to SQL)
                var invoices = rawInvoices.Select(h => new
                {
                    h.ID,
                    h.SoHoaDon,
                    h.TenCongTy,
                    h.MaSoThue,
                    h.DiaChiDKKD,
                    h.EmailNhanHoaDon,
                    h.TongTienTruocThue,
                    h.ThueSuat,
                    h.TienThue,
                    h.TongTienSauThue,
                    NgayLap = h.NgayLap.ToString("dd/MM/yyyy"),
                    h.TrangThai,
                    h.MaDonHang
                }).ToList();

                return Json(new { success = true, data = invoices });
            }
            catch
            {
                // Table may not exist yet (migration not applied)
                return Json(new { success = true, data = new List<object>() });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetDebtSummary()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return NotFound();

            var debtOrders = await _context.DonHang
                .Where(d => d.KhachHangId == khachHang.ID &&
                        (d.TrangThai == "Chờ thanh toán" || d.TrangThai == "Chờ đặt cọc"))
                .Select(d => new
                {
                    d.ID,
                    d.MaHienThi,
                    d.NgayDat,
                    d.TongTien,
                    d.TrangThai,
                    d.GhiChu
                })
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            var creditLimit = 50000000;
            var totalDebt = debtOrders.Sum(x => x.TongTien ?? 0);

            var formattedOrders = debtOrders.Select(d => new
            {
                d.ID,
                d.MaHienThi,
                NgayDat = d.NgayDat.ToString("dd/MM/yyyy"),
                d.TongTien,
                d.TrangThai,
                d.GhiChu
            });

            return Json(new { success = true, totalDebt, creditLimit, orders = formattedOrders });
        }

        // --- RFQ / Yêu Cầu Báo Giá Actions ---

        [HttpGet]
        public async Task<IActionResult> GetQuoteRequests()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return NotFound();

            try
            {
                var requests = await _context.YeuCauBaoGia
                    .Where(r => r.KhachHangId == khachHang.ID)
                    .OrderByDescending(r => r.NgayTao)
                    .Select(r => new {
                        r.ID,
                        r.MaHienThi,
                        NgayTao = r.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                        r.TrangThai,
                        r.TongTienDuKien,
                        ItemCount = r.ChiTietYeuCauBaoGias.Count()
                    })
                    .ToListAsync();

                return Json(new { success = true, data = requests });
            }
            catch
            {
                // Table might not exist yet
                return Json(new { success = true, data = new List<object>() });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuoteRequest([FromServices] IGioHangRepository _gioHangRepo)
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Json(new { success = false, message = "Vui lòng đăng nhập." });

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return Json(new { success = false, message = "Lỗi tài khoản." });

            // Get Cart (DB only since user is logged in)
            var gioHang = await _gioHangRepo.GetByKhachHangIdAsync(khachHang.ID);
            if (gioHang == null || gioHang.ChiTietGioHangs == null || !gioHang.ChiTietGioHangs.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng trống. Vui lòng thêm sản phẩm." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create RFQ Header
                var rfq = new YeuCauBaoGia
                {
                    KhachHangId = khachHang.ID,
                    NgayTao = DateTime.Now,
                    TrangThai = "Mới",
                    MaHienThi = "RFQ" + DateTime.Now.ToString("yyMMdd") + new Random().Next(1000, 9999),
                    TongTienDuKien = 0
                };
                
                _context.YeuCauBaoGia.Add(rfq);
                await _context.SaveChangesAsync();

                decimal total = 0;
                foreach(var item in gioHang.ChiTietGioHangs)
                {
                    var detail = new ChiTietYeuCauBaoGia
                    {
                        YeuCauBaoGiaId = rfq.ID,
                        VatTuId = item.MaVatTu,
                        SoLuong = item.SoLuong,
                        DonGiaDuKien = item.VatTu?.GiaBan ?? 0
                    };
                    _context.ChiTietYeuCauBaoGia.Add(detail);
                    total += (detail.DonGiaDuKien ?? 0) * detail.SoLuong;
                }

                rfq.TongTienDuKien = total;
                await _context.SaveChangesAsync();

                // Clear Cart? 
                // Requirement doesn't specify, but usually "Request Quote" implies converting cart to request.
                // Let's clear it to avoid confusion, or keep it? 
                // Let's KEEP it for now, as user might want to buy some items directly too.
                // Or maybe ask user? For API simplicity, we just create the request.
                
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Đã gửi yêu cầu báo giá thành công!", rfqId = rfq.ID });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelQuoteRequest(int id)
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            
            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return NotFound();

            var rfq = await _context.YeuCauBaoGia.FirstOrDefaultAsync(r => r.ID == id && r.KhachHangId == khachHang.ID);
            if (rfq == null) return Json(new { success = false, message = "Không tìm thấy yêu cầu." });

            if (rfq.TrangThai != "Mới")
                return Json(new { success = false, message = "Chỉ có thể hủy yêu cầu mới." });

            rfq.TrangThai = "Đã hủy";
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã hủy yêu cầu báo giá." });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> EditOrder([FromBody] int orderId)
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Json(new { success = false, message = "Vui lòng đăng nhập." });

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.ID == orderId && d.KhachHangId == khachHang.ID);

            if (donHang == null)
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });

            // Only allow edit for active orders
            var allowedStatuses = new[] { "Chờ xác nhận", "Đã xác nhận", "Đang xử lý", "Chờ thanh toán", "Chờ đặt cọc", "Công nợ" };
            if (!allowedStatuses.Contains(donHang.TrangThai))
                return Json(new { success = false, message = "Không thể chỉnh sửa đơn hàng đã hoàn thành hoặc đã hủy." });

            // --- NEW LOGIC: Copy items to Cart ---
            var gioHang = await _unitOfWork.GioHangRepository.GetByKhachHangIdAsync(khachHang.ID);
            if (gioHang == null)
            {
                gioHang = new GioHang { MaKhachHang = khachHang.ID };
                await _unitOfWork.GioHangRepository.AddAsync(gioHang);
                _unitOfWork.Save();
            }

            if (donHang.ChiTietDonHangs != null)
            {
                foreach (var detail in donHang.ChiTietDonHangs)
                {
                    var vatTu = await _unitOfWork.VatTuRepository.GetByIdAsync(detail.MaVatTu);
                    if (vatTu == null) continue;

                    var existingItem = gioHang.ChiTietGioHangs?.FirstOrDefault(x => x.MaVatTu == detail.MaVatTu);
                    if (existingItem != null)
                    {
                         existingItem.SoLuong += detail.SoLuong ?? 0;
                         _unitOfWork.ChiTietGioHangRepository.Update(existingItem);
                    }
                    else
                    {
                        var newItem = new ChiTietGioHang
                        {
                            MaGioHang = gioHang.ID,
                            MaVatTu = detail.MaVatTu,
                            SoLuong = detail.SoLuong ?? 1
                        };
                        await _unitOfWork.ChiTietGioHangRepository.AddAsync(newItem);
                    }
                }
            }
            
            HttpContext.Session.SetInt32("EditingOrderId", orderId);

            if (donHang.ChiTietDonHangs != null && donHang.ChiTietDonHangs.Any())
            {
                _context.Set<ChiTietDonHang>().RemoveRange(donHang.ChiTietDonHangs);
            }

            donHang.TrangThai = "Chờ xác nhận";
            donHang.TongTien = 0;
            donHang.SoTienDatCoc = null;
            donHang.PhuongThucDatCoc = null;
            donHang.NgayDatCoc = null;
            donHang.GhiChu = null;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đang chuyển đến giỏ hàng để chỉnh sửa..." });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CancelOrder([FromForm] int orderId)
        {
            var email = HttpContext.Session.GetString("Email");
            string referer = Request.Headers["Referer"].ToString() ?? "/Customer/Orders";

            if (string.IsNullOrEmpty(email)) 
            {
                TempData["Error"] = "Vui lòng đăng nhập.";
                return Redirect(referer);
            }

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) 
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return Redirect(referer);
            }

            var donHang = await _context.DonHang.FirstOrDefaultAsync(d => d.ID == orderId && d.KhachHangId == khachHang.ID);
            if (donHang == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return Redirect(referer);
            }

            // Only allow cancellation for early statuses
            // Khách hàng chỉ được hủy sớm → mã voucher sẽ được HOÀN LẠI (REFUNDED)
            // Đơn ở "Đang xử lý" trở đi chỉ Admin mới được hủy muộn (phạt mã BURNED)
            var allowedStatuses = new[] { "Chờ xác nhận", "Đã xác nhận", "Chờ đặt cọc", "Chờ thanh toán", "Chờ xử lý" };
            if (!allowedStatuses.Any(s => s.Equals(donHang.TrangThai, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["Error"] = "Không thể hủy đơn hàng ở trạng thái này. Nếu cần hủy đơn đang xử lý, vui lòng liên hệ với chúng tôi.";
                return Redirect(referer);
            }

            // Get ViewModel to update
            var updateModel = await _donHangService.GetByIdForEditAsync(orderId);
            if (updateModel == null) 
            {
                 TempData["Error"] = "Không tìm thấy dữ liệu để hủy đơn.";
                 return Redirect(referer);
            }

            updateModel.TrangThai = "Đã hủy";
            var result = await _donHangService.UpdateAsync(orderId, updateModel);
            
            if (result)
            {
                TempData["Success"] = "Đã hủy đơn hàng thành công! Mã voucher (nếu có) đã được hoàn lại vào ví.";
            }
            else
            {
                TempData["Error"] = "Đã xảy ra lỗi khi hủy đơn.";
            }

            return Redirect(referer);
        }

        // ==========================================
        // VOUCHER WALLET APIs
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetVoucherWallet()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return NotFound();

            var wallet = await _voucherService.GetWalletAsync(khachHang.ID);
            var data = wallet.Select(v => new
            {
                v.ID,
                v.MaVoucherGoc,
                MaVoucher = v.VoucherGoc?.MaVoucher ?? "",
                LoaiGiamGia = v.VoucherGoc?.LoaiGiamGia ?? "",
                GiaTriGiam = v.VoucherGoc?.GiaTriGiam ?? 0,
                SoTienGiamToiDa = v.VoucherGoc?.SoTienGiamToiDa,
                GiaTriDonHangToiThieu = v.VoucherGoc?.GiaTriDonHangToiThieu ?? 0,
                ThoiGianKetThuc = v.VoucherGoc?.ThoiGianKetThuc.ToString("dd/MM/yyyy HH:mm"),
                v.TrangThaiTrongVi,
                v.ThoiGianLuuMa,
                SoLuongDaDung = v.VoucherGoc?.SoLuongDaDung ?? 0,
                TongSoLuong = v.VoucherGoc?.TongSoLuong ?? 0,
                ConHan = v.VoucherGoc != null && v.VoucherGoc.ThoiGianKetThuc > DateTime.Now,
                SapHetHan = v.VoucherGoc != null && v.VoucherGoc.ThoiGianKetThuc > DateTime.Now && Math.Ceiling((v.VoucherGoc.ThoiGianKetThuc - DateTime.Now).TotalDays) <= 3,
                DaHetLuot = v.VoucherGoc != null && v.VoucherGoc.SoLuongDaDung >= v.VoucherGoc.TongSoLuong
            });

            return Json(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableVouchers()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return NotFound();

            var activeVouchers = await _unitOfWork.VoucherRepository.GetActiveVouchersAsync();
            var data = new List<object>();
            foreach (var v in activeVouchers)
            {
                var alreadySaved = await _unitOfWork.ViVoucherRepository.ExistsAsync(khachHang.ID, v.ID);
                data.Add(new
                {
                    v.ID,
                    v.MaVoucher,
                    v.LoaiGiamGia,
                    v.GiaTriGiam,
                    v.SoTienGiamToiDa,
                    v.GiaTriDonHangToiThieu,
                    ThoiGianKetThuc = v.ThoiGianKetThuc.ToString("dd/MM/yyyy HH:mm"),
                    v.SoLuongDaDung,
                    v.TongSoLuong,
                    DaLuu = alreadySaved,
                    ConHan = v.ThoiGianKetThuc > DateTime.Now,
                    SapHetHan = v.ThoiGianKetThuc > DateTime.Now && Math.Ceiling((v.ThoiGianKetThuc - DateTime.Now).TotalDays) <= 3
                });
            }

            return Json(new { success = true, data });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveVoucher([FromBody] int voucherId)
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Json(new { success = false, message = "Vui lòng đăng nhập." });

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            var result = await _voucherService.SaveVoucherToWalletAsync(khachHang.ID, voucherId);
            return Json(new { result.Success, result.Message });
        }

        // ==========================================
        // POINTS HISTORY APIs
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetPointsHistory()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return NotFound();

            // Ensure tier is evaluated correctly before fetching
            await _diemService.EvaluateTierUpgradeAsync(khachHang.ID);
            
            // Reload user tier stats
            khachHang = await _unitOfWork.KhachHangRepository.GetByIdAsync(khachHang.ID);

            // Load tier info
            var tier = khachHang!.MaHangThanhVien.HasValue
                ? await _unitOfWork.HangThanhVienRepository.GetByIdAsync(khachHang.MaHangThanhVien.Value)
                : null;

            // Get next tier
            var allTiers = await _unitOfWork.HangThanhVienRepository.GetAllOrderedAsync();
            
            // Calculate 365-day spending first so we can use it to determine the exact next tier
            var oneYearAgo = DateTime.Now.AddDays(-365);
            var totalSpent = await _context.DonHang
                .Where(d => d.KhachHangId == khachHang.ID
                         && d.TrangThai == "Hoàn thành"
                         && d.NgayDat >= oneYearAgo)
                .SumAsync(d => d.TongTienThucTra ?? d.TongTien ?? 0);

            var minRequired = Math.Max(totalSpent, tier?.ChiTieuToiThieu ?? 0);
            var nextTier = allTiers.FirstOrDefault(t => t.ChiTieuToiThieu > minRequired);

            // Already calculated above

            var history = await _diemService.GetHistoryAsync(khachHang.ID);
            var historyData = history.Select(h => new
            {
                h.ID,
                h.SoDiem,
                h.LoaiGiaoDich,
                NgayTao = h.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                MaDonHang = h.MaDonHang
            });

            return Json(new
            {
                success = true,
                diemHienTai = khachHang.DiemTichLuy,
                hangHienTai = tier?.TenHang ?? "Chưa có hạng",
                phanTramChietKhau = tier?.PhanTramChietKhau ?? 0,
                ngayHetHanHang = khachHang.NgayHetHanHang?.ToString("dd/MM/yyyy"),
                hangTiepTheo = nextTier?.TenHang,
                chiTieuCanThiet = nextTier?.ChiTieuToiThieu ?? 0,
                tongChiTieu365Ngay = totalSpent,
                history = historyData
            });
        }

        // ==========================================
        //         SYSTEM NOTIFICATIONS APIs
        // ==========================================

        [HttpGet("Customer/GetNotifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            var notifications = await _thongBaoService.GetUserNotificationsAsync(khachHangId, 20);
            var unreadCount = await _thongBaoService.GetUnreadCountAsync(khachHangId);

            var result = notifications.Select(n => new
            {
                n.ID,
                n.TieuDe,
                n.NoiDung,
                n.LoaiThongBao,
                n.LinkDich,
                n.DaDoc,
                NgayTao = n.NgayTao.ToString("dd/MM/yyyy HH:mm")
            }).ToList();

            return Json(new { success = true, data = result, unreadCount });
        }

        [HttpPost("Customer/MarkNotificationAsRead/{id}")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            await _thongBaoService.MarkAsReadAsync(id, khachHangId);
            return Json(new { success = true });
        }

        [HttpPost("Customer/MarkAllNotificationsAsRead")]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId.HasValue)
            {
                await _thongBaoService.MarkAllAsReadAsync(khachHangId.Value);
            }
            return Json(new { success = true });
        }

        [HttpDelete("Customer/DeleteNotification/{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            await _thongBaoService.DeleteNotificationAsync(id, khachHangId);
            return Json(new { success = true });
        }

        // ==========================================
        // SETTINGS & PRIVACY APIs
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetNotificationSettings()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return NotFound();

            return Json(new { 
                success = true,
                donHang = khachHang.NhanThongBaoDonHang,
                khuyenMai = khachHang.NhanThongBaoKhuyenMai,
                hangThanhVien = khachHang.NhanThongBaoHangThanhVien
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] NotificationSettingsModel model)
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) return NotFound();

            khachHang.NhanThongBaoDonHang = model.DonHang;
            khachHang.NhanThongBaoKhuyenMai = model.KhuyenMai;
            khachHang.NhanThongBaoHangThanhVien = model.HangThanhVien;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã lưu cài đặt thông báo" });
        }

        [HttpGet]
        public async Task<IActionResult> ExportPersonalData()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var khachHang = await _context.KhachHangs
                .Include(k => k.LichSuTichDiems)
                .Include(k => k.ViVoucherKhachHangs)
                    .ThenInclude(v => v.VoucherGoc)
                .FirstOrDefaultAsync(k => k.Email == email);

            if (khachHang == null) return NotFound();

            var orders = await _context.DonHang.Where(d => d.KhachHangId == khachHang.ID).ToListAsync();

            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);

                if (System.IO.File.Exists(fontPath))
                {
                    BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    normalFont = new Font(bf, 12);
                    boldFont = new Font(bf, 14, Font.BOLD);
                }

                doc.Add(new Paragraph("THONG TIN CA NHAN - PERSONAL DATA", boldFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("\n"));

                doc.Add(new Paragraph($"Ho ten: {khachHang.HoTen}", normalFont));
                doc.Add(new Paragraph($"Email: {khachHang.Email}", normalFont));
                doc.Add(new Paragraph($"So dien thoai: {khachHang.SoDienThoai ?? "N/A"}", normalFont));
                var diaChiStr = string.Join(", ", new[] { khachHang.SoNhaTenDuong, khachHang.PhuongXa, khachHang.TinhThanhPho }.Where(s => !string.IsNullOrWhiteSpace(s)));
                doc.Add(new Paragraph($"Dia chi: {(string.IsNullOrEmpty(diaChiStr) ? "N/A" : diaChiStr)}", normalFont));
                doc.Add(new Paragraph($"Diem tich luy: {khachHang.DiemTichLuy}", normalFont));
                doc.Add(new Paragraph($"Ngay tao tai khoan: {khachHang.NgayTao:dd/MM/yyyy}", normalFont));
                doc.Add(new Paragraph("\n"));

                doc.Add(new Paragraph("LICH SU DON HANG", boldFont));
                foreach(var o in orders)
                {
                    var sum = o.TongTienThucTra ?? o.TongTien ?? 0;
                    doc.Add(new Paragraph($"- Don {o.MaHienThi} ({o.TrangThai}): {sum:N0} VND (Ngay {o.NgayDat:dd/MM/yyyy})", normalFont));
                }
                
                doc.Add(new Paragraph("\n"));
                doc.Add(new Paragraph("LICH SU TICH DIEM", boldFont));
                foreach(var p in khachHang.LichSuTichDiems)
                {
                    doc.Add(new Paragraph($"- {p.LoaiGiaoDich} {p.SoDiem} diem (Ngay {p.NgayTao:dd/MM/yyyy})", normalFont));
                }

                doc.Add(new Paragraph("\n"));
                doc.Add(new Paragraph("VOUCHER CUA BAN", boldFont));
                foreach(var v in khachHang.ViVoucherKhachHangs)
                {
                    doc.Add(new Paragraph($"- Voucher {v.VoucherGoc?.MaVoucher} ({v.TrangThaiTrongVi}) (Ngay {v.ThoiGianLuuMa:dd/MM/yyyy})", normalFont));
                }

                doc.Close();
                return File(ms.ToArray(), "application/pdf", $"DuLieuCaNhan_{khachHang.MaHienThi}.pdf");
            }
        }
    }

    public class NotificationSettingsModel
    {
        public bool DonHang { get; set; }
        public bool KhuyenMai { get; set; }
        public bool HangThanhVien { get; set; }
    }
}