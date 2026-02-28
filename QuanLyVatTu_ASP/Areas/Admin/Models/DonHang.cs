using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class DonHang : BaseEntity
    {
        [Column(TypeName = "varchar(20)")]
        public string? MaHienThi { get; set; }

        [Column("MaKhachHang")]
        public int KhachHangId { get; set; }

        [Column("MaNhanVien")]
        public int? NhanVienId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TongTien { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoTienDatCoc { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        [MaxLength(50)]
        public string? PhuongThucDatCoc { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? NgayDatCoc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái đơn hàng")]
        [Column(TypeName = "nvarchar(50)")]
        public string TrangThai { get; set; } = "Chờ xác nhận";

        [Column(TypeName = "nvarchar(255)")]
        public string? GhiChu { get; set; }

        [ForeignKey("KhachHangId")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("NhanVienId")]
        public virtual NhanVien? NhanVien { get; set; }

        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
        public virtual ICollection<LichSuSuDungVoucher> LichSuSuDungVouchers { get; set; } = new List<LichSuSuDungVoucher>();
        public virtual ICollection<LichSuTichDiem> LichSuTichDiems { get; set; } = new List<LichSuTichDiem>();

        // === Các trường mới cho Checkout breakdown ===

        /// <summary>Tiền giảm từ hạng thành viên</summary>
        [Column("SoTienChietKhauHang", TypeName = "decimal(18,2)")]
        public decimal? SoTienChietKhauHang { get; set; }

        /// <summary>Tiền giảm từ voucher</summary>
        [Column("SoTienGiamVoucher", TypeName = "decimal(18,2)")]
        public decimal? SoTienGiamVoucher { get; set; }

        /// <summary>Số điểm đã dùng</summary>
        [Column("SoDiemSuDung", TypeName = "int")]
        public int? SoDiemSuDung { get; set; }

        /// <summary>Tiền giảm từ điểm (1 điểm = 1 VNĐ)</summary>
        [Column("SoTienGiamDiem", TypeName = "decimal(18,2)")]
        public decimal? SoTienGiamDiem { get; set; }

        /// <summary>Tổng tiền thực trả cuối cùng (sau tất cả giảm giá)</summary>
        [Column("TongTienThucTra", TypeName = "decimal(18,2)")]
        public decimal? TongTienThucTra { get; set; }

        /// <summary>FK đến Voucher đã áp dụng</summary>
        [Column("MaVoucherId")]
        public int? MaVoucherId { get; set; }

        [ForeignKey("MaVoucherId")]
        public virtual Voucher? VoucherApDung { get; set; }
    }
}
