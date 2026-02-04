namespace QuanLyVatTu_ASP.Models.ViewModels
{
    public class AddressViewModel
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public double? KinhDo { get; set; }
        public double? ViDo { get; set; }
        public string LoaiDiaChi { get; set; } = "Nhà riêng";
        public bool MacDinh { get; set; }
    }
}
