using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class ViVoucherKhachHang : BaseEntity
    {
        [Required]
        [Column("MaKhachHang")]
        public int MaKhachHang { get; set; }

        [Required]
        [Column("MaVoucherGoc")]
        public int MaVoucherGoc { get; set; }

        [Required]
        [Column("ThoiGianLuuMa", TypeName = "datetime")]
        public DateTime ThoiGianLuuMa { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [Column("TrangThaiTrongVi", TypeName = "varchar(20)")]
        // "AVAILABLE", "USED", "EXPIRED"
        public string TrangThaiTrongVi { get; set; } = "AVAILABLE";

        [ForeignKey("MaKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("MaVoucherGoc")]
        public virtual Voucher? VoucherGoc { get; set; }
    }
}
