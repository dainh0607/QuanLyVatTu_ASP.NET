namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class LoaiVatTu : BaseEntity
    {
        public string MaHienThi => $"LVT{ID:000}";
        public string TenLoaiVatTu { get; set; } = null!;
        public string? MoTa { get; set; }

        public ICollection<VatTu> VatTus { get; set; } = new List<VatTu>();
    }
}
