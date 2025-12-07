namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class NhaCungCap : BaseEntity
    {
        public string MaHienThi => $"NCC{ID:000}";
        public string TenNhaCungCap { get; set; } = null!;
        public string? DiaChi { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }

        public ICollection<VatTu> VatTus { get; set; } = new List<VatTu>();
    }
}
