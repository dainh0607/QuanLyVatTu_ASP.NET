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
                    HinhAnh = !string.IsNullOrEmpty(vatTu.HinhAnh) ? vatTu.HinhAnh : $"https://placehold.co/120x120?text={Uri.EscapeDataString(vatTu.TenVatTu ?? "SP")}"
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
        public async Task<IActionResult> Wishlist()
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                // Chưa đăng nhập -> Trả về danh sách rỗng và cờ RequireLogin để hiện Modal ở View
                ViewBag.RequireLogin = true;
                return View(new List<WishlistItem>());
            }

            var wishlistEntities = await _unitOfWork.YeuThichRepository.GetByKhachHangIdAsync(khachHangId.Value);
            var wishlistViewModels = wishlistEntities.Select(x => new WishlistItem
            {
                VatTuId = x.MaVatTu,
                TenVatTu = x.VatTu?.TenVatTu,
                DonGia = x.VatTu?.GiaBan ?? 0,
                HinhAnh = !string.IsNullOrEmpty(x.VatTu?.HinhAnh) ? x.VatTu.HinhAnh : $"https://placehold.co/100x100?text={Uri.EscapeDataString(x.VatTu?.TenVatTu ?? "SP")}",
                NgayThem = x.NgayThem
            }).ToList();

            ViewBag.Total = wishlistViewModels.Count;
            return View(wishlistViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                return Json(new { success = false, requireLogin = true, message = "Vui lòng đăng nhập để thêm vào yêu thích!" });
            }

            var vatTu = await _unitOfWork.VatTuRepository.GetByIdAsync(productId);
            if (vatTu == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            var existingItem = await _unitOfWork.YeuThichRepository.GetByKhachHangAndVatTuAsync(khachHangId.Value, productId);
            if (existingItem != null)
            {
                return Json(new { success = false, message = "Sản phẩm đã có trong danh sách yêu thích" });
            }

            var newItem = new QuanLyVatTu_ASP.Areas.Admin.Models.YeuThich
            {
                MaKhachHang = khachHangId.Value,
                MaVatTu = productId,
                NgayThem = DateTime.Now
            };

            await _unitOfWork.YeuThichRepository.AddAsync(newItem);
            _unitOfWork.Save();

            // Lấy lại số lượng để cập nhật badge
            var currentList = await _unitOfWork.YeuThichRepository.GetByKhachHangIdAsync(khachHangId.Value);

            return Json(new { success = true, message = "Đã thêm vào yêu thích", count = currentList.Count() });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                return Json(new { success = false, requireLogin = true, message = "Vui lòng đăng nhập!" });
            }

            var item = await _unitOfWork.YeuThichRepository.GetByKhachHangAndVatTuAsync(khachHangId.Value, productId);
            
            if (item != null)
            {
                _unitOfWork.YeuThichRepository.Delete(item);
                _unitOfWork.Save();
            }

            var currentList = await _unitOfWork.YeuThichRepository.GetByKhachHangIdAsync(khachHangId.Value);

            return Json(new { success = true, count = currentList.Count() });
        }

        /// <summary>
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