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
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY);
            
            // DEMO DATA GENERATION IF CART IS EMPTY
            if (cart == null || !cart.Any())
            {
                cart = new List<CartItem>
                {
                    new CartItem { VatTuId = 101, TenVatTu = "Máy khoan Bosch GSB 550", DonGia = 1250000, SoLuong = 1, HinhAnh = "https://placehold.co/100x100/0d6efd/ffffff?text=Khoan" },
                    new CartItem { VatTuId = 102, TenVatTu = "Bộ mũi khoan đa năng", DonGia = 350000, SoLuong = 2, HinhAnh = "https://placehold.co/100x100/ffc107/ffffff?text=Mui+Khoan" },
                    new CartItem { VatTuId = 103, TenVatTu = "Găng tay bảo hộ 3M", DonGia = 45000, SoLuong = 5, HinhAnh = "https://placehold.co/100x100/198754/ffffff?text=Gang+Tay" }
                };
            }

            // MOCK USER DATA IF NOT LOGGED IN
            KhachHang? khachHang = null;
            var email = HttpContext.Session.GetString("Email");
            
            if (!string.IsNullOrEmpty(email))
            {
                 khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            }
            
            if (khachHang == null)
            {
                khachHang = new KhachHang 
                { 
                    HoTen = "Nguyễn Văn A", 
                    SoDienThoai = "0987654321", 
                    DiaChi = "123 Đường Số 1, Phường Bến Nghé, Quận 1, TP.HCM",
                    Email = "nguyenvana@example.com"
                };
            }

            ViewBag.Cart = cart;
            ViewBag.Total = cart.Sum(x => x.ThanhTien);

            return View(khachHang);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout(string diaChi, string soDienThoai, string ghiChu)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY);
            if (cart == null || !cart.Any()) return RedirectToAction("Index", "Home");

            var email = HttpContext.Session.GetString("Email");
            var khachHang = !string.IsNullOrEmpty(email) 
                ? await _unitOfWork.KhachHangRepository.GetByEmailAsync(email) 
                : null;

            decimal tongTien = 0;

            // TODO: Tạm thời vô hiệu hóa để test - cần bật lại sau
            // if (khachHang == null) return RedirectToAction("Login", "Account");
            
            // Mock user if not logged in
            if (khachHang == null)
            {
                khachHang = new KhachHang
                {
                    ID = 1,
                    HoTen = "Nguyễn Văn An",
                    Email = "nguyenvanan@example.com",
                    SoDienThoai = soDienThoai,
                    DiaChi = diaChi
                };
            }

            try
            {
                // 1. Tạo Đơn Hàng
                var donHang = new DonHang
                {
                    KhachHangId = khachHang.ID,
                    NgayDat = DateTime.Now,
                    TrangThai = "Chờ xử lý",
                    GhiChu = ghiChu,
                    TongTien = 0,
                };

                if (khachHang.DiaChi != diaChi || khachHang.SoDienThoai != soDienThoai)
                {
                    khachHang.DiaChi = diaChi;
                    khachHang.SoDienThoai = soDienThoai;
                    await _unitOfWork.KhachHangRepository.UpdateAsync(khachHang);
                }

                await _unitOfWork.DonHangRepository.AddAsync(donHang);

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
                        DonHang = donHang,
                        MaVatTu = item.VatTuId,
                        SoLuong = item.SoLuong,
                        DonGia = item.DonGia,
                        ThanhTien = item.ThanhTien
                    };

                    tongTien += item.ThanhTien;
                    await _unitOfWork.ChiTietDonHangRepository.AddAsync(chiTiet);
                }
                donHang.TongTien = tongTien;

                _unitOfWork.Save();
                HttpContext.Session.Remove(CART_KEY);

                return RedirectToAction("OrderSuccess");
            }
            catch (Exception)
            {
                // Log lỗi
                TempData["Error"] = "Có lỗi xảy ra khi xử lý đơn hàng. Vui lòng thử lại.";
                return RedirectToAction("Checkout");
            }
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}