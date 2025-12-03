using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class ChiTietHoaDon
    {
        public int ID { get; set; }

        public int MaHoaDon { get; set; }
        public int MaVatTu { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal ThanhTien { get; private set; }

        public HoaDon HoaDon { get; set; } = null!;
        public VatTu VatTu { get; set; } = null!;
    }
}
