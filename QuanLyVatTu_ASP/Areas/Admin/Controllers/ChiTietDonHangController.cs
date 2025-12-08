using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels;
using QuanLyVatTu_ASP.DataAccess;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/chi-tiet-don-hang")]
    public class ChiTietDonHangController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public ChiTietDonHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id, string search = "")
        {
            if (!await _context.DonHang.AnyAsync(d => d.ID == id))
                return NotFound();

            var model = await LoadChiTietDonHang(id, search);
            return View("~/Areas/Admin/Views/DonHang/Details.cshtml", model);
        }

        // URL: /admin/chi-tiet-don-hang/them-vat-tu (POST)
        [HttpPost("them-vat-tu")]
        public async Task<IActionResult> ThemVatTu(int maDonHang, int maVatTu, int soLuong = 1)
        {
            if (maVatTu <= 0 || soLuong <= 0)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ";
                return RedirectToAction(nameof(Details), new { id = maDonHang });
            }

            var vatTu = await _context.VatTus.FindAsync(maVatTu);
            if (vatTu == null)
            {
                TempData["Error"] = "Vật tư không tồn tại";
                return RedirectToAction(nameof(Details), new { id = maDonHang });
            }

            var exist = await _context.ChiTietDonHangs
                .FirstOrDefaultAsync(c => c.MaDonHang == maDonHang && c.MaVatTu == maVatTu);

            if (exist != null)
            {
                exist.SoLuong += soLuong;
            }
            else
            {
                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDonHang = maDonHang,
                    MaVatTu = maVatTu,
                    SoLuong = soLuong,
                    DonGia = vatTu.GiaBan
                });
            }

            await _context.SaveChangesAsync();
            await CapNhatTongTienDonHang(maDonHang);

            TempData["Success"] = "Đã thêm vật tư vào đơn hàng";
            return RedirectToAction(nameof(Details), new { id = maDonHang });
        }

        // URL: /admin/chi-tiet-don-hang/sua-so-luong (POST)
        [HttpPost("sua-so-luong")]
        public async Task<IActionResult> SuaSoLuong(int maDonHang, int maVatTu, int soLuong)
        {
            if (soLuong < 1)
            {
                TempData["Error"] = "Số lượng phải ≥ 1";
                return RedirectToAction(nameof(Details), new { id = maDonHang });
            }

            var ct = await _context.ChiTietDonHangs
                .FirstOrDefaultAsync(c => c.MaDonHang == maDonHang && c.MaVatTu == maVatTu);

            if (ct != null)
            {
                ct.SoLuong = soLuong;
                await _context.SaveChangesAsync();
                await CapNhatTongTienDonHang(maDonHang);
                TempData["Success"] = "Cập nhật số lượng thành công";
            }

            return RedirectToAction(nameof(Details), new { id = maDonHang });
        }

        // URL: /admin/chi-tiet-don-hang/xoa-nhieu (POST)
        [HttpPost("xoa-nhieu")]
        public async Task<IActionResult> XoaNhieu(int maDonHang, List<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
            {
                TempData["Error"] = "Chưa chọn vật tư nào để xóa";
            }
            else
            {
                var items = await _context.ChiTietDonHangs
                    .Where(c => c.MaDonHang == maDonHang && selectedIds.Contains(c.MaVatTu))
                    .ToListAsync();

                if (items.Any())
                {
                    _context.ChiTietDonHangs.RemoveRange(items);
                    await _context.SaveChangesAsync();
                    await CapNhatTongTienDonHang(maDonHang);
                    TempData["Success"] = $"Đã xóa {items.Count} vật tư";
                }
            }
            return RedirectToAction(nameof(Details), new { id = maDonHang });
        }

        private async Task CapNhatTongTienDonHang(int maDonHang)
        {
            var donHang = await _context.DonHang.FindAsync(maDonHang);
            if (donHang != null)
            {
                var tongTien = await _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == maDonHang)
                    .SumAsync(ct => ct.SoLuong * ct.DonGia);
                
                donHang.TongTien = tongTien;
                _context.Update(donHang);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<ChiTietDonHangViewModel> LoadChiTietDonHang(int maDonHang, string search = "")
        {
            var query = _context.VatTus.Include(v => v.LoaiVatTu).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(v =>
                    v.ID.ToString().Contains(s) ||
                    v.TenVatTu.ToLower().Contains(s) ||
                    (v.LoaiVatTu != null && v.LoaiVatTu.TenLoaiVatTu.ToLower().Contains(s)));
            }

            var dsVatTu = await query
                .Select(v => new VatTuSelectItem
                {
                    MaVatTu = v.ID,
                    MaCode = v.ID.ToString("VT000"),
                    TenVatTu = v.TenVatTu,
                    TenLoai = v.LoaiVatTu != null ? v.LoaiVatTu.TenLoaiVatTu : "Chưa phân loại",
                    SoLuongTon = v.SoLuongTon,
                    GiaBan = v.GiaBan
                })
                .ToListAsync();

            var chiTiet = await _context.ChiTietDonHangs
                .Include(c => c.VatTu)
                .Where(c => c.MaDonHang == maDonHang)
                .Select(c => new ChiTietDonHangItem
                {
                    MaVatTu = c.MaVatTu,
                    TenVatTu = c.VatTu.TenVatTu,
                    SoLuong = c.SoLuong,
                    DonGia = c.DonGia
                })
                .ToListAsync();

            return new ChiTietDonHangViewModel
            {
                MaDonHang = maDonHang,
                DanhSachVatTu = dsVatTu,
                ChiTietDonHang = chiTiet,
                SearchTerm = search
            };
        }
    }
}