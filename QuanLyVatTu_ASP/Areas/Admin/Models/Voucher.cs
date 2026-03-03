using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class Voucher : BaseEntity
    {
        [Required(ErrorMessage = "Mã voucher không được để trống")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Mã voucher phải từ 3 đến 50 ký tự")]
        [Column("MaVoucher", TypeName = "varchar(50)")]
        public string MaVoucher { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn loại giảm giá")]
        [Column("LoaiGiamGia", TypeName = "varchar(20)")]
        // "PERCENT" hoặc "FIXED"
        public string LoaiGiamGia { get; set; } = "PERCENT";

        [Required(ErrorMessage = "Giá trị giảm không được để trống")]
        [Column("GiaTriGiam", TypeName = "decimal(18,2)")]
        public decimal GiaTriGiam { get; set; }

        [Column("SoTienGiamToiDa", TypeName = "decimal(18,2)")]
        public decimal? SoTienGiamToiDa { get; set; }

        [Required(ErrorMessage = "Giá trị đơn hàng tối thiểu không được để trống")]
        [Column("GiaTriDonHangToiThieu", TypeName = "decimal(18,2)")]
        public decimal GiaTriDonHangToiThieu { get; set; } = 0;

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        [Column("ThoiGianBatDau", TypeName = "datetime")]
        public DateTime ThoiGianBatDau { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        [Column("ThoiGianKetThuc", TypeName = "datetime")]
        public DateTime ThoiGianKetThuc { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "Tổng số lượng không được để trống")]
        [Column("TongSoLuong", TypeName = "int")]
        public int TongSoLuong { get; set; } = 100;

        [Column("SoLuongDaDung", TypeName = "int")]
        public int SoLuongDaDung { get; set; } = 0;

        [Required(ErrorMessage = "Giới hạn sử dụng trên mỗi khách hàng không được để trống")]
        [Column("GioiHanSuDungMoiUser", TypeName = "int")]
        public int GioiHanSuDungMoiUser { get; set; } = 1;

        [Column("MaNhanVienTao")]
        public int? MaNhanVienTao { get; set; }

        [Required(ErrorMessage = "Trạng thái gốc không được để trống")]
        [Column("TrangThaiGoc", TypeName = "varchar(20)")]
        // "ACTIVE", "EXPIRED", "REVOKED"
        public string TrangThaiGoc { get; set; } = "ACTIVE";

        [ForeignKey("MaNhanVienTao")]
        public virtual NhanVien? NhanVienTao { get; set; }

        public virtual ICollection<ViVoucherKhachHang> ViVoucherKhachHangs { get; set; } = new List<ViVoucherKhachHang>();
        public virtual ICollection<LichSuSuDungVoucher> LichSuSuDungVouchers { get; set; } = new List<LichSuSuDungVoucher>();
    }
}
