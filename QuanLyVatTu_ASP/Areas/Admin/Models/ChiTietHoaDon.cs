using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class ChiTietHoaDon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [ForeignKey("HoaDon")]
        public int MaHoaDon { get; set; }

        [Required]
        [ForeignKey("VatTu")]
        public int MaVatTu { get; set; }

        [Required]
        public int SoLuong { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGia { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ThanhTien { get; set; }
        [ForeignKey("MaHoaDon")]
        public HoaDon HoaDon { get; set; } = null!;

        [ForeignKey("MaVatTu")]
        public VatTu VatTu { get; set; } = null!;
    }
}
