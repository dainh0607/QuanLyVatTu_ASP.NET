using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.NhanVien;
using QuanLyVatTu_ASP.DataAccess;
using BCryptNet = BCrypt.Net.BCrypt;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/nhan-vien")]
    public class NhanVienController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public NhanVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /admin/nhan-vien
        [HttpGet("")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            if (page < 1) page = 1;

            var query = _context.NhanViens.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.MaHienThi.Contains(keyword) ||
                    x.HoTen.Contains(keyword) ||
                    x.CCCD.Contains(keyword) ||
                    x.SoDienThoai.Contains(keyword) ||
                    x.VaiTro.Contains(keyword));
                ViewBag.Keyword = keyword;
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.NgayTao)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(x => new NhanVienIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi,
                    HoTen = x.HoTen,
                    NgaySinh = x.NgaySinh,
                    CCCD = x.CCCD,
                    SoDienThoai = x.SoDienThoai,
                    Email = x.Email,
                    VaiTro = x.VaiTro,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            var model = new NhanVienIndexViewModel
            {
                Items = items,
                PageIndex = page,
                PageSize = PageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)PageSize)
            };

            return View(model);
        }

        // GET: /admin/nhan-vien/them-moi
        [HttpGet("them-moi")]
        public IActionResult Create()
        {
            return View(new NhanVienCreateEditViewModel
            {
                NgaySinh = DateTime.Now.AddYears(-22)
            });
        }

        // POST: /admin/nhan-vien/them-moi
        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVienCreateEditViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.MatKhau))
                ModelState.AddModelError("MatKhau", "Vui lòng nhập mật khẩu.");

            if (await _context.NhanViens.AnyAsync(x => x.CCCD == model.CCCD))
                ModelState.AddModelError("CCCD", "CCCD này đã tồn tại.");

            if (await _context.NhanViens.AnyAsync(x => x.SoDienThoai == model.SoDienThoai))
                ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã tồn tại.");

            if (!string.IsNullOrEmpty(model.Email) && await _context.NhanViens.AnyAsync(x => x.Email == model.Email))
                ModelState.AddModelError("Email", "Email này đã tồn tại.");

            if (await _context.NhanViens.AnyAsync(x => x.TaiKhoan == model.TaiKhoan))
                ModelState.AddModelError("TaiKhoan", "Tài khoản đăng nhập đã tồn tại.");

            if (ModelState.IsValid)
            {
                var nhanVien = new NhanVien
                {
                    HoTen = model.HoTen,
                    NgaySinh = model.NgaySinh,
                    CCCD = model.CCCD,
                    SoDienThoai = model.SoDienThoai,
                    Email = model.Email,
                    TaiKhoan = model.TaiKhoan,
                    MatKhau = BCryptNet.HashPassword(model.MatKhau),
                    VaiTro = model.VaiTro,
                    NgayTao = DateTime.Now
                };

                _context.NhanViens.Add(nhanVien);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm nhân viên thành công";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: /admin/nhan-vien/sua/5
        [HttpGet("sua/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return NotFound();

            var model = new NhanVienCreateEditViewModel
            {
                Id = nhanVien.ID,
                HoTen = nhanVien.HoTen,
                NgaySinh = nhanVien.NgaySinh,
                CCCD = nhanVien.CCCD,
                SoDienThoai = nhanVien.SoDienThoai,
                Email = nhanVien.Email,
                TaiKhoan = nhanVien.TaiKhoan,
                VaiTro = nhanVien.VaiTro
            };

            return View(model);
        }

        // POST: /admin/nhan-vien/sua/5
        [HttpPost("sua/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NhanVienCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (string.IsNullOrWhiteSpace(model.MatKhau))
                ModelState.Remove("MatKhau");

            if (await _context.NhanViens.AnyAsync(x => x.CCCD == model.CCCD && x.ID != id))
                ModelState.AddModelError("CCCD", "CCCD đã được sử dụng bởi nhân viên khác.");

            if (await _context.NhanViens.AnyAsync(x => x.SoDienThoai == model.SoDienThoai && x.ID != id))
                ModelState.AddModelError("SoDienThoai", "SĐT đã được sử dụng bởi nhân viên khác.");

            if (!string.IsNullOrEmpty(model.Email) && await _context.NhanViens.AnyAsync(x => x.Email == model.Email && x.ID != id))
                ModelState.AddModelError("Email", "Email đã được sử dụng bởi nhân viên khác.");

            if (await _context.NhanViens.AnyAsync(x => x.TaiKhoan == model.TaiKhoan && x.ID != id))
                ModelState.AddModelError("TaiKhoan", "Tài khoản này đã tồn tại.");

            if (ModelState.IsValid)
            {
                var nhanVien = await _context.NhanViens.FindAsync(id);
                if (nhanVien == null) return NotFound();

                nhanVien.HoTen = model.HoTen;
                nhanVien.NgaySinh = model.NgaySinh;
                nhanVien.CCCD = model.CCCD;
                nhanVien.SoDienThoai = model.SoDienThoai;
                nhanVien.Email = model.Email;
                nhanVien.TaiKhoan = model.TaiKhoan;
                nhanVien.VaiTro = model.VaiTro;

                if (!string.IsNullOrEmpty(model.MatKhau))
                {
                    nhanVien.MatKhau = BCryptNet.HashPassword(model.MatKhau);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thông tin thành công";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: /admin/nhan-vien/xoa/5
        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                TempData["Error"] = "Nhân viên không tồn tại";
                return RedirectToAction(nameof(Index));
            }

            bool hasDonHang = await _context.DonHang.AnyAsync(d => d.NhanVienId == id);

            if (hasDonHang)
            {
                TempData["Error"] = "Không thể xóa! Nhân viên này đã có lịch sử lập đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            _context.NhanViens.Remove(nhanVien);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa nhân viên";
            return RedirectToAction(nameof(Index));
        }
    }
}