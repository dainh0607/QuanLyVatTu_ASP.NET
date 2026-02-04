using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class DanhGia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        // Khóa ngoại trỏ về Khách Hàng
        [ForeignKey("KhachHang")]
        public int MaKhachHang { get; set; }

        [NotMapped]
        public string? TenNguoiDanhGia { get; set; } // Giữ lại property nhưng không map vào DB để tránh lỗi Runtime cũ

        // Khóa ngoại trỏ về Vật Tư (Sản phẩm)
        [ForeignKey("VatTu")]
        public int MaVatTu { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5")]
        public int SoSao { get; set; }

        // Loại bỏ ChatLuongSanPham nếu thừa, hoặc giữ lại nhưng cho phép null/default
        // Ở đây comment lại để tập trung vào SoSao
        // [Required]
        // [Range(1, 5)]
        // public int ChatLuongSanPham { get; set; }

        public string? BinhLuan { get; set; }

        public string? PhanHoi { get; set; } // Phản hồi từ cửa hàng
        public DateTime? NgayPhanHoi { get; set; }

        public int LuotThich { get; set; } = 0;

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        public virtual KhachHang? KhachHang { get; set; }
        public virtual VatTu? VatTu { get; set; }
        public virtual ICollection<TuongTacDanhGia> TuongTacDanhGias { get; set; } = new List<TuongTacDanhGia>();
    }
}
