using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class LichSuSuDungVoucher : BaseEntity
    {
        [Required]
        [Column("MaVoucherGoc")]
        public int MaVoucherGoc { get; set; }

        [Required]
        [Column("MaDonHang")]
        public int MaDonHang { get; set; }

        [Required]
        [Column("MaKhachHang")]
        public int MaKhachHang { get; set; }

        [Required]
        [Column("TenKhachHangSnapshot", TypeName = "nvarchar(255)")]
        public string TenKhachHangSnapshot { get; set; } = null!;

        [Required]
        [Column("SoTienGiamSnapshot", TypeName = "decimal(18,2)")]
        public decimal SoTienGiamSnapshot { get; set; }

        [Required]
        [Column("ThoiGianSuDung", TypeName = "datetime")]
        public DateTime ThoiGianSuDung { get; set; } = DateTime.Now;

        [Required]
        [Column("TrangThaiSuDung", TypeName = "varchar(20)")]
        // "APPLIED", "REFUNDED", "BURNED"
        public string TrangThaiSuDung { get; set; } = "APPLIED";

        [ForeignKey("MaVoucherGoc")]
        public virtual Voucher? VoucherGoc { get; set; }

        [ForeignKey("MaDonHang")]
        public virtual DonHang? DonHang { get; set; }

        [ForeignKey("MaKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }
    }
}
