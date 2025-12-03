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

            var query = _context.NhaCungCaps.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.MaHienThi.Contains(keyword) ||
                    x.TenNhaCungCap.Contains(keyword) ||
                    x.Email.Contains(keyword) ||
                    (x.SoDienThoai != null && x.SoDienThoai.Contains(keyword)));
                ViewBag.Keyword = keyword;
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.TenNhaCungCap)
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

            if (ModelState.IsValid)
            {
                var ncc = await _context.NhaCungCaps.FindAsync(id);
                if (ncc == null) return NotFound();

                ncc.TenNhaCungCap = model.TenNhaCungCap;
                ncc.Email = model.Email;
                ncc.SoDienThoai = model.SoDienThoai;
                ncc.DiaChi = model.DiaChi;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: /admin/nha-cung-cap/xoa/5
        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ncc = await _context.NhaCungCaps.FindAsync(id);
            if (ncc != null)
            {
                _context.NhaCungCaps.Remove(ncc);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}