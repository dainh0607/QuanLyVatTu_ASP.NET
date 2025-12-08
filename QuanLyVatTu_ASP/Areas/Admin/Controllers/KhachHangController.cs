using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.KhachHangViewModels;
using Microsoft.EntityFrameworkCore;
using BCryptNet = BCrypt.Net.BCrypt;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/khach-hang")]
    public class KhachHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public KhachHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /admin/khach-hang
        [HttpGet("")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            if (page < 1) page = 1;

            var query = _context.KhachHangs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.MaHienThi.Contains(keyword) ||
                    x.HoTen.Contains(keyword) ||
                    x.Email.Contains(keyword) ||
                    (x.SoDienThoai != null && x.SoDienThoai.Contains(keyword)));
                ViewBag.Keyword = keyword;
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.NgayTao)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(x => new KhachHangIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi,
                    HoTen = x.HoTen,
                    Email = x.Email,
                    SoDienThoai = x.SoDienThoai,
                    DiaChi = x.DiaChi,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            var model = new KhachHangIndexViewModel
            {
                Items = items,
                PageIndex = page,
                PageSize = PageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)PageSize)
            };

            return View(model);
        }

        // GET: /admin/khach-hang/them-moi
        [HttpGet("them-moi")]
        public IActionResult Create()
        {
            return View(new KhachHangCreateEditViewModel());
        }

        // POST: /admin/khach-hang/them-moi
        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhachHangCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var kh = new KhachHang
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    DiaChi = model.DiaChi,
                    TaiKhoan = model.TaiKhoan,
                    MatKhau = BCryptNet.HashPassword(model.MatKhau),
                    DangNhapGoogle = false,
                    NgayTao = DateTime.Now
                };

                _context.KhachHangs.Add(kh);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: /admin/khach-hang/sua/5
        [HttpGet("sua/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return NotFound();

            var model = new KhachHangCreateEditViewModel
            {
                Id = kh.ID,
                HoTen = kh.HoTen,
                Email = kh.Email,
                SoDienThoai = kh.SoDienThoai,
                DiaChi = kh.DiaChi,
                TaiKhoan = kh.TaiKhoan
                // Không trả về MatKhau
            };

            return View(model);
        }

        // POST: /admin/khach-hang/sua/5
        [HttpPost("sua/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhachHangCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var kh = await _context.KhachHangs.FindAsync(id);
                if (kh == null) return NotFound();

                kh.HoTen = model.HoTen;
                kh.Email = model.Email;
                kh.SoDienThoai = model.SoDienThoai;
                kh.DiaChi = model.DiaChi;
                kh.TaiKhoan = model.TaiKhoan;

                if (!string.IsNullOrEmpty(model.MatKhau))
                {
                    kh.MatKhau = BCryptNet.HashPassword(model.MatKhau);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: /admin/khach-hang/xoa/5
        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh != null)
            {
                _context.KhachHangs.Remove(kh);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}