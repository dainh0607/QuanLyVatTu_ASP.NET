using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Models.ViewModels;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Repositories.Implementations;
using QuanLyVatTu_ASP.Repositories.Interfaces;
using QuanLyVatTu_ASP.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyVatTu_ASP.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DataAccess.AppDbContext _context;

        public CustomerController(IUnitOfWork unitOfWork, DataAccess.AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
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
                
                if (!string.IsNullOrWhiteSpace(request.DiaChi))
                    khachHang.DiaChi = request.DiaChi.Trim();

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

            var donHang = await _unitOfWork.DonHangRepository.GetByIdAsync(id.Value);
            
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
                    
                    // Update main profile address
                    khachHang.DiaChi = model.DiaChi;
                    // Also update phone/name if user desires? (Requirement says "Textbox address in profile also changes")
                    // Let's assume just updating the DiaChi field is enough based on requirement.
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
                    khachHang.DiaChi = model.DiaChi; // Sync to profile
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
            
            // Update Profile Address
            khachHang.DiaChi = address.DiaChi;
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

    }
}