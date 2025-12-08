using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    [Table("ChiTietHoaDon")]
    public class ChiTietHoaDon
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("HoaDon")]
        public int MaHoaDon { get; set; }

        [ForeignKey("VatTu")]
        public int MaVatTu { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal ThanhTien { get; set; }
        public HoaDon HoaDon { get; set; } = null!;
        public VatTu VatTu { get; set; } = null!;
    }
}
