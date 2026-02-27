using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu.Areas.Admin.Controllers;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    // [Authorize] không dùng vì hệ thống dùng Session-based Auth qua [Authentication] ở AdminBaseController
    [Area("Admin")]
    [Route("admin/danh-gia")]
    public class DanhGiaController : AdminBaseController
    {
        private readonly AppDbContext _context;

        public DanhGiaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /admin/danh-gia
        [HttpGet("", Name = "AdminDanhGia")]
        public async Task<IActionResult> Index(int? filterSao)
        {
            // Lọc theo role: Chỉ "Quản trị" được truy cập
            var role = HttpContext.Session.GetString("Role");
            if (role != "Quản trị")
                return RedirectToAction("Index", "DonHang", new { area = "Admin" });

            var danhGias = _context.DanhGias
                .Include(d => d.KhachHang)
                .Include(d => d.VatTu)
                .AsQueryable();

            if (filterSao.HasValue && filterSao.Value >= 1 && filterSao.Value <= 5)
            {
                danhGias = danhGias.Where(d => d.SoSao == filterSao.Value);
            }

            // Mặc định sắp xếp đánh giá mới nhất lên đầu
            var result = await danhGias.OrderByDescending(d => d.NgayDanhGia).ToListAsync();

            ViewBag.FilterSao = filterSao;

            return View(result);
        }

        // POST: /admin/danh-gia/reply
        [HttpPost("reply")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, string phanHoi)
        {
            var danhGia = await _context.DanhGias.FindAsync(id);
            if (danhGia == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(phanHoi))
            {
                danhGia.PhanHoi = phanHoi;
                danhGia.NgayPhanHoi = DateTime.Now;
                _context.Update(danhGia);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Phản hồi đánh giá thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Nội dung phản hồi không được để trống.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /admin/danh-gia/delete/5
        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var danhGia = await _context.DanhGias.FindAsync(id);
            if (danhGia != null)
            {
                _context.DanhGias.Remove(danhGia);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa đánh giá thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đánh giá để xóa.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
