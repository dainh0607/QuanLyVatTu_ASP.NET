using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Extensions;
using QuanLyVatTu_ASP.Models.ViewModels;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private const string CART_KEY = "MY_CART";
        private const string DIRECT_CART_KEY = "DIRECT_CART";

        public ThanhToanController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private async Task<List<CartItem>> GetCartItemsSecureAsync(bool isBuyNow)
        {
            if (isBuyNow)
            {
                return HttpContext.Session.Get<List<CartItem>>(DIRECT_CART_KEY) ?? new List<CartItem>();
            }

            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId != null)
            {
                var gioHang = await _unitOfWork.GioHangRepository.GetByKhachHangIdAsync(khachHangId.Value);
                if (gioHang == null || gioHang.ChiTietGioHangs == null) return new List<CartItem>();
                
                return gioHang.ChiTietGioHangs.Select(ct => new CartItem
                {
                    VatTuId = ct.MaVatTu,
                    TenVatTu = ct.VatTu?.TenVatTu,
                    DonGia = ct.VatTu?.GiaBan ?? 0,
                    SoLuong = ct.SoLuong,
                    DonViTinh = ct.VatTu?.DonViTinh,
                    HinhAnh = !string.IsNullOrEmpty(ct.VatTu?.HinhAnh) ? ct.VatTu.HinhAnh : $"https://placehold.co/120x120?text={Uri.EscapeDataString(ct.VatTu?.TenVatTu ?? "SP")}"
                }).ToList();
            }
            
            return HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(bool isBuyNow = false)
        {
            // Prevent caching of this sensitive page
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            var cart = await GetCartItemsSecureAsync(isBuyNow);
            
            // Nếu giỏ hàng trống, redirect về trang sản phẩm
            if (!cart.Any())
            {
                TempData["Warning"] = "Giỏ hàng của bạn đang trống. Vui lòng thêm sản phẩm trước khi thanh toán.";
                return RedirectToAction("Index", "SanPham");
            }

            // Kiểm tra đăng nhập
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để tiếp tục thanh toán.";
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin khách hàng từ database
            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tài khoản.";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Cart = cart;
            ViewBag.IsBuyNow = isBuyNow;
            ViewBag.Total = cart.Sum(x => x.ThanhTien);

            return View(khachHang);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout(
            string diaChi, 
            string soDienThoai, 
            string ghiChu,
            string paymentMethod = "cod",
            string shippingMethod = "delivery",
            bool isBuyNow = false)
        {
            var cart = await GetCartItemsSecureAsync(isBuyNow);
            if (cart == null || !cart.Any()) 
            {
                TempData["Warning"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "SanPham");
            }

            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Login", "Account");
            }

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null) 
            {
                TempData["Error"] = "Không tìm thấy thông tin tài khoản.";
                return RedirectToAction("Login", "Account");
            }

            decimal tongTien = 0;

            try
            {
                // 1. Tạo Đơn Hàng
                var donHang = new DonHang
                {
                    MaHienThi = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999), 
                    KhachHangId = khachHang.ID,
                    NgayDat = DateTime.Now,
                    TrangThai = paymentMethod == "bank" ? "Chờ thanh toán" : "Chờ xử lý",
                    GhiChu = ghiChu,
                    TongTien = 0,
                    PhuongThucDatCoc = paymentMethod == "bank" ? "Chuyển khoản" : "COD"
                };

                if (khachHang.DiaChi != diaChi || khachHang.SoDienThoai != soDienThoai)
                {
                    khachHang.DiaChi = diaChi;
                    khachHang.SoDienThoai = soDienThoai;
                    await _unitOfWork.KhachHangRepository.UpdateAsync(khachHang);
                }

                await _unitOfWork.DonHangRepository.AddAsync(donHang);
                // Save first to get DonHang ID if needed for FK (though EF usually handles it, sometimes needed for logic)
                _unitOfWork.Save();

                foreach (var item in cart)
                {
                    var vatTu = await _unitOfWork.VatTuRepository.GetByIdAsync(item.VatTuId, tracking: true);

                    if (vatTu == null || vatTu.SoLuongTon < item.SoLuong)
                    {
                        TempData["Error"] = vatTu == null 
                            ? $"Sản phẩm không tồn tại." 
                            : $"Sản phẩm {vatTu.TenVatTu} chỉ còn {vatTu.SoLuongTon} sản phẩm.";
                        return RedirectToAction("Checkout");
                    }

                    // Trừ tồn kho
                    vatTu.SoLuongTon -= item.SoLuong;
                    _unitOfWork.VatTuRepository.Update(vatTu);

                    var chiTiet = new ChiTietDonHang
                    {
                        MaDonHang = donHang.ID, // Corrected property name
                        MaVatTu = item.VatTuId,
                        SoLuong = item.SoLuong,
                        DonGia = item.DonGia,
                        ThanhTien = item.ThanhTien
                    };

                    tongTien += item.ThanhTien;
                    await _unitOfWork.ChiTietDonHangRepository.AddAsync(chiTiet);
                }
                if (tongTien >= 5000000)
                {
                    // Logic mới: Đơn hàng >= 5.000.000 bắt buộc cọc 10%
                    donHang.SoTienDatCoc = tongTien * 0.1m; // 10%
                    donHang.TrangThai = "Chờ đặt cọc";
                    donHang.PhuongThucDatCoc = "Chuyển khoản (QR)";
                    donHang.GhiChu += " | Đơn hàng >= 5tr, bắt buộc cọc 10%.";
                }
                else if (paymentMethod != "cod" && paymentMethod != "bank") // Logic cũ cho các mức cọc tùy chọn (nếu có - hiện tại logic mới override)
                {
                     // Giữ lại logic cũ nếu cần thiết, nhưng theo yêu cầu mới thì >= 5tr là fix cứng 10%.
                     // Nếu < 5tr mà user chọn cọc (nếu UI cho phép) thì xử lý ở đây. 
                     // Tuy nhiên UI sẽ được update để ẩn options.
                }

                donHang.TongTien = tongTien;
                _unitOfWork.DonHangRepository.Update(donHang); 
                _unitOfWork.Save();

                // Serialize DonHang to JSON using safe anonymous object to avoid EF Proxy issues
                var donHangSafe = new 
                {
                    ID = donHang.ID,
                    MaHienThi = donHang.MaHienThi,
                    TongTien = donHang.TongTien,
                    SoTienDatCoc = donHang.SoTienDatCoc,
                    TrangThai = donHang.TrangThai,
                    PhuongThucDatCoc = donHang.PhuongThucDatCoc
                };
                TempData["DonHangJson"] = Newtonsoft.Json.JsonConvert.SerializeObject(donHangSafe);
                
                // Safe cart serialization
                var cartSafe = cart.Select(c => new {
                    c.VatTuId, c.TenVatTu, c.DonGia, c.SoLuong, c.ThanhTien, c.HinhAnh
                }).ToList();
                TempData["CartItemsJson"] = Newtonsoft.Json.JsonConvert.SerializeObject(cartSafe);
                
                TempData["CartItems"] = null;
                TempData["TongTien"] = tongTien.ToString();
                TempData["PaymentMethod"] = paymentMethod;
                TempData["ShippingMethod"] = shippingMethod;
                TempData["HoTen"] = khachHang.HoTen;
                TempData["SoDienThoai"] = soDienThoai;
                TempData["MaHienThi"] = donHang.MaHienThi;
                TempData["SoTienDatCoc"] = donHang.SoTienDatCoc.ToString(); 

                // Clear Cart
                if (isBuyNow)
                {
                     HttpContext.Session.Remove(DIRECT_CART_KEY);
                }
                else
                {
                    // If DB cart, remove from DB
                    if (khachHang.ID > 0)
                    {
                        var gioHang = await _unitOfWork.GioHangRepository.GetByKhachHangIdAsync(khachHang.ID);
                        if (gioHang != null && gioHang.ChiTietGioHangs.Any())
                        {
                            // Fix: Use ToList() to prevent 'Collection was modified' error
                            var itemsToDelete = gioHang.ChiTietGioHangs.ToList();
                            foreach (var item in itemsToDelete)
                            {
                                _unitOfWork.ChiTietGioHangRepository.Delete(item);
                            }
                            _unitOfWork.Save();
                        }
                    }
                    // Also clear session just in case
                    HttpContext.Session.Remove(CART_KEY);
                }

                // Redirect logic
                if (donHang.SoTienDatCoc > 0 || paymentMethod == "bank")
                {
                    // Redirect to QR page specifically for deposit or full payment
                    return RedirectToAction("OrderSuccessQR"); 
                }
                
                return RedirectToAction("OrderSuccessThankYou");
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết ra TempData để debug
                TempData["Error"] = "Lỗi xử lý đơn hàng: " + ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "");
                return RedirectToAction("Checkout");
            }
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }

        public IActionResult OrderSuccessThankYou()
        {
            return View();
        }

        public IActionResult OrderSuccessQR()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SimulateTransfer(int orderId)
        {
            try
            {
                var donHang = await _unitOfWork.DonHangRepository.GetByIdAsync(orderId);
                if (donHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                // Cập nhật trạng thái thanh toán
                if (donHang.TrangThai == "Chờ đặt cọc")
                {
                    donHang.TrangThai = "Đã đặt cọc";
                    donHang.NgayDatCoc = DateTime.Now;
                    donHang.GhiChu += " | Đã thanh toán cọc (Giả lập).";
                }
                else if (donHang.TrangThai == "Chờ thanh toán")
                {
                     donHang.TrangThai = "Đã thanh toán";
                     donHang.GhiChu += " | Đã thanh toán (Giả lập).";
                }

                _unitOfWork.DonHangRepository.Update(donHang);
                _unitOfWork.Save();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                 return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
