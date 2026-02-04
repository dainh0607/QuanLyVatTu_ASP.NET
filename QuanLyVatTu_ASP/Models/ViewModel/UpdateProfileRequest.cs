using Microsoft.AspNetCore.Http;

namespace QuanLyVatTu_ASP.Models.ViewModels
{
    /// <summary>
    /// Request model cho việc cập nhật hồ sơ khách hàng
    /// </summary>
    public class UpdateProfileRequest
    {
        public int Id { get; set; }
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public IFormFile? AnhDaiDienFile { get; set; }
    }
}
