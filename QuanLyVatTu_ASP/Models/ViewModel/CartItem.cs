
// File: ViewModels/CartItem.cs
public class CartItem
{
    public int VatTuId { get; set; }
    public string TenVatTu { get; set; } = string.Empty;
    public string HinhAnh { get; set; } = string.Empty;
    public decimal DonGia { get; set; }
    public int SoLuong { get; set; }
    public decimal ThanhTien => DonGia * SoLuong;
}