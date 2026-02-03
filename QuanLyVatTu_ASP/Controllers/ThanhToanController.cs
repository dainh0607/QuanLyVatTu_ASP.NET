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

        public ThanhToanController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            
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
            ViewBag.Total = cart.Sum(x => x.ThanhTien);

            return View(khachHang);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout(
            string diaChi, 
            string soDienThoai, 
            string ghiChu,
            string paymentMethod = "cod",
            string shippingMethod = "delivery")
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY);
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
                    var vatTu = await _unitOfWork.VatTuRepository.GetByIdAsync(item.VatTuId);

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
                // Tính toán cọc nếu đơn hàng > 5.000.000 VÀ không phải là COD (khách chọn COD thì không bắt cọc)
                if (tongTien > 5000000 && paymentMethod != "cod")
                {
                    donHang.SoTienDatCoc = tongTien * 0.1m; // 10%
                    donHang.TrangThai = "Chờ đặt cọc";
                    donHang.PhuongThucDatCoc = "Chuyển khoản (QR)";
                    donHang.GhiChu += " | Đơn hàng cần cọc 10%.";
                }

                donHang.TongTien = tongTien;
                _unitOfWork.DonHangRepository.Update(donHang); 
                _unitOfWork.Save();

                // Lưu thông tin vào TempData để hiển thị ở trang success
                // Serialize DonHang to JSON to avoid DefaultTempDataSerializer issues with EF entities
                TempData["DonHangJson"] = Newtonsoft.Json.JsonConvert.SerializeObject(donHang, new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
                
                // TempData["DonHang"] = donHang; // REMOVED: Causes InvalidOperationException
                TempData["CartItemsJson"] = Newtonsoft.Json.JsonConvert.SerializeObject(cart); // Also serialize cart list just in case
                TempData["CartItems"] = null; // Clear old key if present to avoid confusion
                TempData["TongTien"] = tongTien.ToString();
                TempData["PaymentMethod"] = paymentMethod;
                TempData["ShippingMethod"] = shippingMethod;
                TempData["HoTen"] = khachHang.HoTen;
                TempData["SoDienThoai"] = soDienThoai;
                TempData["MaHienThi"] = donHang.MaHienThi;
                TempData["SoTienDatCoc"] = donHang.SoTienDatCoc.ToString(); // Pass deposit amount as string

                HttpContext.Session.Remove(CART_KEY);

                // Redirect logic
                if (donHang.SoTienDatCoc > 0)
                {
                    // Redirect to QR page specifically for deposit
                    return RedirectToAction("OrderSuccessQR"); 
                }
                else if (paymentMethod == "bank")
                {
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
    }
}
