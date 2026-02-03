namespace QuanLyVatTu_ASP.Models.ViewModels
{
    /// <summary>
    /// Model cho sản phẩm trong danh sách yêu thích
    /// </summary>
    public class WishlistItem
    {
        public int VatTuId { get; set; }
        public string? TenVatTu { get; set; }
        public decimal DonGia { get; set; }
        public string? HinhAnh { get; set; }
        public DateTime NgayThem { get; set; }
    }
}
