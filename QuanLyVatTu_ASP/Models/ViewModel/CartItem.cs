namespace QuanLyVatTu_ASP.Models.ViewModels
{
    /// <summary>
    /// CartItem model cho giỏ hàng - Hỗ trợ nghiệp vụ vật tư xây dựng
    /// </summary>
    public class CartItem
    {
        public int VatTuId { get; set; }
        public string TenVatTu { get; set; } = string.Empty;
        public string HinhAnh { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        
        /// <summary>
        /// Đơn vị tính (Cái, Bao, Thùng, Cây, Kg, m²...)
        /// </summary>
        public string DonViTinh { get; set; } = "Cái";
        
        /// <summary>
        /// Trọng lượng đơn vị (kg)
        /// </summary>
        public decimal TrongLuong { get; set; } = 0;
        
        /// <summary>
        /// Thể tích đơn vị (m³) - optional
        /// </summary>
        public decimal? TheTich { get; set; }
        
        /// <summary>
        /// Thành tiền = Đơn giá x Số lượng
        /// </summary>
        public decimal ThanhTien => DonGia * SoLuong;
        
        /// <summary>
        /// Tổng trọng lượng = Trọng lượng đơn vị x Số lượng
        /// </summary>
        public decimal TongTrongLuong => TrongLuong * SoLuong;
    }
}
