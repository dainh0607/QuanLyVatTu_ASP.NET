using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Extensions;
using QuanLyVatTu_ASP.Models.ViewModels;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Repositories.Interfaces;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace QuanLyVatTu_ASP.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string CART_KEY = "MY_CART";
        private const string DIRECT_CART_KEY = "DIRECT_CART";

        public ThanhToanController(IUnitOfWork unitOfWork, AppDbContext context, IEmailService emailService, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
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

            // Check if we are editing an existing order
            var editingOrderId = HttpContext.Session.GetInt32("EditingOrderId");
            if (editingOrderId.HasValue)
            {
                ViewBag.IsEditingOrder = true;
                ViewBag.EditingOrderId = editingOrderId.Value;
                // Optional: Fetch order to show old details or warn user?
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
            bool isBuyNow = false,
            // VAT Invoice fields
            bool xuatVAT = false,
            string? tenCongTy = null,
            string? maSoThue = null,
            string? emailVAT = null,
            string? diaChiDKKD = null)
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
                // ===== BƯỚC 1: KIỂM TRA TỒN KHO TRƯỚC KHI TẠO ĐƠN =====
                // Pre-validate ALL items against stock before creating any order
                var stockIssues = new List<string>();
                bool hasStockProblem = false;

                foreach (var item in cart)
                {
                    var vatTuCheck = await _unitOfWork.VatTuRepository.GetByIdAsync(item.VatTuId, tracking: false);

                    if (vatTuCheck == null)
                    {
                        stockIssues.Add($"Sản phẩm (ID: {item.VatTuId}) không tồn tại.");
                        hasStockProblem = true;
                        continue;
                    }

                    if (vatTuCheck.SoLuongTon < item.SoLuong)
                    {
                        hasStockProblem = true;
                        // Cap cart quantity to available stock
                        if (vatTuCheck.SoLuongTon > 0)
                        {
                            await CapCartItemQuantity((int)item.VatTuId, (int)vatTuCheck.SoLuongTon, isBuyNow, khachHang.ID);
                            stockIssues.Add($"Sản phẩm {vatTuCheck.TenVatTu} chỉ còn {vatTuCheck.SoLuongTon} sản phẩm. Số lượng trong giỏ đã được điều chỉnh.");
                        }
                        else
                        {
                            stockIssues.Add($"Sản phẩm {vatTuCheck.TenVatTu} đã hết hàng.");
                        }
                    }
                }

                // If any stock issues, redirect back WITHOUT creating the order
                if (hasStockProblem)
                {
                    TempData["Error"] = string.Join("<br/>", stockIssues);
                    return RedirectToAction("Checkout");
                }

                // ===== BƯỚC 1.5: XỬ LÝ GHI CHÚ VẬN CHUYỂN =====
                string shippingMethodDesc = "";
                switch (shippingMethod)
                {
                    case "delivery": shippingMethodDesc = "Cửa hàng giao tận nơi"; break;
                    case "pickup": shippingMethodDesc = "Đến lấy tại cửa hàng"; break;
                    default: shippingMethodDesc = "Mặc định"; break;
                }
                
                if (!string.IsNullOrEmpty(ghiChu))
                    ghiChu += $" | Vận chuyển: {shippingMethodDesc}";
                else
                    ghiChu = $"Vận chuyển: {shippingMethodDesc}";


                // ===== BƯỚC 2: TẠO HOẶC CẬP NHẬT ĐƠN HÀNG =====
                DonHang donHang;
                var editingOrderId = HttpContext.Session.GetInt32("EditingOrderId");
                bool isEditMode = false;

                // Initial Status determination (will be refined in Step 4)
                string initialStatus = "Chờ xử lý";
                string initialDepositMethod = "COD";
                
                if (paymentMethod == "bank") 
                {
                    initialStatus = "Chờ thanh toán";
                    initialDepositMethod = "Chuyển khoản";
                }

                if (editingOrderId.HasValue)
                {
                    donHang = await _unitOfWork.DonHangRepository.GetByIdAsync(editingOrderId.Value);
                    if (donHang != null && donHang.KhachHangId == khachHang.ID)
                    {
                        isEditMode = true;
                        donHang.NgayDat = DateTime.Now;
                        donHang.TrangThai = initialStatus;
                        donHang.GhiChu = ghiChu;
                        donHang.TongTien = 0;
                        donHang.PhuongThucDatCoc = initialDepositMethod;
                        donHang.SoTienDatCoc = 0; // Reset
                        donHang.NgayDatCoc = null;
                    }
                    else
                    {
                        donHang = new DonHang
                        {
                            MaHienThi = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999), 
                            KhachHangId = khachHang.ID,
                            NgayDat = DateTime.Now,
                            TrangThai = initialStatus,
                            GhiChu = ghiChu,
                            TongTien = 0,
                            PhuongThucDatCoc = initialDepositMethod
                        };
                        await _unitOfWork.DonHangRepository.AddAsync(donHang);
                    }
                }
                else
                {
                    donHang = new DonHang
                    {
                        MaHienThi = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999), 
                        KhachHangId = khachHang.ID,
                        NgayDat = DateTime.Now,
                        TrangThai = initialStatus,
                        GhiChu = ghiChu,
                        TongTien = 0,
                        PhuongThucDatCoc = initialDepositMethod
                    };
                    await _unitOfWork.DonHangRepository.AddAsync(donHang);
                }

                if (khachHang.DiaChi != diaChi || khachHang.SoDienThoai != soDienThoai)
                {
                    khachHang.DiaChi = diaChi;
                    khachHang.SoDienThoai = soDienThoai;
                    await _unitOfWork.KhachHangRepository.UpdateAsync(khachHang);
                }
                
                _unitOfWork.Save();


                // ===== BƯỚC 3: TRỪ TỒN KHO VÀ TẠO CHI TIẾT ĐƠN =====
                if (isEditMode)
                {
                   // Ensure details are cleared if not already
                   // We trust EditOrder cleared them, but for safety in 'Update Existing' flow:
                   var existingDetails = await _context.ChiTietDonHangs.Where(d => d.MaDonHang == donHang.ID).ToListAsync();
                   if (existingDetails.Any()) _context.ChiTietDonHangs.RemoveRange(existingDetails);
                }

                foreach (var item in cart)
                {
                    var vatTu = await _unitOfWork.VatTuRepository.GetByIdAsync(item.VatTuId, tracking: true);
                    vatTu.SoLuongTon -= item.SoLuong;
                    _unitOfWork.VatTuRepository.Update(vatTu);

                    var chiTiet = new ChiTietDonHang
                    {
                        MaDonHang = donHang.ID,
                        MaVatTu = item.VatTuId,
                        SoLuong = item.SoLuong,
                        DonGia = item.DonGia,
                        ThanhTien = item.ThanhTien
                    };

                    tongTien += item.ThanhTien;
                    await _unitOfWork.ChiTietDonHangRepository.AddAsync(chiTiet);
                }

                // ===== BƯỚC 4: CẬP NHẬT TỔNG TIỀN & LOGIC ĐẶT CỌC =====
                donHang.TongTien = tongTien;

                if (tongTien >= 5000000)
                {
                    // Over 5M -> Require 10% Deposit via Bank Transfer (QR)
                    donHang.SoTienDatCoc = tongTien * 0.1m; 
                    donHang.TrangThai = "Chờ đặt cọc";
                    donHang.PhuongThucDatCoc = "Chuyển khoản (QR)";
                    donHang.GhiChu += " | Đơn hàng >= 5tr, bắt buộc cọc 10%.";
                }
                else
                {
                    // Under 5M -> Keep original choice (COD or Bank)
                    // If COD was chosen, it remains "COD" and Status "Chờ xử lý"
                    // If Bank was chosen, it remains "Chuyển khoản" and Status "Chờ thanh toán"
                    donHang.SoTienDatCoc = 0;
                }

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
                TempData["TongTien"] = tongTien.ToString(System.Globalization.CultureInfo.InvariantCulture);
                TempData["PaymentMethod"] = paymentMethod;
                TempData["ShippingMethod"] = shippingMethod;
                TempData["HoTen"] = khachHang.HoTen;
                TempData["SoDienThoai"] = soDienThoai;
                TempData["MaHienThi"] = donHang.MaHienThi;
                TempData["SoTienDatCoc"] = (donHang.SoTienDatCoc ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture); 

                // ===== BƯỚC 5: TẠO HÓA ĐƠN VAT (nếu được yêu cầu) =====
                if (xuatVAT && !string.IsNullOrEmpty(tenCongTy) && !string.IsNullOrEmpty(maSoThue))
                {
                    try
                    {
                        var tienThue = tongTien * 0.10m;
                        var hoaDonVAT = new HoaDonVAT
                        {
                            MaDonHang = donHang.ID,
                            MaKhachHang = khachHang.ID,
                            TenCongTy = tenCongTy,
                            MaSoThue = maSoThue,
                            DiaChiDKKD = diaChiDKKD ?? diaChi,
                            EmailNhanHoaDon = emailVAT ?? email,
                            TongTienTruocThue = tongTien,
                            ThueSuat = 10,
                            TienThue = tienThue,
                            TongTienSauThue = tongTien + tienThue,
                            NgayLap = DateTime.Now,
                            TrangThai = "Đã xuất",
                            NgayTao = DateTime.Now
                        };

                        // Step 1: Insert to get auto-generated ID
                        _context.HoaDonVATs.Add(hoaDonVAT);
                        await _context.SaveChangesAsync();

                        // Step 2: Generate SoHoaDon from ID (C# code thay vì computed column)
                        hoaDonVAT.SoHoaDon = $"VAT{DateTime.Now:yyyy}-{hoaDonVAT.ID:D3}";
                        await _context.SaveChangesAsync();

                        TempData["VATInvoiceCreated"] = "true";

                        // Send VAT invoice email to customer
                        var recipientEmail = hoaDonVAT.EmailNhanHoaDon;
                        if (!string.IsNullOrEmpty(recipientEmail))
                        {
                            try
                            {
                                var (emailBody, embeddedImages) = BuildVATEmailBody(hoaDonVAT, cart, _webHostEnvironment);
                                var vatSubject = $"Hóa đơn GTGT #{hoaDonVAT.SoHoaDon} - Đơn hàng {donHang.MaHienThi}";

                                await _emailService.SendEmailWithEmbeddedImagesAsync(recipientEmail, vatSubject, emailBody, embeddedImages);

                                // Send copy to admin
                                await _emailService.SendEmailWithEmbeddedImagesAsync(
                                    "cuongdqtb01697@gmail.com",
                                    $"[Admin Copy] {vatSubject}",
                                    emailBody,
                                    embeddedImages
                                );
                            }
                            catch (Exception emailEx)
                            {
                                TempData["Warning"] = "Hóa đơn VAT đã lưu nhưng gửi email thất bại: " + emailEx.Message;
                            }
                        }
                    }
                    catch (Exception vatEx)
                    {
                        // Log detailed error for debugging
                        var innerMsg = vatEx.InnerException?.Message ?? "";
                        TempData["Warning"] = $"Đơn hàng tạo thành công nhưng lỗi hóa đơn VAT: {vatEx.Message} {innerMsg}";
                    }
                }

                // Clear Cart
                if (isBuyNow)
                {
                     HttpContext.Session.Remove(DIRECT_CART_KEY);
                }
                else
                {
                    // Clear DB cart using direct SQL to avoid EF tracking conflicts
                    // (Layout already loaded and tracked ChiTietGioHang entities)
                    if (khachHang.ID > 0)
                    {
                        var gioHangId = await _context.GioHangs
                            .Where(g => g.MaKhachHang == khachHang.ID)
                            .Select(g => g.ID)
                            .FirstOrDefaultAsync();
                        
                        if (gioHangId > 0)
                        {
                            await _context.Database.ExecuteSqlRawAsync(
                                "DELETE FROM ChiTietGioHang WHERE MaGioHang = {0}", gioHangId);
                        }
                    }
                    // Also clear session just in case
                    HttpContext.Session.Remove(CART_KEY);
                }
                
                // Clear Editing Session IF we just updated it
                if (isEditMode)
                {
                    HttpContext.Session.Remove("EditingOrderId");
                }

                // Redirect logic
                if (donHang.SoTienDatCoc > 0 || paymentMethod == "bank")
                {
                    // Redirect to QR page specifically for deposit or full payment
                    return RedirectToAction("OrderSuccessQR", new { id = donHang.ID }); 
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

        public async Task<IActionResult> OrderSuccessQR(int? id)
        {
            if (id.HasValue)
            {
                 var donHang = await _context.DonHang
                                    .Include(d => d.KhachHang)
                                    .FirstOrDefaultAsync(d => d.ID == id.Value);
                 
                 if (donHang != null)
                 {
                      // Optional: Check ownership
                      var email = HttpContext.Session.GetString("Email");
                      if (!string.IsNullOrEmpty(email)) {
                          // Note: This relies on donHang.KhachHang being loaded or checking against DB again
                          // simpler:
                          if (donHang.KhachHang != null && donHang.KhachHang.Email == email)
                          {
                               return View(donHang);
                          }
                      }
                 }
            }
            return View(null);
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
        /// <summary>
        /// Cap the cart item quantity to maxQty in both session and DB carts.
        /// </summary>
        private async Task CapCartItemQuantity(int productId, int maxQty, bool isBuyNow, int khachHangId)
        {
            // Update session cart
            var sessionKey = isBuyNow ? DIRECT_CART_KEY : CART_KEY;
            var sessionCart = HttpContext.Session.Get<List<CartItem>>(sessionKey);
            if (sessionCart != null)
            {
                var sessionItem = sessionCart.FirstOrDefault(x => x.VatTuId == productId);
                if (sessionItem != null)
                {
                    sessionItem.SoLuong = maxQty;
                }
                HttpContext.Session.Set(sessionKey, sessionCart);
            }

            // Update DB cart for logged-in users
            if (!isBuyNow && khachHangId > 0)
            {
                var gioHang = await _unitOfWork.GioHangRepository.GetByKhachHangIdAsync(khachHangId);
                if (gioHang?.ChiTietGioHangs != null)
                {
                    var dbItem = gioHang.ChiTietGioHangs.FirstOrDefault(x => x.MaVatTu == productId);
                    if (dbItem != null)
                    {
                        dbItem.SoLuong = maxQty;
                        _unitOfWork.Save();
                    }
                }
            }
        }

        /// <summary>
        /// Build HTML email body for VAT invoice
        /// </summary>
        private static (string, Dictionary<string, byte[]>) BuildVATEmailBody(HoaDonVAT invoice, List<CartItem> items, IWebHostEnvironment env)
        {
            var stt = 0;
            var itemRows = string.Join("", items.Select(item =>
            {
                stt++;
                return $@"<tr>
                    <td style='padding:10px 8px;border:1px solid #dee2e6;text-align:center;font-size:14px;'>{stt:D2}</td>
                    <td style='padding:10px 8px;border:1px solid #dee2e6;font-size:14px;'>{item.TenVatTu}</td>
                    <td style='padding:10px 8px;border:1px solid #dee2e6;text-align:center;font-size:14px;'>{item.DonViTinh}</td>
                    <td style='padding:10px 8px;border:1px solid #dee2e6;text-align:center;font-size:14px;'>{item.SoLuong}</td>
                    <td style='padding:10px 8px;border:1px solid #dee2e6;text-align:right;font-size:14px;'>{item.DonGia:N0} ₫</td>
                    <td style='padding:10px 8px;border:1px solid #dee2e6;text-align:right;font-size:14px;'>{item.ThanhTien:N0} ₫</td>
                </tr>";
            }));

            var soHoaDon = invoice.SoHoaDon ?? "(Đang cập nhật)";
            var ngayLap = invoice.NgayLap.ToString("dd/MM/yyyy");
            
            // Prepare embedded images
            var images = new Dictionary<string, byte[]>();
            string logoCid = "logo";
            
            try 
            {
                string logoPath = Path.Combine(env.WebRootPath, "images", "khachhang", "Logo.jpg");
                if (System.IO.File.Exists(logoPath))
                {
                    using (var image = Image.Load(logoPath))
                    {
                        // Resize to width 150px, maintain aspect ratio
                        if (image.Width > 150)
                        {
                            image.Mutate(x => x.Resize(150, 0));
                        }
                        
                        using (var ms = new MemoryStream())
                        {
                            image.SaveAsJpeg(ms, new JpegEncoder { Quality = 75 });
                            images[logoCid] = ms.ToArray();
                        }
                    }
                }
            } 
            catch { /* Ignore if logo missing or error */ }

            // Use CID reference if image exists, otherwise placeholder
            var logoSrc = images.ContainsKey(logoCid) ? $"cid:{logoCid}" : "https://via.placeholder.com/80x80?text=Logo";
            var bidvQrUrl = "https://img.vietqr.io/image/BIDV-8861365240-compact.png";
            var bidvLogoUrl = "https://api.vietqr.io/img/BIDV.png";

            var html = $@"
            <div style='font-family:""Segoe UI"",Inter,Arial,sans-serif;max-width:900px;margin:0 auto;background:#ffffff;'>

                <!-- Company Header with gold border -->
                <div style='padding:24px 32px;border-bottom:3px solid #f59f00;'>
                    <table style='width:100%;'>
                        <tr>
                            <td style='vertical-align:top;width:90px;'>
                                <img src='{logoSrc}' alt='Logo' style='width:80px;height:80px;object-fit:contain;border-radius:8px;border:1px solid #ffecb5;' />
                            </td>
                            <td style='vertical-align:top;padding-left:16px;'>
                                <h2 style='margin:0 0 4px;color:#265077;text-transform:uppercase;font-size:18px;font-weight:700;'>{invoice.TenNguoiBan ?? "CÔNG TY VẬT TƯ XÂY DỰNG"}</h2>
                                <p style='margin:2px 0;color:#6c757d;font-size:13px;'>MST: {invoice.MaSoThueBan}</p>
                                <p style='margin:2px 0;color:#6c757d;font-size:13px;'>Địa chỉ: {invoice.DiaChiNguoiBan}</p>
                                <p style='margin:2px 0;color:#6c757d;font-size:13px;'>Hotline: 0909 000 000 • Email: contact@vlxdabc.vn</p>
                            </td>
                        </tr>
                    </table>
                </div>

                <!-- Invoice Title -->
                <div style='padding:24px 32px 16px;'>
                    <h1 style='margin:0 0 8px;color:#265077;font-size:22px;font-weight:700;text-transform:uppercase;'>HÓA ĐƠN GIÁ TRỊ GIA TĂNG (VAT)</h1>
                    <p style='margin:2px 0;color:#6c757d;font-size:13px;'>Mẫu số: 01GTKT • Ký hiệu: AA/23E • Số: <span style='color:#dc3545;font-weight:700;'>{soHoaDon}</span></p>
                    <p style='margin:2px 0;color:#6c757d;font-size:13px;'>Ngày lập: {ngayLap}</p>
                </div>

                <!-- Buyer Info Block -->
                <div style='margin:0 32px 24px;padding:16px 20px;background-color:#f8f9fa;border:1px solid #e9ecef;border-radius:6px;'>
                    <p style='margin:4px 0;font-size:14px;'><strong>Đơn vị mua hàng:</strong> {invoice.TenCongTy?.ToUpper()}</p>
                    <p style='margin:4px 0;font-size:14px;'><strong>MST:</strong> {invoice.MaSoThue}</p>
                    <p style='margin:4px 0;font-size:14px;'><strong>Địa chỉ:</strong> {invoice.DiaChiDKKD}</p>
                    <p style='margin:4px 0;font-size:14px;'><strong>Email:</strong> {invoice.EmailNhanHoaDon}</p>
                </div>

                <!-- Items Table -->
                <div style='padding:0 32px;'>
                    <table style='width:100%;border-collapse:collapse;'>
                        <thead>
                            <tr style='background-color:#f59f00;'>
                                <th style='padding:10px 8px;color:#ffffff;text-align:center;font-size:13px;font-weight:600;width:50px;'>STT</th>
                                <th style='padding:10px 8px;color:#ffffff;text-align:left;font-size:13px;font-weight:600;'>Tên hàng hóa</th>
                                <th style='padding:10px 8px;color:#ffffff;text-align:center;font-size:13px;font-weight:600;width:80px;'>ĐVT</th>
                                <th style='padding:10px 8px;color:#ffffff;text-align:center;font-size:13px;font-weight:600;width:60px;'>SL</th>
                                <th style='padding:10px 8px;color:#ffffff;text-align:right;font-size:13px;font-weight:600;width:120px;'>Đơn giá</th>
                                <th style='padding:10px 8px;color:#ffffff;text-align:right;font-size:13px;font-weight:600;width:140px;'>Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>{itemRows}</tbody>
                    </table>
                </div>

                <!-- Totals Section -->
                <div style='padding:16px 32px 24px;'>
                    <table style='width:100%;'>
                        <tr><td></td>
                            <td style='width:280px;'>
                                <table style='width:100%;'>
                                    <tr>
                                        <td style='padding:6px 0;color:#6c757d;font-size:14px;'>Cộng tiền hàng:</td>
                                        <td style='padding:6px 0;text-align:right;font-weight:700;font-size:14px;'>{invoice.TongTienTruocThue:N0} ₫</td>
                                    </tr>
                                    <tr style='border-bottom:1px solid #dee2e6;'>
                                        <td style='padding:6px 0 10px;color:#6c757d;font-size:14px;'>Thuế GTGT ({invoice.ThueSuat}%):</td>
                                        <td style='padding:6px 0 10px;text-align:right;font-weight:700;font-size:14px;'>{invoice.TienThue:N0} ₫</td>
                                    </tr>
                                    <tr>
                                        <td style='padding:12px 0 6px;font-weight:700;font-size:16px;color:#212529;'>Tổng thanh toán:</td>
                                        <td style='padding:12px 0 6px;text-align:right;font-weight:700;font-size:20px;color:#f59f00;'>{invoice.TongTienSauThue:N0} ₫</td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </div>

                <!-- Signature Area -->
                <div style='padding:0 32px 32px;'>
                    <table style='width:100%;text-align:center;'>
                        <tr>
                            <td style='width:33%;vertical-align:top;'>
                                <img src='{bidvQrUrl}' alt='BIDV QR' style='width:120px;height:auto;border:1px solid #dee2e6;border-radius:8px;margin:0 auto;display:block;' />
                                <p style='margin:8px 0 0;text-align:center;'><img src='{bidvLogoUrl}' alt='BIDV' style='height:16px;vertical-align:middle;' /> <span style='color:#6c757d;font-size:12px;'>BIDV</span></p>
                            </td>
                            <td style='width:33%;vertical-align:top;'>
                                <p style='font-weight:700;margin:0 0 60px;font-size:14px;'>Người bán hàng</p>
                                <p style='margin:0;color:#6c757d;font-style:italic;font-size:13px;'>(Ký, ghi rõ họ tên)</p>
                            </td>
                            <td style='width:33%;vertical-align:top;'>
                                <p style='font-weight:700;margin:0 0 60px;font-size:14px;'>Người mua hàng</p>
                                <p style='margin:0;color:#6c757d;font-style:italic;font-size:13px;'>(Ký, ghi rõ họ tên)</p>
                            </td>
                        </tr>
                    </table>
                </div>

                <!-- Footer Note -->
                <div style='padding:16px 32px;background-color:#f8f9fa;border-top:1px solid #e9ecef;text-align:center;'>
                    <p style='margin:0;color:#6c757d;font-size:12px;font-style:italic;'>Đây là hóa đơn giá trị gia tăng điện tử. Mọi thay đổi phải được xác nhận bằng chữ ký số của người bán.</p>
                    <p style='margin:6px 0 0;color:#6c757d;font-size:12px;'>📞 Hotline: 0909 000 000 • ✉ contact@vlxdabc.vn</p>
                </div>

            </div>";
            return (html, images);
        }
    }
}
