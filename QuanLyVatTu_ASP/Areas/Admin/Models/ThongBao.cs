using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    [Table("ThongBao")]
    public class ThongBao
    {
        [Key]
        public int ID { get; set; }

        public int? KhachHangId { get; set; } // Nếu NULL thì là thông báo Global cho tất cả mọi người

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(255)]
        public required string TieuDe { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public required string NoiDung { get; set; }

        [StringLength(50)]
        public string? LoaiThongBao { get; set; } // "HeThong", "DonHang", "Voucher", "HangThanhVien"

        [StringLength(500)]
        public string? LinkDich { get; set; } // URL để click vào (VD: /Customer/Profile#vouchers)

        public bool DaDoc { get; set; } = false;

        public bool DaXoa { get; set; } = false;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey("KhachHangId")]
        public virtual KhachHang? KhachHang { get; set; }
    }
}
