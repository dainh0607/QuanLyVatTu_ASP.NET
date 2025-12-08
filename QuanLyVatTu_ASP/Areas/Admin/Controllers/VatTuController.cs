using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.VatTu;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/vat-tu")]
    public class VatTuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public VatTuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /admin/vat-tu
        [HttpGet("")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            if (page < 1) page = 1;

            var query = _context.VatTus
                .Include(v => v.LoaiVatTu)
                .Include(v => v.NhaCungCap)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.MaHienThi.Contains(keyword) ||
                    x.TenVatTu.Contains(keyword) ||
                    x.DonViTinh.Contains(keyword) ||
                    x.LoaiVatTu.TenLoaiVatTu.Contains(keyword) ||
                    x.NhaCungCap.TenNhaCungCap.Contains(keyword));
                ViewBag.Keyword = keyword;
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.NgayTao)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(x => new VatTuIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi,
                    TenVatTu = x.TenVatTu,
                    DonViTinh = x.DonViTinh,
                    GiaNhap = x.GiaNhap,
                    GiaBan = x.GiaBan,
                    SoLuongTon = x.SoLuongTon,
                    TenLoaiVatTu = x.LoaiVatTu.TenLoaiVatTu,
                    TenNhaCungCap = x.NhaCungCap.TenNhaCungCap,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            var model = new VatTuIndexViewModel
            {
                Items = items,
                PageIndex = page,
                PageSize = PageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)PageSize)
            };

            return View(model);
        }

        // GET: /admin/vat-tu/them-moi
        [HttpGet("them-moi")]
        public async Task<IActionResult> Create()
        {
            await PrepareViewBag();
            return View(new VatTuCreateEditViewModel());
        }

        // POST: /admin/vat-tu/them-moi
        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VatTuCreateEditViewModel model)
        {
            if (await _context.VatTus.AnyAsync(x => x.TenVatTu == model.TenVatTu))
            {
                ModelState.AddModelError("TenVatTu", "Tên vật tư này đã tồn tại trong kho.");
            }

            if (ModelState.IsValid)
            {
                var vt = new VatTu
                {
                    TenVatTu = model.TenVatTu,
                    DonViTinh = model.DonViTinh,
                    GiaNhap = model.GiaNhap,
                    GiaBan = model.GiaBan,
                    SoLuongTon = model.SoLuongTon,
                    MaLoaiVatTu = model.MaLoaiVatTu,
                    MaNhaCungCap = model.MaNhaCungCap,
                    NgayTao = DateTime.Now
                };

                _context.VatTus.Add(vt);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm vật tư thành công";
                return RedirectToAction(nameof(Index));
            }

            await PrepareViewBag(model.MaLoaiVatTu, model.MaNhaCungCap);
            return View(model);
        }

        // GET: /admin/vat-tu/sua/5
        [HttpGet("sua/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var vt = await _context.VatTus.FindAsync(id);
            if (vt == null) return NotFound();

            var model = new VatTuCreateEditViewModel
            {
                Id = vt.ID,
                TenVatTu = vt.TenVatTu,
                DonViTinh = vt.DonViTinh,
                GiaNhap = vt.GiaNhap,
                GiaBan = vt.GiaBan,
                SoLuongTon = vt.SoLuongTon,
                MaLoaiVatTu = vt.MaLoaiVatTu,
                MaNhaCungCap = vt.MaNhaCungCap
            };

            await PrepareViewBag(model.MaLoaiVatTu, model.MaNhaCungCap);
            return View(model);
        }

        // POST: /admin/vat-tu/sua/5
        [HttpPost("sua/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VatTuCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (await _context.VatTus.AnyAsync(x => x.TenVatTu == model.TenVatTu && x.ID != id))
            {
                ModelState.AddModelError("TenVatTu", "Tên vật tư đã được sử dụng.");
            }

            if (ModelState.IsValid)
            {
                var vt = await _context.VatTus.FindAsync(id);
                if (vt == null) return NotFound();

                vt.TenVatTu = model.TenVatTu;
                vt.DonViTinh = model.DonViTinh;
                vt.GiaNhap = model.GiaNhap;
                vt.GiaBan = model.GiaBan;
                vt.SoLuongTon = model.SoLuongTon;
                vt.MaLoaiVatTu = model.MaLoaiVatTu;
                vt.MaNhaCungCap = model.MaNhaCungCap;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật vật tư thành công";
                return RedirectToAction(nameof(Index));
            }

            await PrepareViewBag(model.MaLoaiVatTu, model.MaNhaCungCap);
            return View(model);
        }

        // POST: /admin/vat-tu/xoa/5
        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var vt = await _context.VatTus.FindAsync(id);
            if (vt == null)
            {
                TempData["Error"] = "Vật tư không tồn tại";
                return RedirectToAction(nameof(Index));
            }

            bool inDonHang = await _context.ChiTietDonHangs.AnyAsync(ct => ct.MaVatTu == id);
            bool inHoaDon = await _context.ChiTietHoaDons.AnyAsync(ct => ct.MaVatTu == id);

            if (inDonHang || inHoaDon)
            {
                TempData["Error"] = "Không thể xóa! Vật tư này đã phát sinh giao dịch (Đơn hàng/Hóa đơn).";
                return RedirectToAction(nameof(Index));
            }

            _context.VatTus.Remove(vt);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa vật tư khỏi kho";
            return RedirectToAction(nameof(Index));
        }

        private async Task PrepareViewBag(int? selectedLoai = null, int? selectedNCC = null)
        {
            ViewBag.LoaiVatTuList = new SelectList(await _context.LoaiVatTus
                .Select(l => new { l.ID, l.TenLoaiVatTu }).ToListAsync(), "ID", "TenLoaiVatTu", selectedLoai);

            ViewBag.NhaCungCapList = new SelectList(await _context.NhaCungCaps
                .Select(n => new { n.ID, n.TenNhaCungCap }).ToListAsync(), "ID", "TenNhaCungCap", selectedNCC);
        }
    }
}