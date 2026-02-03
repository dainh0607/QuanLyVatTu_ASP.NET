using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    [Table("TuongTacDanhGia")]
    public class TuongTacDanhGia
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("DanhGia")]
        public int MaDanhGia { get; set; }

        [ForeignKey("KhachHang")]
        public int MaKhachHang { get; set; }

        public bool DaThich { get; set; } = true; // True = Like, False = Unlike

        public DateTime NgayTuongTac { get; set; } = DateTime.Now;

        public virtual DanhGia DanhGia { get; set; }
        public virtual KhachHang KhachHang { get; set; }
    }
}