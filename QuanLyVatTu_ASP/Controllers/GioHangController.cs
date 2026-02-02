using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Extensions;
using QuanLyVatTu_ASP.Models.ViewModels;
using QuanLyVatTu_ASP.Repositories.Interfaces;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Areas.Admin.Models;


namespace QuanLyVatTu_ASP.Controllers
{
    public class GioHangController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private const string CART_KEY = "MY_CART";

        public GioHangController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult GioHang()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY);

            // DEMO DATA: Thêm dữ liệu mẫu nếu giỏ hàng trống
            if (cart == null || !cart.Any())
            {
                cart = new List<CartItem>
                {
                    new CartItem 
                    { 
                        VatTuId = 1, 
                        TenVatTu = "Xi măng Holcim PCB40 (50kg)", 
                        DonGia = 125000, 
                        SoLuong = 10, 
                        HinhAnh = "https://placehold.co/120x120/0d6efd/ffffff?text=Xi+Mang" 
                    },
                    new CartItem 
                    { 
                        VatTuId = 2, 
                        TenVatTu = "Thép hình chữ V 50x50x5mm (dài 6m)", 
                        DonGia = 285000, 
                        SoLuong = 4, 
                        HinhAnh = "https://placehold.co/120x120/495057/ffffff?text=Thep+V" 
                    },
                    new CartItem 
                    { 
                        VatTuId = 3, 
                        TenVatTu = "Dây điện Cadivi CV 2.5mm² (cuộn 100m)", 
                        DonGia = 850000, 
                        SoLuong = 2, 
                        HinhAnh = "https://placehold.co/120x120/ff6b00/ffffff?text=Day+Dien" 
                    },
                    new CartItem 
                    { 
                        VatTuId = 4, 
                        TenVatTu = "Sơn Dulux nội thất EasyClean (5L)", 
                        DonGia = 1250000, 
                        SoLuong = 3, 
                        HinhAnh = "https://placehold.co/120x120/ffc107/333333?text=Son+Dulux" 
                    },
                    new CartItem 
                    { 
                        VatTuId = 5, 
                        TenVatTu = "Ống nhựa PVC Bình Minh Ø90mm (4m)", 
                        DonGia = 185000, 
                        SoLuong = 8, 
                        HinhAnh = "https://placehold.co/120x120/17a2b8/ffffff?text=Ong+PVC" 
                    },
                    new CartItem 
                    { 
                        VatTuId = 6, 
                        TenVatTu = "Máy khoan Bosch GSB 550 Professional", 
                        DonGia = 1450000, 
                        SoLuong = 1, 
                        HinhAnh = "https://placehold.co/120x120/28a745/ffffff?text=May+Khoan" 
                    }
                };
            }

            // Tính tổng tiền để hiển thị
            ViewBag.Total = cart.Sum(item => item.ThanhTien);
            return View(cart);
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var vatTu = await _unitOfWork.VatTuRepository.GetByIdAsync(productId);
            if (vatTu == null) return NotFound();

            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(x => x.VatTuId == productId);

            if (existingItem != null)
            {
                existingItem.SoLuong += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    VatTuId = vatTu.ID,
                    TenVatTu = vatTu.TenVatTu,
                    DonGia = vatTu.GiaBan ?? 0,
                    SoLuong = quantity,
                });
            }

            HttpContext.Session.Set(CART_KEY, cart);

            return Json(new { success = true, message = "Added to cart", cartCount = cart.Sum(x => x.SoLuong) });
        }

        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(x => x.VatTuId == productId);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.Set(CART_KEY, cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(x => x.VatTuId == productId);
            if (item != null && quantity > 0)
            {
                item.SoLuong = quantity;
                HttpContext.Session.Set(CART_KEY, cart);
            }
            return Json(new { success = true });
        }
        public IActionResult Wishlist()
        {

            return View();
        }

    }
}