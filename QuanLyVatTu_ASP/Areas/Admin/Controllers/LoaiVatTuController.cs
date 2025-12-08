using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.Admin.LoaiVatTu;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.LoaiVatTu;
using QuanLyVatTu_ASP.DataAccess;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/loai-vat-tu")]
    public class LoaiVatTuController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public LoaiVatTuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /admin/loai-vat-tu
        [HttpGet("")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            if (page < 1) page = 1;

            var query = _context.LoaiVatTus.Include(x => x.VatTus).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.MaHienThi.Contains(keyword) ||
                    x.TenLoaiVatTu.Contains(keyword) ||
                    (x.MoTa != null && x.MoTa.Contains(keyword)));
                ViewBag.Keyword = keyword;
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.NgayTao)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(x => new LoaiVatTuIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi,
                    TenLoaiVatTu = x.TenLoaiVatTu,
                    MoTa = x.MoTa,
                    SoLuongVatTu = x.VatTus.Count,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            var model = new LoaiVatTuIndexViewModel
            {
                Items = items,
                PageIndex = page,
                PageSize = PageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)PageSize)
            };

            return View(model);
        }

        // GET: /admin/loai-vat-tu/them-moi
        [HttpGet("them-moi")]
        public IActionResult Create()
        {
            return View(new LoaiVatTuCreateEditViewModel());
        }

        // POST: /admin/loai-vat-tu/them-moi
        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoaiVatTuCreateEditViewModel model)
        {
            if (await _context.LoaiVatTus.AnyAsync(x => x.TenLoaiVatTu == model.TenLoaiVatTu))
            {
                ModelState.AddModelError("TenLoaiVatTu", "Tên loại vật tư này đã tồn tại");
            }

            if (ModelState.IsValid)
            {
                var loai = new LoaiVatTu
                {
                    TenLoaiVatTu = model.TenLoaiVatTu,
                    MoTa = model.MoTa,
                    NgayTao = DateTime.Now
                };

                _context.LoaiVatTus.Add(loai);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm mới thành công";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: /admin/loai-vat-tu/sua/5
        [HttpGet("sua/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var loai = await _context.LoaiVatTus.FindAsync(id);
            if (loai == null) return NotFound();

            var model = new LoaiVatTuCreateEditViewModel
            {
                Id = loai.ID,
                TenLoaiVatTu = loai.TenLoaiVatTu,
                MoTa = loai.MoTa
            };

            return View(model);
        }

        // POST: /admin/loai-vat-tu/sua/5
        [HttpPost("sua/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LoaiVatTuCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            // FIX 3: Kiểm tra trùng tên (trừ chính nó)
            if (await _context.LoaiVatTus.AnyAsync(x => x.TenLoaiVatTu == model.TenLoaiVatTu && x.ID != id))
            {
                ModelState.AddModelError("TenLoaiVatTu", "Tên loại vật tư đã được sử dụng");
            }

            if (ModelState.IsValid)
            {
                var loai = await _context.LoaiVatTus.FindAsync(id);
                if (loai == null) return NotFound();

                loai.TenLoaiVatTu = model.TenLoaiVatTu;
                loai.MoTa = model.MoTa;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thành công";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: /admin/loai-vat-tu/xoa/5
        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var loai = await _context.LoaiVatTus
                .Include(x => x.VatTus)
                .FirstOrDefaultAsync(x => x.ID == id);

            if (loai == null)
            {
                TempData["Error"] = "Loại vật tư không tồn tại";
                return RedirectToAction(nameof(Index));
            }

            if (loai.VatTus != null && loai.VatTus.Any())
            {
                TempData["Error"] = $"Không thể xóa! Có {loai.VatTus.Count} vật tư đang thuộc loại này.";
                return RedirectToAction(nameof(Index));
            }

            _context.LoaiVatTus.Remove(loai);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa loại vật tư";
            return RedirectToAction(nameof(Index));
        }
    }
}