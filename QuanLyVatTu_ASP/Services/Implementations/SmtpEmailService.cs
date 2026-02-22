using System.Net;
using System.Net.Mail;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            await SendEmailWithEmbeddedImagesAsync(to, subject, htmlBody, new Dictionary<string, byte[]>());
        }

        public async Task SendEmailWithEmbeddedImagesAsync(string to, string subject, string htmlBody, Dictionary<string, byte[]> images)
        {
            var smtpSettings = _config.GetSection("SmtpSettings");
            var host = smtpSettings["Host"];
            var port = int.TryParse(smtpSettings["Port"], out var p) ? p : 587;
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var fromEmail = smtpSettings["FromEmail"] ?? username;
            var fromName = smtpSettings["FromName"] ?? "Cửa hàng Vật tư Xây dựng";

            // If SMTP is not configured, log and return gracefully
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("SMTP chưa được cấu hình. Email không được gửi đến {To}. Subject: {Subject}", to, subject);
                _logger.LogInformation("Nội dung email:\n{Body}", htmlBody);
                return;
            }

            try
            {
                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = subject,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                // Create HTML view and embed images
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");
                if (images != null && images.Count > 0)
                {
                    foreach (var img in images)
                    {
                        var linkedResource = new LinkedResource(new MemoryStream(img.Value), "image/jpeg") // Assuming JPEG for now, could be dynamic
                        {
                            ContentId = img.Key,
                            TransferEncoding = System.Net.Mime.TransferEncoding.Base64
                        };
                        htmlView.LinkedResources.Add(linkedResource);
                    }
                }
                message.AlternateViews.Add(htmlView);

                await client.SendMailAsync(message);
                _logger.LogInformation("Đã gửi email thành công đến {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi gửi email đến {To}: {Message}", to, ex.Message);
            }
        }
    }
}
