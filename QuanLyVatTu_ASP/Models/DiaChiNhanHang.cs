using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Models
{
    [Table("DiaChiNhanHang")]
    public class DiaChiNhanHang
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int KhachHangId { get; set; }

        [ForeignKey("KhachHangId")]
        public virtual KhachHang? KhachHang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        [Column(TypeName = "nvarchar(100)")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Column(TypeName = "varchar(15)")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [Column(TypeName = "nvarchar(255)")]
        public string DiaChi { get; set; } = string.Empty;

        public double? KinhDo { get; set; }
        public double? ViDo { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string LoaiDiaChi { get; set; } = "Nhà riêng"; // Nhà riêng / Văn phòng

        public bool MacDinh { get; set; } = false;

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
