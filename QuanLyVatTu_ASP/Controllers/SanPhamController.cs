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

            return View(product);
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo từ khóa
        /// </summary>
        public async Task<IActionResult> Search(string q, int page = 1)
        {
            var allProducts = await _unitOfWork.VatTuRepository.GetAllAsync();
            var products = allProducts.AsQueryable();

            ViewBag.SearchQuery = q;

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim().ToLower();
                products = products.Where(p =>
                    (p.TenVatTu != null && p.TenVatTu.ToLower().Contains(keyword)) ||
                    (p.MoTa != null && p.MoTa.ToLower().Contains(keyword))
                );
            }

            // Pagination
            int pageSize = 15;
            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            // Get categories and suppliers for sidebar
            ViewBag.Categories = await _unitOfWork.LoaiVatTuRepository.GetAllAsync();
            ViewBag.Suppliers = await _unitOfWork.NhaCungCapRepository.GetAllAsync();

            return View("Index", pagedProducts);
        }
    }
}