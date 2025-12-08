using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.NhaCungCap;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/nha-cung-cap")]
    public class NhaCungCapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public NhaCungCapController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /admin/nha-cung-cap
        [HttpGet("")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            if (page < 1) page = 1;

            var query = _context.NhaCungCaps.Include(x => x.VatTus).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.MaHienThi.Contains(keyword) ||
                    x.TenNhaCungCap.Contains(keyword) ||
                    (x.Email != null && x.Email.Contains(keyword)) ||
                    (x.SoDienThoai != null && x.SoDienThoai.Contains(keyword)));
                ViewBag.Keyword = keyword;
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.NgayTao)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(x => new NhaCungCapIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi,
                    TenNhaCungCap = x.TenNhaCungCap,
                    Email = x.Email,
                    SoDienThoai = x.SoDienThoai,
                    DiaChi = x.DiaChi,
                    SoLuongVatTuCungCap = x.VatTus.Count,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            var model = new NhaCungCapIndexViewModel
            {
                Items = items,
                PageIndex = page,
                PageSize = PageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)PageSize)
            };

            return View(model);
        }

        // GET: /admin/nha-cung-cap/them-moi
        [HttpGet("them-moi")]
        public IActionResult Create()
        {
            return View(new NhaCungCapCreateEditViewModel());
        }

        // POST: /admin/nha-cung-cap/them-moi
        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhaCungCapCreateEditViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Email) && await _context.NhaCungCaps.AnyAsync(x => x.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã tồn tại trong hệ thống.");
            }
            if (await _context.NhaCungCaps.AnyAsync(x => x.TenNhaCungCap == model.TenNhaCungCap))
            {
                ModelState.AddModelError("TenNhaCungCap", "Tên nhà cung cấp này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                var ncc = new NhaCungCap
                {
                    TenNhaCungCap = model.TenNhaCungCap,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    DiaChi = model.DiaChi,
                    NgayTao = DateTime.Now
                };

                _context.NhaCungCaps.Add(ncc);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm nhà cung cấp thành công";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: /admin/nha-cung-cap/sua/5
        [HttpGet("sua/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var ncc = await _context.NhaCungCaps.FindAsync(id);
            if (ncc == null) return NotFound();

            var model = new NhaCungCapCreateEditViewModel
            {
                Id = ncc.ID,
                TenNhaCungCap = ncc.TenNhaCungCap,
                Email = ncc.Email,
                SoDienThoai = ncc.SoDienThoai,
                DiaChi = ncc.DiaChi
            };

            return View(model);
        }

        // POST: /admin/nha-cung-cap/sua/5
        [HttpPost("sua/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NhaCungCapCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!string.IsNullOrEmpty(model.Email) &&
                await _context.NhaCungCaps.AnyAsync(x => x.Email == model.Email && x.ID != id))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng bởi nhà cung cấp khác.");
            }

            if (await _context.NhaCungCaps.AnyAsync(x => x.TenNhaCungCap == model.TenNhaCungCap && x.ID != id))
            {
                ModelState.AddModelError("TenNhaCungCap", "Tên nhà cung cấp này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                var ncc = await _context.NhaCungCaps.FindAsync(id);
                if (ncc == null) return NotFound();

                ncc.TenNhaCungCap = model.TenNhaCungCap;
                ncc.Email = model.Email;
                ncc.SoDienThoai = model.SoDienThoai;
                ncc.DiaChi = model.DiaChi;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thành công";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: /admin/nha-cung-cap/xoa/5
        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ncc = await _context.NhaCungCaps
                .Include(x => x.VatTus)
                .FirstOrDefaultAsync(x => x.ID == id);

            if (ncc == null)
            {
                TempData["Error"] = "Nhà cung cấp không tồn tại";
                return RedirectToAction(nameof(Index));
            }

            if (ncc.VatTus != null && ncc.VatTus.Any())
            {
                TempData["Error"] = $"Không thể xóa! Nhà cung cấp này đang cung cấp {ncc.VatTus.Count} vật tư.";
                return RedirectToAction(nameof(Index));
            }

            _context.NhaCungCaps.Remove(ncc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa nhà cung cấp";
            return RedirectToAction(nameof(Index));
        }
    }
}