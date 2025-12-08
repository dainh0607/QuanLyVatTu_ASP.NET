namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels.ThongKe
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int PaidOrders { get; set; }

        public List<OrderStatisticItem> Orders { get; set; } = new List<OrderStatisticItem>();

        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<decimal> ChartData { get; set; } = new List<decimal>();

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
        public int? NhanVienId { get; set; }
        public int? KhachHangId { get; set; }
    }
    public class OrderStatisticItem
    {
        public int Id { get; set; }
        public string MaDH { get; set; } = string.Empty;
        public DateTime Ngay { get; set; }
        public string KhachHang { get; set; } = string.Empty;
        public string NhanVien { get; set; } = string.Empty;
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = string.Empty;

        public bool DaThanhToan => TrangThai == "Hoàn thành" || TrangThai == "Đã thanh toán";
    }
}
