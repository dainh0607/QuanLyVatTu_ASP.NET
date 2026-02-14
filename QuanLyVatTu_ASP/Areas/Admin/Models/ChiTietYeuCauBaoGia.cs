using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyVatTu_ASP.Models;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    [Table("ChiTietYeuCauBaoGia")]
    public class ChiTietYeuCauBaoGia : BaseEntity
    {
        [Column("MaYeuCau")]
        public int YeuCauBaoGiaId { get; set; }

        [Column("MaVatTu")]
        public int VatTuId { get; set; }

        public int SoLuong { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DonGiaDuKien { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        public string? GhiChu { get; set; }

        [ForeignKey("YeuCauBaoGiaId")]
        public virtual YeuCauBaoGia? YeuCauBaoGia { get; set; }

        [ForeignKey("VatTuId")]
        public virtual VatTu? VatTu { get; set; }
    }
}
