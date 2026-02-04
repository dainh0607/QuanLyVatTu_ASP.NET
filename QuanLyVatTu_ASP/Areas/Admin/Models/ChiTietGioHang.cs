using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    [Table("ChiTietGioHang")]
    public class ChiTietGioHang
    {
        [Key]
        public int ID { get; set; }

        public int MaGioHang { get; set; }

        [ForeignKey("MaGioHang")]
        public virtual GioHang? GioHang { get; set; }

        public int MaVatTu { get; set; }

        [ForeignKey("MaVatTu")]
        public virtual VatTu? VatTu { get; set; }

        public int SoLuong { get; set; }
    }
}
