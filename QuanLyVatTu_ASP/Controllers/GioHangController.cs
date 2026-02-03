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
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();

            // Tính tổng tiền để hiển thị
            ViewBag.Total = cart.Sum(item => item.ThanhTien);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var vatTu = await _unitOfWork.VatTuRepository.GetByIdAsync(productId);
            if (vatTu == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });

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
                    DonViTinh = vatTu.DonViTinh,
                    HinhAnh = $"https://placehold.co/120x120?text={Uri.EscapeDataString(vatTu.TenVatTu ?? "SP")}"
                });
            }

            HttpContext.Session.Set(CART_KEY, cart);

            return Json(new { 
                success = true, 
                message = "Đã thêm vào giỏ hàng!", 
                cartCount = cart.Sum(x => x.SoLuong),
                cartTotal = cart.Sum(x => x.ThanhTien)
            });
        }

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng (AJAX)
        /// </summary>
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(x => x.VatTuId == productId);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.Set(CART_KEY, cart);
            }
            return Json(new { 
                success = true, 
                message = "Đã xóa sản phẩm khỏi giỏ hàng",
                cartCount = cart.Sum(x => x.SoLuong),
                cartTotal = cart.Sum(x => x.ThanhTien)
            });
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng (AJAX)
        /// </summary>
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
            else if (item != null && quantity <= 0)
            {
                // Nếu số lượng = 0, xóa sản phẩm
                cart.Remove(item);
                HttpContext.Session.Set(CART_KEY, cart);
            }
            return Json(new { 
                success = true,
                cartCount = cart.Sum(x => x.SoLuong),
                cartTotal = cart.Sum(x => x.ThanhTien),
                lineTotal = item?.ThanhTien ?? 0
            });
        }

        /// <summary>
        /// Cập nhật số lượng từ nút +/- (AJAX) - endpoint compatible với frontend hiện tại
        /// </summary>
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            return UpdateCart(productId, quantity);
        }

        /// <summary>
        /// Lấy dữ liệu giỏ hàng cho dropdown (AJAX)
        /// </summary>
        [HttpGet]
        public IActionResult GetCartData()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            return Json(new {
                success = true,
                items = cart.Select(x => new {
                    x.VatTuId,
                    x.TenVatTu,
                    x.DonGia,
                    x.SoLuong,
                    x.ThanhTien,
                    x.HinhAnh
                }),
                cartCount = cart.Sum(x => x.SoLuong),
                cartTotal = cart.Sum(x => x.ThanhTien)
            });
        }
        public IActionResult Wishlist()
        {
            var wishlist = HttpContext.Session.Get<List<WishlistItem>>("WISHLIST") ?? new List<WishlistItem>();
            ViewBag.Total = wishlist.Count;
            return View(wishlist);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var vatTu = await _unitOfWork.VatTuRepository.GetByIdAsync(productId);
            if (vatTu == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            var wishlist = HttpContext.Session.Get<List<WishlistItem>>("WISHLIST") ?? new List<WishlistItem>();

            if (wishlist.Any(x => x.VatTuId == productId))
            {
                return Json(new { success = false, message = "Sản phẩm đã có trong danh sách yêu thích" });
            }

            wishlist.Add(new WishlistItem
            {
                VatTuId = vatTu.ID,
                TenVatTu = vatTu.TenVatTu,
                DonGia = vatTu.GiaBan ?? 0,
                HinhAnh = $"https://placehold.co/100x100?text={Uri.EscapeDataString(vatTu.TenVatTu ?? "SP")}",
                NgayThem = DateTime.Now
            });

            HttpContext.Session.Set("WISHLIST", wishlist);

            return Json(new { success = true, message = "Đã thêm vào yêu thích", count = wishlist.Count });
        }

        [HttpPost]
        public IActionResult RemoveFromWishlist(int productId)
        {
            var wishlist = HttpContext.Session.Get<List<WishlistItem>>("WISHLIST") ?? new List<WishlistItem>();
            var item = wishlist.FirstOrDefault(x => x.VatTuId == productId);
            
            if (item != null)
            {
                wishlist.Remove(item);
                HttpContext.Session.Set("WISHLIST", wishlist);
            }

            return Json(new { success = true, count = wishlist.Count });
        }

        /// <summary>
        /// Xuất báo giá từ giỏ hàng - trang print-friendly
        /// </summary>
        public IActionResult BaoGia()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();

            // Nếu giỏ hàng trống, redirect về trang sản phẩm
            if (!cart.Any())
            {
                TempData["Warning"] = "Giỏ hàng của bạn đang trống. Vui lòng thêm sản phẩm trước khi tải báo giá.";
                return RedirectToAction("Index", "SanPham");
            }

            ViewBag.Total = cart.Sum(x => x.ThanhTien);
            ViewBag.TotalWeight = cart.Sum(x => x.TongTrongLuong);
            ViewBag.QuoteDate = DateTime.Now;

            return View(cart);
        }

    }
}