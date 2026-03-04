using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Repositories.Interfaces;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public HomeController(IUnitOfWork unitOfWork, IEmailService emailService, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _unitOfWork.VatTuRepository.GetAllAsync();
            var featuredProducts = products.OrderByDescending(p => p.ID).Take(8).ToList();
            return View(featuredProducts);
        }

        public async Task<IActionResult> Contact()
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId != null)
            {
                var khachHang = await _unitOfWork.KhachHangRepository.GetByIdAsync(khachHangId.Value);
                return View(khachHang);
            }
            return View();
        }

        public IActionResult Welcome()
        {
            return View();
        }

        [HttpGet("/TestDB")]
        public IActionResult TestDB([FromServices] QuanLyVatTu_ASP.DataAccess.AppDbContext db)
        {
            var users = db.KhachHangs.Select(k => new { k.ID, k.HoTen, k.MaHangThanhVien, k.NgayHetHanHang }).ToList();
            var notifs = db.ThongBaos.OrderByDescending(t => t.ID).Take(10).ToList();
            return Json(new { users, notifs });
        }

        [HttpPost]
        public async Task<IActionResult> Contact(string fullName, string email, string phone, string subject, string content)
        {
            try
            {
                // Map subject value to readable label
                var subjectLabels = new Dictionary<string, string>
                {
                    { "tu-van", "Tư vấn sản phẩm" },
                    { "khieu-nai", "Khiếu nại dịch vụ" },
                    { "bao-hanh", "Yêu cầu bảo hành" },
                    { "hop-tac", "Hợp tác kinh doanh" },
                    { "khac", "Khác" }
                };
                var subjectLabel = subjectLabels.ContainsKey(subject ?? "") ? subjectLabels[subject!] : "Khác";

                // Build professional HTML email body
                var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; }}
        .email-container {{ max-width: 600px; margin: 30px auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.08); }}
        .email-header {{ background: linear-gradient(135deg, #0d6efd, #0a58ca); color: #fff; padding: 30px; text-align: center; }}
        .email-header h1 {{ margin: 0; font-size: 22px; font-weight: 700; }}
        .email-header p {{ margin: 8px 0 0; opacity: 0.85; font-size: 14px; }}
        .email-body {{ padding: 30px; }}
        .info-row {{ display: flex; border-bottom: 1px solid #e9ecef; padding: 14px 0; }}
        .info-label {{ font-weight: 600; color: #495057; width: 140px; min-width: 140px; font-size: 14px; }}
        .info-value {{ color: #212529; font-size: 14px; }}
        .subject-badge {{ display: inline-block; background: #e8f0fe; color: #0d6efd; padding: 5px 14px; border-radius: 20px; font-size: 13px; font-weight: 600; }}
        .content-section {{ margin-top: 20px; }}
        .content-section h3 {{ color: #374151; font-size: 15px; margin-bottom: 10px; border-left: 4px solid #0d6efd; padding-left: 12px; }}
        .content-box {{ background: #f8f9fa; border-radius: 8px; padding: 18px; color: #333; font-size: 14px; line-height: 1.7; white-space: pre-wrap; }}
        .email-footer {{ background: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #e9ecef; }}
        .email-footer p {{ margin: 0; color: #6c757d; font-size: 12px; }}
        .timestamp {{ color: #6c757d; font-size: 12px; margin-top: 15px; text-align: right; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <h1>📩 Phản hồi mới từ khách hàng</h1>
            <p>Hệ thống tiếp nhận phản hồi - QLVT Store</p>
        </div>
        <div class='email-body'>
            <div class='info-row'>
                <div class='info-label'>👤 Họ và tên:</div>
                <div class='info-value'><strong>{System.Net.WebUtility.HtmlEncode(fullName)}</strong></div>
            </div>
            <div class='info-row'>
                <div class='info-label'>📞 Số điện thoại:</div>
                <div class='info-value'>{System.Net.WebUtility.HtmlEncode(phone)}</div>
            </div>
            <div class='info-row'>
                <div class='info-label'>📧 Email:</div>
                <div class='info-value'>{(string.IsNullOrEmpty(email) ? "<em>Không cung cấp</em>" : System.Net.WebUtility.HtmlEncode(email))}</div>
            </div>
            <div class='info-row'>
                <div class='info-label'>📋 Chủ đề:</div>
                <div class='info-value'><span class='subject-badge'>{subjectLabel}</span></div>
            </div>

            <div class='content-section'>
                <h3>Nội dung phản hồi</h3>
                <div class='content-box'>{System.Net.WebUtility.HtmlEncode(content)}</div>
            </div>

            <div class='timestamp'>🕐 Thời gian gửi: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</div>
        </div>
        <div class='email-footer'>
            <p>Email này được gửi tự động từ hệ thống QLVT Store. Vui lòng không trả lời trực tiếp.</p>
        </div>
    </div>
</body>
</html>";

                // Send email to store's configured email
                var recipientEmail = _config["SmtpSettings:FromEmail"] ?? _config["SmtpSettings:Username"]!;
                var emailSubject = $"[Phản hồi khách hàng] {subjectLabel} - {fullName}";

                await _emailService.SendEmailAsync(recipientEmail, emailSubject, emailBody);

                TempData["Success"] = "Cảm ơn bạn đã gửi phản hồi! Chúng tôi đã tiếp nhận và sẽ phản hồi sớm nhất có thể.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra khi gửi phản hồi. Vui lòng thử lại sau hoặc liên hệ trực tiếp qua hotline.";
            }

            return RedirectToAction("Contact");
        }
    }
}