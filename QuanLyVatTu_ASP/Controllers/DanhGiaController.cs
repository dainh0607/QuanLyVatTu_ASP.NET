using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;

namespace QuanLyVatTu_ASP.Controllers
{
    [IgnoreAntiforgeryToken]
    public class DanhGiaController : Controller
    {
        private readonly AppDbContext _context;

        public DanhGiaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DanhGia/GetReviews?maVatTu=1
        [HttpGet]
        public async Task<IActionResult> GetReviews(int maVatTu, int? soSao = null)
        {
            var query = _context.DanhGias
                .Include(d => d.KhachHang)
                .Where(d => d.MaVatTu == maVatTu)
                .AsQueryable();

            // Filter by star rating if provided
            if (soSao.HasValue && soSao >= 1 && soSao <= 5)
            {
                query = query.Where(d => d.SoSao == soSao);
            }

            var reviews = await query
                .OrderByDescending(d => d.NgayDanhGia)
                .Select(d => new
                {
                    d.ID,
                    d.SoSao,
                    d.BinhLuan,
                    d.PhanHoi,
                    d.NgayPhanHoi,
                    d.LuotThich,
                    d.NgayDanhGia,
                    d.MaKhachHang,
                    TenKhachHang = d.KhachHang!.HoTen ?? "Khách hàng",
                    AvatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(d.KhachHang!.HoTen ?? "KH")}&background=3b82f6&color=fff&size=48"
                })
                .ToListAsync();

            // Calculate statistics
            var allReviews = await _context.DanhGias
                .Where(d => d.MaVatTu == maVatTu)
                .ToListAsync();

            var stats = new
            {
                TongSo = allReviews.Count,
                DiemTrungBinh = allReviews.Any() ? Math.Round(allReviews.Average(r => r.SoSao), 1) : 0,
                Sao5 = allReviews.Count(r => r.SoSao == 5),
                Sao4 = allReviews.Count(r => r.SoSao == 4),
                Sao3 = allReviews.Count(r => r.SoSao == 3),
                Sao2 = allReviews.Count(r => r.SoSao == 2),
                Sao1 = allReviews.Count(r => r.SoSao == 1)
            };

            var currentKhachHangId = HttpContext.Session.GetInt32("KhachHangId");
            return Json(new { success = true, reviews, stats, currentKhachHangId });
        }

        // POST: DanhGia/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DanhGiaRequest request)
        {
            // Check if user is logged in
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để đánh giá" });
            }

            // Validate input
            if (request.MaVatTu <= 0)
            {
                return Json(new { success = false, message = "Sản phẩm không hợp lệ" });
            }

            if (request.SoSao < 1 || request.SoSao > 5)
            {
                return Json(new { success = false, message = "Số sao phải từ 1 đến 5" });
            }

            // Check if user already reviewed this product
            var existingReview = await _context.DanhGias
                .FirstOrDefaultAsync(d => d.MaKhachHang == khachHangId && d.MaVatTu == request.MaVatTu);

            if (existingReview != null)
            {
                return Json(new { success = false, message = "Bạn đã đánh giá sản phẩm này rồi" });
            }

            var danhGia = new DanhGia
            {
                MaKhachHang = khachHangId.Value,
                MaVatTu = request.MaVatTu,
                SoSao = request.SoSao,
                // ChatLuongSanPham = request.ChatLuongSanPham ?? request.SoSao,
                BinhLuan = request.BinhLuan?.Trim(),
                NgayDanhGia = DateTime.Now,
                LuotThich = 0
            };

            _context.DanhGias.Add(danhGia);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đánh giá thành công!" });
        }

        // POST: DanhGia/Like
        [HttpPost]
        public async Task<IActionResult> Like([FromBody] LikeRequest request)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var danhGia = await _context.DanhGias.FindAsync(request.MaDanhGia);
            if (danhGia == null)
            {
                return Json(new { success = false, message = "Đánh giá không tồn tại" });
            }

            // Check if already liked
            var existingLike = await _context.TuongTacDanhGias
                .FirstOrDefaultAsync(t => t.MaDanhGia == request.MaDanhGia && t.MaKhachHang == khachHangId);

            if (existingLike != null)
            {
                // Unlike - remove the interaction
                _context.TuongTacDanhGias.Remove(existingLike);
                danhGia.LuotThich = Math.Max(0, danhGia.LuotThich - 1);
            }
            else
            {
                // Like - add new interaction
                var tuongTac = new TuongTacDanhGia
                {
                    MaDanhGia = request.MaDanhGia,
                    MaKhachHang = khachHangId.Value,
                    DaThich = true,
                    NgayTuongTac = DateTime.Now
                };
                _context.TuongTacDanhGias.Add(tuongTac);
                danhGia.LuotThich++;
            }

            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                liked = existingLike == null,
                luotThich = danhGia.LuotThich 
            });
        }

        // Check if user already liked a review
        [HttpGet]
        public async Task<IActionResult> CheckLiked(int maDanhGia)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
            {
                return Json(new { liked = false });
            }

            var liked = await _context.TuongTacDanhGias
                .AnyAsync(t => t.MaDanhGia == maDanhGia && t.MaKhachHang == khachHangId);

            return Json(new { liked });
        }
        // POST: DanhGia/Reply
        [HttpPost]
        public async Task<IActionResult> Reply([FromBody] ReplyRequest request)
        {
            // Check if user is Admin or Employee
            var role = HttpContext.Session.GetString("Role");
            var userName = HttpContext.Session.GetString("UserName");
            
            bool isAdmin = role == "Admin" || role == "Employee" || (userName?.Contains("Admin") ?? false);
            
            if (!isAdmin)
            {
                return Json(new { success = false, message = "Bạn không có quyền trả lời đánh giá" });
            }

            var danhGia = await _context.DanhGias.FindAsync(request.MaDanhGia);
            if (danhGia == null)
            {
                return Json(new { success = false, message = "Đánh giá không tồn tại" });
            }

            danhGia.PhanHoi = request.NoiDung;
            danhGia.NgayPhanHoi = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã gửi phản hồi!", ngayPhanHoi = danhGia.NgayPhanHoi });
        }

        /// <summary>
        /// Sửa đánh giá — chỉ chủ sở hữu mới được phép
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateDanhGiaRequest request)
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập." });

            var danhGia = await _context.DanhGias.FindAsync(request.Id);
            if (danhGia == null)
                return Json(new { success = false, message = "Đánh giá không tồn tại." });

            if (danhGia.MaKhachHang != khachHangId.Value)
                return Json(new { success = false, message = "Bạn không có quyền sửa đánh giá này." });

            if (request.SoSao < 1 || request.SoSao > 5)
                return Json(new { success = false, message = "Số sao phải từ 1 đến 5." });

            danhGia.SoSao = request.SoSao;
            danhGia.BinhLuan = request.BinhLuan?.Trim();
            danhGia.NgayDanhGia = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã cập nhật đánh giá!" });
        }
    }

    // Request DTOs
    public class DanhGiaRequest
    {
        public int MaVatTu { get; set; }
        public int SoSao { get; set; }
        public int? ChatLuongSanPham { get; set; }
        public string? BinhLuan { get; set; }
    }

    public class LikeRequest
    {
        public int MaDanhGia { get; set; }
    }

    public class ReplyRequest
    {
        public int MaDanhGia { get; set; }
        public string NoiDung { get; set; } = null!;
    }

    public class UpdateDanhGiaRequest
    {
        public int Id { get; set; }
        public int SoSao { get; set; }
        public string? BinhLuan { get; set; }
    }
}
