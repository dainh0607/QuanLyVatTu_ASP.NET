using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels
{
    public class DonHangCreateEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Mã đơn hàng")]
        public string MaHienThi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        [Display(Name = "Khách hàng")]
        public int KhachHangId { get; set; }

        [Display(Name = "Nhân viên")]
        public int? NhanVienId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày đặt")]
        [Display(Name = "Ngày đặt")]
        public DateTime NgayDat { get; set; }

        [Display(Name = "Tổng tiền")]
        public decimal? TongTien { get; set; }

        [Display(Name = "Số tiền đặt cọc")]
        public decimal? SoTienDatCoc { get; set; }

        public decimal TiềnCọcTốiThiểu => (TongTien ?? 0) * 0.1M;

        [Display(Name = "Phương thức đặt cọc")]
        public string? PhuongThucDatCoc { get; set; }

        [Display(Name = "Ngày đặt cọc")]
        public DateTime? NgayDatCoc { get; set; }

        [Display(Name = "Trạng thái")]
        public string? TrangThai { get; set; }

        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }
}