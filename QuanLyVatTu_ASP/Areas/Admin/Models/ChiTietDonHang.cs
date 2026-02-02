using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class ChiTietDonHang
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int MaDonHang { get; set; }

        [Required]
        public int MaVatTu { get; set; }

        [Required]
        public int SoLuong { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoTienDatCoc { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGia { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ThanhTien { get; set; }

        [ForeignKey("MaDonHang")]
        public DonHang DonHang { get; set; } = null!;

        [ForeignKey("MaVatTu")]
        public VatTu VatTu { get; set; } = null!;
    }
}
