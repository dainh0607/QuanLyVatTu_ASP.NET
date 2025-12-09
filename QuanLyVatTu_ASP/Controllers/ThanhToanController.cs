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
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "GioHang");
            }
            if (HttpContext.Session.GetString("Role") != "Customer")
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/ThanhToan/Checkout" });
            }
            var email = HttpContext.Session.GetString("Email");
            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);

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
            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);

            decimal tongTien = 0;

            if (khachHang == null) return RedirectToAction("Login", "Account");

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
                    _unitOfWork.KhachHangRepository.UpdateAsync(khachHang);
                }

                await _unitOfWork.DonHangRepository.AddAsync(donHang);

                foreach (var item in cart)
                {
                    var vatTu = await _unitOfWork.VatTuRepository.GetByIdAsync(item.VatTuId);

                    if (vatTu.SoLuongTon < item.SoLuong)
                    {
                        TempData["Error"] = $"Sản phẩm {vatTu.TenVatTu} chỉ còn {vatTu.SoLuongTon} sản phẩm.";
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
            catch (Exception ex)
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