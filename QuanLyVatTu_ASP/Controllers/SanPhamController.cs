using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SanPhamController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(int[]? loaiIds, int[]? nhaCungCapIds, string[]? priceRanges, int page = 1)
        {
            var productsQuery = await _unitOfWork.VatTuRepository.GetAllAsync();
            var products = productsQuery.AsQueryable();

            // Filter by Categories (Multiple)
            if (loaiIds != null && loaiIds.Length > 0)
            {
                products = products.Where(p => loaiIds.Contains(p.MaLoaiVatTu));
            }

            // Filter by Suppliers (Multiple)
            if (nhaCungCapIds != null && nhaCungCapIds.Length > 0)
            {
                products = products.Where(p => nhaCungCapIds.Contains(p.MaNhaCungCap));
            }

            // Filter by Price Ranges (Multiple)
            if (priceRanges != null && priceRanges.Length > 0)
            {
                // Logic: (Range1 OR Range2 OR Range3...)
                // We build a predicate manually or just evaluate in memory since it's IEnumerable from Repository currently 
                // (Note: optimally this should be IQueryable in repo, but assuming IEnumerable for now)
                
                products = products.Where(p => 
                    (priceRanges.Contains("under1m") && p.GiaBan < 1000000) ||
                    (priceRanges.Contains("1m-2.5m") && p.GiaBan >= 1000000 && p.GiaBan <= 2500000) ||
                    (priceRanges.Contains("2.5m-5m") && p.GiaBan > 2500000 && p.GiaBan <= 5000000) ||
                    (priceRanges.Contains("5m-10m") && p.GiaBan > 5000000 && p.GiaBan <= 10000000) ||
                    (priceRanges.Contains("above10m") && p.GiaBan > 10000000)
                );
            }

            ViewBag.Categories = await _unitOfWork.LoaiVatTuRepository.GetAllAsync();
            ViewBag.Suppliers = await _unitOfWork.NhaCungCapRepository.GetAllAsync();
            
            // Retain filter state
            ViewBag.CurrentLoaiIds = loaiIds ?? Array.Empty<int>();
            ViewBag.CurrentNhaCungCapIds = nhaCungCapIds ?? Array.Empty<int>();
            ViewBag.CurrentPriceRanges = priceRanges ?? Array.Empty<string>();

            // Pagination
            int pageSize = 15;
            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            // Keep 'Global' min/max just in case we need context, though mostly unused now
            ViewBag.GlobalMinPrice = 0;
            ViewBag.GlobalMaxPrice = 10000000;

            return View(pagedProducts);
        }
        public async Task<IActionResult> ProductDetail(int id)
        {
            var product = await _unitOfWork.VatTuRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            
            // Get Category Name
            var category = await _unitOfWork.LoaiVatTuRepository.GetByIdAsync(product.MaLoaiVatTu);
            ViewBag.TenLoai = category?.TenLoaiVatTu;

            // Get Related Products
            var allProducts = await _unitOfWork.VatTuRepository.GetAllAsync();
            var relatedProducts = allProducts
                .Where(p => p.MaLoaiVatTu == product.MaLoaiVatTu && p.ID != id)
                .Take(4)
                .ToList();
            ViewBag.RelatedProducts = relatedProducts;

            ViewBag.RelatedProducts = relatedProducts;

            // Get Reviews
            ViewBag.Reviews = await _unitOfWork.DanhGiaRepository.GetByProductIdAsync(id);

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(int productId, int rating, string comment)
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để đánh giá." });
            }

            var customer = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (customer == null)
            {
                // Fallback for hardcoded users like 'client@gmail.com' if not in DB, 
                // but usually Client should be in DB. If not, we can't save FK.
                return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng trong hệ thống." });
            }

            if (string.IsNullOrEmpty(comment) || rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var review = new QuanLyVatTu_ASP.Areas.Admin.Models.DanhGia
            {
                MaVatTu = productId,
                SoSao = rating,
                MaKhachHang = customer.ID,
                BinhLuan = comment,
                NgayDanhGia = DateTime.Now
            };

            await _unitOfWork.DanhGiaRepository.AddAsync(review);
            _unitOfWork.Save();

            return Json(new { success = true });
        }
    }
}