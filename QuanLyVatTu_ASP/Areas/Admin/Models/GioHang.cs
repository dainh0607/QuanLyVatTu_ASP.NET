using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    [Table("GioHang")]
    public class GioHang
    {
        [Key]
        public int ID { get; set; }

        public int MaKhachHang { get; set; }

        [ForeignKey("MaKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }

        public virtual ICollection<ChiTietGioHang>? ChiTietGioHangs { get; set; }
    }
}
