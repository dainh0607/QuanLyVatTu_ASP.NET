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
        private readonly QuanLyVatTu_ASP.Services.Interfaces.IThongBaoService _thongBaoService;

        public VoucherController(AppDbContext context, QuanLyVatTu_ASP.Services.Interfaces.IThongBaoService thongBaoService)
        {
            _context = context;
            _thongBaoService = thongBaoService;
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
                
                // Phát thông báo Khuyến mãi cho TOÀN BỘ khách hàng (hoặc Khách có hạng cụ thể)
                // Hiện tại bảng KhachHang có Id. Ta có thể query ID và tạo Notification cho từng người
                // Hoặc insert 1 Record ThongBao với KhachHangId = NULL (Global).
                // Do thiết kế yêu cầu "Nhắm mục tiêu đúng Hạng" và "Teaser", ta sẽ quét KhachHang.
                
                var allCustomers = await _context.KhachHangs.Include(k => k.HangThanhVien).ToListAsync();
                foreach (var kh in allCustomers)
                {
                    bool isVIP = kh.MaHangThanhVien > 1; 
                    
                    if (isVIP)
                    {
                        var tieuDe = $"🔥 Voucher mới: Giảm {voucher.GiaTriGiam}{(voucher.LoaiGiamGia == "PERCENT" ? "%" : "đ")}!";
                        var noiDung = $"Chúng tôi vừa tung ra mã ưu đãi {voucher.MaVoucher}. Hãy nhanh tay lưu vào ví trước khi hết lượt nhé!";
                        await _thongBaoService.CreateVoucherNotificationAsync(kh.ID, tieuDe, noiDung, "/Customer/Profile#promotions");
                    }
                    else
                    {
                        var tieuDe = $"✨ Sắp đạt hạng VIP để nhận mã {voucher.MaVoucher}!";
                        var noiDung = $"Tài khoản của bạn sắp đủ điều kiện thăng hạng để mở khóa mã ưu đãi khổng lồ {voucher.MaVoucher}. Hãy tiếp tục mua sắm nhé!";
                        await _thongBaoService.CreateVoucherNotificationAsync(kh.ID, tieuDe, noiDung, "/Customer/Profile#promotions");
                    }
                }

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
