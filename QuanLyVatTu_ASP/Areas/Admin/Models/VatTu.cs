namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class VatTu : BaseEntity
    {
        public string MaHienThi => $"VT{ID:000}";
        public string TenVatTu { get; set; } = null!;
        public string DonViTinh { get; set; } = null!;
        public decimal GiaNhap { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuongTon { get; set; } = 0;

        public int MaLoaiVatTu { get; set; }
        public int MaNhaCungCap { get; set; }

        public LoaiVatTu LoaiVatTu { get; set; } = null!;
        public NhaCungCap NhaCungCap { get; set; } = null!;
    }
}
