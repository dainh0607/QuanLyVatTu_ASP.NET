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
        private const string DIRECT_CART_KEY = "DIRECT_CART";

        public GioHangController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> GioHang()
        {
            var cart = await GetCartItemsSecureAsync();

            // Tính tổng tiền để hiển thị
            ViewBag.Total = cart.Sum(item => item.ThanhTien);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> BuyNow(int productId, int quantity = 1)
        {
            // Buy Now keeps using logic session for immediate checkout
            var vatTu = await _unitOfWork.VatTuRepository.GetByIdAsync(productId);
            if (vatTu == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            var directCart = new List<CartItem>
            {
                new CartItem
                {
                    VatTuId = vatTu.ID,
                    TenVatTu = vatTu.TenVatTu,
                    DonGia = vatTu.GiaBan ?? 0,
                    SoLuong = quantity,
                    DonViTinh = vatTu.DonViTinh,
                    HinhAnh = !string.IsNullOrEmpty(vatTu.HinhAnh) ? vatTu.HinhAnh : $"https://placehold.co/120x120?text={Uri.EscapeDataString(vatTu.TenVatTu ?? "SP")}"
                }
            };
            HttpContext.Session.Set(DIRECT_CART_KEY, directCart);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var vatTu = await _unitOfWork.VatTuRepository.GetByIdAsync(productId);
            if (vatTu == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            // Server-side stock validation
            if (quantity < 1) quantity = 1;
            if (quantity > vatTu.SoLuongTon)
            {
                return Json(new { success = false, message = $"Chỉ còn {vatTu.SoLuongTon} sản phẩm trong kho." });
            }

            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            
            if (khachHangId != null)
            {
                // LOGGED IN: Use Database
                var gioHang = await _unitOfWork.GioHangRepository.GetByKhachHangIdAsync(khachHangId.Value);
                if (gioHang == null)
                {
                    gioHang = new GioHang { MaKhachHang = khachHangId.Value };
                    await _unitOfWork.GioHangRepository.AddAsync(gioHang);
                    _unitOfWork.Save(); // Save to generate ID
                }

                // Check existing item
                var chiTiet = gioHang.ChiTietGioHangs?.FirstOrDefault(x => x.MaVatTu == productId);
                if (chiTiet != null)
                {
                    chiTiet.SoLuong += quantity;
                    // Ensure tracking or generic update
                    // Since we loaded via navigation, Context tracks it. saving changes is enough? 
                    // EF Core usually tracks connected entities.
                     _unitOfWork.ChiTietGioHangRepository.Update(chiTiet);
                }
                else
                {
                    var newChiTiet = new ChiTietGioHang
                    {
                        MaGioHang = gioHang.ID,
                        MaVatTu = productId,
                        SoLuong = quantity
                    };
                     await _unitOfWork.ChiTietGioHangRepository.AddAsync(newChiTiet);
                }
                _unitOfWork.Save();
            }
            else
            {
                // GUEST: Use Session
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
            }

            // Return updated counts
            var updatedCart = await GetCartItemsSecureAsync();

            return Json(new { 
                success = true, 
                message = "Đã thêm vào giỏ hàng!", 
                cartCount = updatedCart.Sum(x => x.SoLuong),
                cartTotal = updatedCart.Sum(x => x.ThanhTien)
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId != null)
            {
                var gioHang = await _unitOfWork.GioHangRepository.GetByKhachHangIdAsync(khachHangId.Value);
                if (gioHang != null)
                {
                    var item = gioHang.ChiTietGioHangs?.FirstOrDefault(x => x.MaVatTu == productId);
                    if (item != null)
                    {
                        _unitOfWork.ChiTietGioHangRepository.Delete(item);
                        _unitOfWork.Save();
                    }
                }
            }
            else
            {
                var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
                var item = cart.FirstOrDefault(x => x.VatTuId == productId);
                if (item != null)
                {
                    cart.Remove(item);
                    HttpContext.Session.Set(CART_KEY, cart);
                }
            }

            var updatedCart = await GetCartItemsSecureAsync();
            return Json(new { 
                success = true, 
                message = "Đã xóa sản phẩm khỏi giỏ hàng",
                cartCount = updatedCart.Sum(x => x.SoLuong),
                cartTotal = updatedCart.Sum(x => x.ThanhTien)
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCart(int productId, int quantity)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            decimal lineTotal = 0;

            if (khachHangId != null)
            {
                var gioHang = await _unitOfWork.GioHangRepository.GetByKhachHangIdAsync(khachHangId.Value);
                if (gioHang != null)
                {
                    var item = gioHang.ChiTietGioHangs?.FirstOrDefault(x => x.MaVatTu == productId);
                    if (item != null)
                    {
                         if (quantity > 0)
                         {
                             item.SoLuong = quantity;
                             _unitOfWork.ChiTietGioHangRepository.Update(item);
                             lineTotal = (decimal)(item.SoLuong * (item.VatTu?.GiaBan ?? 0));
                         }
                         else
                         {
                             _unitOfWork.ChiTietGioHangRepository.Delete(item);
                         }
                         _unitOfWork.Save();
                    }
                }
            }
            else
            {
                var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
                var item = cart.FirstOrDefault(x => x.VatTuId == productId);
                if (item != null)
                {
                    if (quantity > 0)
                    {
                        item.SoLuong = quantity;
                        HttpContext.Session.Set(CART_KEY, cart);
                        lineTotal = item.ThanhTien;
                    }
                    else
                    {
                        cart.Remove(item);
                        HttpContext.Session.Set(CART_KEY, cart);
                    }
                }
            }

            var updatedCart = await GetCartItemsSecureAsync();
            return Json(new { 
                success = true,
                cartCount = updatedCart.Sum(x => x.SoLuong),
                cartTotal = updatedCart.Sum(x => x.ThanhTien),
                lineTotal = lineTotal
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            return await UpdateCart(productId, quantity);
        }

        [HttpGet]
        public async Task<IActionResult> GetCartData()
        {
            var cart = await GetCartItemsSecureAsync();
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

        private async Task<List<CartItem>> GetCartItemsSecureAsync()
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId != null)
            {
                var gioHang = await _unitOfWork.GioHangRepository.GetByKhachHangIdAsync(khachHangId.Value);
                if (gioHang == null || gioHang.ChiTietGioHangs == null) return new List<CartItem>();
                
                return gioHang.ChiTietGioHangs.Select(ct => new CartItem
                {
                    VatTuId = ct.MaVatTu,
                    TenVatTu = ct.VatTu?.TenVatTu ?? "",
                    DonGia = ct.VatTu?.GiaBan ?? 0,
                    SoLuong = ct.SoLuong,
                    DonViTinh = ct.VatTu?.DonViTinh ?? "",
                    HinhAnh = !string.IsNullOrEmpty(ct.VatTu?.HinhAnh) ? ct.VatTu.HinhAnh : $"https://placehold.co/120x120?text={Uri.EscapeDataString(ct.VatTu?.TenVatTu ?? "SP")}"
                }).ToList();
            }
            
            // Fallback to session
            return HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
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
        /// Xuất báo giá từ giỏ hàng (supports both DB and session carts)
        /// </summary>
        public async Task<IActionResult> BaoGia()
        {
            var cart = await GetCartItemsSecureAsync();

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



        [HttpPost]
        public async Task<IActionResult> ReOrder(int orderId)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                TempData["Warning"] = "Vui lòng đăng nhập để thực hiện chức năng này.";
                return RedirectToAction("Login", "Account");
            }

            var donHang = await _unitOfWork.DonHangRepository.GetDonHangByIdAsync(orderId);
            if (donHang == null || donHang.KhachHangId != khachHangId)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Orders", "Customer");
            }

            // Get active cart or create new
            var gioHang = await _unitOfWork.GioHangRepository.GetByKhachHangIdAsync(khachHangId.Value);
            if (gioHang == null)
            {
                gioHang = new GioHang { MaKhachHang = khachHangId.Value };
                await _unitOfWork.GioHangRepository.AddAsync(gioHang);
                _unitOfWork.Save();
            }

            int addedCount = 0;
            var successMessages = new List<string>();
            var warningMessages = new List<string>();

            if (donHang.ChiTietDonHangs != null)
            {
                foreach (var detail in donHang.ChiTietDonHangs)
                {
                    // Check current product state
                    // Force reload to bypass cache and get real-time stock
                    var vatTu = await _unitOfWork.VatTuRepository.GetByIdRealtimeAsync(detail.MaVatTu);

                    // Check if product exists and is active (you might want to check a 'Deleted' flag if exists, here checking null)
                    if (vatTu == null)
                    {
                         warningMessages.Add($"SP #{detail.MaVatTu}: Không còn tồn tại.");
                         continue;
                    }

                    if (vatTu.SoLuongTon <= 0)
                    {
                        warningMessages.Add($"{vatTu.TenVatTu}: Đã hết hàng.");
                        continue;
                    }

                    int quantityRequested = detail.SoLuong ?? 1;
                    int quantityToAdd = quantityRequested;
                    bool isPartial = false;

                    // Partial Stock Check
                    if (quantityToAdd > vatTu.SoLuongTon)
                    {
                        quantityToAdd = vatTu.SoLuongTon ?? 0;
                        isPartial = true;
                    }

                    if (quantityToAdd <= 0) 
                    {
                         warningMessages.Add($"{vatTu.TenVatTu}: Đã hết hàng thực tế.");
                         continue;
                    }

                    var existingItem = gioHang.ChiTietGioHangs?.FirstOrDefault(x => x.MaVatTu == detail.MaVatTu);
                    if (existingItem != null)
                    {
                        existingItem.SoLuong += quantityToAdd;
                        // Re-check stock cap for total in cart (optional but safe)
                        if (existingItem.SoLuong > vatTu.SoLuongTon) 
                        {
                            existingItem.SoLuong = vatTu.SoLuongTon ?? 0;
                        }
                        _unitOfWork.ChiTietGioHangRepository.Update(existingItem);
                    }
                    else
                    {
                        var newItem = new ChiTietGioHang
                        {
                            MaGioHang = gioHang.ID,
                            MaVatTu = detail.MaVatTu,
                            SoLuong = quantityToAdd
                        };
                        await _unitOfWork.ChiTietGioHangRepository.AddAsync(newItem);
                    }
                    
                    addedCount++;
                    
                    if (isPartial)
                    {
                        warningMessages.Add($"{vatTu.TenVatTu}: Tồn kho không đủ (Cần {quantityRequested}, chỉ còn {quantityToAdd}). Đã thêm {quantityToAdd}.");
                    }
                    else
                    {
                        successMessages.Add($"{vatTu.TenVatTu}: Đã thêm {quantityToAdd}.");
                    }
                }
            }
            
            _unitOfWork.Save();

            // Construct Result Message
            var finalMessage = "";
            if (addedCount > 0)
            {
                finalMessage += $"<div class=\"text-success fw-bold mb-2\"><i class=\"fas fa-check-circle me-1\"></i>Đã thêm {addedCount} sản phẩm vào giỏ hàng</div>";
            }
            
            if (warningMessages.Any())
            {
                finalMessage += "<div class=\"text-start mt-2\"><div class=\"fw-bold text-warning mb-1\"><i class=\"fas fa-exclamation-triangle me-1\"></i>Một số sản phẩm chưa được thêm:</div><ul class=\"mb-0 ps-3 text-secondary text-start\" style=\"list-style-type: disc;\">";
                foreach (var msg in warningMessages)
                {
                    finalMessage += $"<li class=\"mb-1\">{msg}</li>";
                }
                finalMessage += "</ul></div>";
                
                TempData["Warning"] = finalMessage; 
            }
            else if (addedCount > 0)
            {
                TempData["Success"] = finalMessage;
            }
            else
            {
                TempData["Error"] = "Tất cả sản phẩm trong đơn hàng này hiện đã hết hàng.";
                return RedirectToAction("Orders", "Customer");
            }

            return RedirectToAction("GioHang");
        }
    }
}