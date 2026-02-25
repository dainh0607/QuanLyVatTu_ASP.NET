using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class LichSuTichDiem : BaseEntity
    {
        [Required]
        [Column("MaKhachHang")]
        public int MaKhachHang { get; set; }

        [Required]
        [Column("MaDonHang")]
        public int MaDonHang { get; set; }

        [Required(ErrorMessage = "Số điểm không được để trống")]
        [Column("SoDiem", TypeName = "int")]
        public int SoDiem { get; set; }

        [Required(ErrorMessage = "Loại giao dịch không được để trống")]
        [Column("LoaiGiaoDich", TypeName = "varchar(20)")]
        // "EARN", "REDEEM", "REFUND", "CLAWBACK"
        public string LoaiGiaoDich { get; set; } = null!;

        [ForeignKey("MaKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("MaDonHang")]
        public virtual DonHang? DonHang { get; set; }
    }
}
