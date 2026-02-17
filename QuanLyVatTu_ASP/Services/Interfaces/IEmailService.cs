namespace QuanLyVatTu_ASP.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
        Task SendEmailWithEmbeddedImagesAsync(string to, string subject, string htmlBody, Dictionary<string, byte[]> images);
    }
}
