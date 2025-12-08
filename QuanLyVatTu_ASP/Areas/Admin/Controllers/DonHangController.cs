using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels;
using QuanLyVatTu_ASP.DataAccess;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/don-hang")]
    public class DonHangController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public DonHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /admin/don-hang
        [HttpGet("")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            if (page < 1) page = 1;

            var query = _context.DonHang
                .Include(d => d.KhachHang)
                .Include(d => d.NhanVien)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.MaHienThi.Contains(keyword) ||
                    (x.GhiChu != null && x.GhiChu.Contains(keyword)) ||
                    x.KhachHang.HoTen.Contains(keyword));
                ViewBag.Keyword = keyword;
            }

            int pageSize = 10;
            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DonHangIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi ?? "DH" + x.ID.ToString("0000"),
                    
                    TenKhachHang = x.KhachHang.HoTen, 
                    TenNhanVien = x.NhanVien != null ? x.NhanVien.HoTen : "", 

                    NgayDat = x.NgayDat,
                    TongTien = x.TongTien,
                    SoTienDatCoc = x.SoTienDatCoc ?? 0,
                    PhuongThucDatCoc = x.PhuongThucDatCoc,
                    NgayDatCoc = x.NgayDatCoc,
                    TrangThai = string.IsNullOrWhiteSpace(x.TrangThai) ? "Chờ xác nhận" : x.TrangThai,
                    GhiChu = x.GhiChu,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            var model = new DonHangIndexViewModel
            {
                Items = items,
                PageIndex = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };

            return View(model);
        }

        // GET: /admin/don-hang/them-moi
        [HttpGet("them-moi")]
        public async Task<IActionResult> Create()
        {
            ViewBag.KhachHangList = new SelectList(await _context.KhachHangs.Select(k => new { k.ID, k.HoTen }).ToListAsync(), "ID", "HoTen");
            ViewBag.NhanVienList = new SelectList(await _context.NhanViens.Select(n => new { n.ID, n.HoTen }).ToListAsync(), "ID", "HoTen");

            return View(new DonHangCreateEditViewModel
            {
                NgayDat = DateTime.Now,
                TrangThai = "Chờ xác nhận"
            });
        }

        // POST: /admin/don-hang/them-moi
        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonHangCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = new DonHang
                {
                    KhachHangId = model.KhachHangId,
                    NhanVienId = model.NhanVienId ?? 0, 
                    NgayDat = model.NgayDat,
                    TongTien = model.TongTien,
                    SoTienDatCoc = model.SoTienDatCoc,
                    PhuongThucDatCoc = model.PhuongThucDatCoc,
                    NgayDatCoc = model.NgayDatCoc,
                    TrangThai = model.TrangThai ?? "Chờ xác nhận",
                    GhiChu = model.GhiChu,
                    NgayTao = DateTime.Now
                };

                _context.DonHang.Add(entity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.KhachHangList = new SelectList(await _context.KhachHangs.Select(k => new { k.ID, k.HoTen }).ToListAsync(), "ID", "HoTen", model.KhachHangId);
            ViewBag.NhanVienList = new SelectList(await _context.NhanViens.Select(n => new { n.ID, n.HoTen }).ToListAsync(), "ID", "HoTen", model.NhanVienId);
            return View(model);
        }

        // GET: /admin/don-hang/sua/5
        [HttpGet("sua/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.DonHang.FindAsync(id);
            if (entity == null) return NotFound();

            var model = new DonHangCreateEditViewModel
            {
                Id = entity.ID,
                MaHienThi = entity.MaHienThi,
                KhachHangId = entity.KhachHangId ?? 0,
                NhanVienId = entity.NhanVienId,
                NgayDat = entity.NgayDat,
                TongTien = entity.TongTien,
                SoTienDatCoc = entity.SoTienDatCoc,
                PhuongThucDatCoc = entity.PhuongThucDatCoc,
                NgayDatCoc = entity.NgayDatCoc,
                TrangThai = entity.TrangThai ?? "Chờ xác nhận",
                GhiChu = entity.GhiChu
            };

            ViewBag.KhachHangList = new SelectList(await _context.KhachHangs.Select(k => new { k.ID, k.HoTen }).ToListAsync(), "ID", "HoTen", model.KhachHangId);
            ViewBag.NhanVienList = new SelectList(await _context.NhanViens.Select(n => new { n.ID, n.HoTen }).ToListAsync(), "ID", "HoTen", model.NhanVienId);

            return View(model);
        }

        // POST: /admin/don-hang/sua/5
        [HttpPost("sua/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DonHangCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var entity = await _context.DonHang.FindAsync(id);
                if (entity == null) return NotFound();

                entity.KhachHangId = model.KhachHangId;
                entity.NhanVienId = model.NhanVienId ?? 0;
                entity.NgayDat = model.NgayDat;

                entity.SoTienDatCoc = model.SoTienDatCoc;
                entity.PhuongThucDatCoc = model.PhuongThucDatCoc;
                entity.NgayDatCoc = model.NgayDatCoc;
                entity.TrangThai = model.TrangThai ?? "Chờ xác nhận";
                entity.GhiChu = model.GhiChu;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.KhachHangList = new SelectList(await _context.KhachHangs.Select(k => new { k.ID, k.HoTen }).ToListAsync(), "ID", "HoTen", model.KhachHangId);
            ViewBag.NhanVienList = new SelectList(await _context.NhanViens.Select(n => new { n.ID, n.HoTen }).ToListAsync(), "ID", "HoTen", model.NhanVienId);
            return View(model);
        }

        // POST: /admin/don-hang/xoa/5
        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.DonHang.FindAsync(id);
            if (entity != null)
            {
                _context.DonHang.Remove(entity);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}