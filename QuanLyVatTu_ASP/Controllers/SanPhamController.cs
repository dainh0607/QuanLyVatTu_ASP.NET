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

        public async Task<IActionResult> Index(int? loaiId)
        {
            var products = await _unitOfWork.VatTuRepository.GetAllAsync();

            if (loaiId.HasValue)
            {
                products = products.Where(p => p.MaLoaiVatTu == loaiId.Value);
            }
            ViewBag.Categories = await _unitOfWork.LoaiVatTuRepository.GetAllAsync();

            return View(products);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _unitOfWork.VatTuRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            var category = await _unitOfWork.LoaiVatTuRepository.GetByIdAsync(product.MaLoaiVatTu);
            ViewBag.TenLoai = category?.TenLoaiVatTu;

            return View(product);
        }
    }
}