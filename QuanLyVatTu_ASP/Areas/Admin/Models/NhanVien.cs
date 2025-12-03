namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class NhanVien : BaseEntity
    {
        public string MaHienThi => $"NV{ID:000}";
        public string HoTen { get; set; } = null!;
        public DateTime NgaySinh { get; set; }
        public string CCCD { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string? Email { get; set; }
        public string TaiKhoan { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
        public string VaiTro { get; set; } = null!;
    }
}
