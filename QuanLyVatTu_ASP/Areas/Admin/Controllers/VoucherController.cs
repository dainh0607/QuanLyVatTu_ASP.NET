using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VoucherController : Controller
    {
        private readonly AppDbContext _context;

        public VoucherController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Voucher
        public async Task<IActionResult> Index()
        {
            var vouchers = await _context.Vouchers.OrderByDescending(v => v.NgayTao).ToListAsync();
            return View(vouchers);
        }

        // GET: Admin/Voucher/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var voucher = await _context.Vouchers.FirstOrDefaultAsync(m => m.ID == id);
            if (voucher == null) return NotFound();

            // Lấy thêm thống kê từ lịch sử để show lên View
            ViewBag.LichSuDungCount = await _context.LichSuSuDungVouchers.CountAsync(x => x.MaVoucherGoc == id);
            ViewBag.NguoiLuuCount = await _context.ViVoucherKhachHangs.CountAsync(x => x.MaVoucherGoc == id);

            return View(voucher);
        }

        // GET: Admin/Voucher/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Voucher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaVoucher,LoaiGiamGia,GiaTriGiam,SoTienGiamToiDa,GiaTriDonHangToiThieu,ThoiGianBatDau,ThoiGianKetThuc,TongSoLuong,GioiHanSuDungMoiUser,TrangThaiGoc")] Voucher voucher)
        {
            // Custom Validation
            if (voucher.ThoiGianBatDau >= voucher.ThoiGianKetThuc)
            {
                ModelState.AddModelError("ThoiGianKetThuc", "Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");
            }
            if (voucher.LoaiGiamGia == "PERCENT" && voucher.GiaTriGiam > 100)
            {
                ModelState.AddModelError("GiaTriGiam", "Phần trăm giảm không được vượt quá 100%.");
            }
            // Check trùng mã
            var exists = await _context.Vouchers.AnyAsync(v => v.MaVoucher == voucher.MaVoucher);
            if(exists)
            {
                ModelState.AddModelError("MaVoucher", "Mã Voucher này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                // Cài đặt mặc định
                voucher.SoLuongDaDung = 0;
                voucher.TrangThaiGoc = "ACTIVE";
                voucher.NgayTao = DateTime.Now;

                _context.Add(voucher);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo Voucher thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }

        // GET: Admin/Voucher/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound();

            ViewBag.ProtectFinancials = voucher.SoLuongDaDung > 0;
            return View(voucher);
        }

        // POST: Admin/Voucher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,MaVoucher,LoaiGiamGia,GiaTriGiam,SoTienGiamToiDa,GiaTriDonHangToiThieu,ThoiGianBatDau,ThoiGianKetThuc,TongSoLuong,GioiHanSuDungMoiUser,TrangThaiGoc")] Voucher voucher)
        {
            if (id != voucher.ID) return NotFound();

            var dbVoucher = await _context.Vouchers.AsNoTracking().FirstOrDefaultAsync(v => v.ID == id);
            if (dbVoucher == null) return NotFound();

            // Nếu đã có người dùng, không cho sửa Loại giảm và Giá trị giảm
            bool protectFinancials = dbVoucher.SoLuongDaDung > 0;

            if (protectFinancials)
            {
                voucher.LoaiGiamGia = dbVoucher.LoaiGiamGia;
                voucher.GiaTriGiam = dbVoucher.GiaTriGiam;
            }

            if (voucher.ThoiGianBatDau >= voucher.ThoiGianKetThuc)
            {
                ModelState.AddModelError("ThoiGianKetThuc", "Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");
            }
            if (voucher.LoaiGiamGia == "PERCENT" && voucher.GiaTriGiam > 100)
            {
                ModelState.AddModelError("GiaTriGiam", "Phần trăm giảm không được vượt quá 100%.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Copy lại fields không cho phép bind ở edit form
                    voucher.SoLuongDaDung = dbVoucher.SoLuongDaDung;
                    voucher.MaNhanVienTao = dbVoucher.MaNhanVienTao;
                    voucher.NgayTao = dbVoucher.NgayTao;

                    _context.Update(voucher);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật Voucher thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.ID)) return NotFound();
                    else throw;
                }
            }
            return View(voucher);
        }

        private bool VoucherExists(int id)
        {
            return _context.Vouchers.Any(e => e.ID == id);
        }
    }
}
