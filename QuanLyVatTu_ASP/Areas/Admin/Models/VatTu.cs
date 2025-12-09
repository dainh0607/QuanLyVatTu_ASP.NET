using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class VatTu
    {
        [Key]
        public int ID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string MaHienThi { get; set; }

        [Required]
        public string TenVatTu { get; set; }

        public string DonViTinh { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal GiaNhap { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal GiaBan { get; set; }

        public int SoLuongTon { get; set; }
        public string? MoTa { get; set; }

        public DateTime? NgayTao { get; set; }

        public int MaLoaiVatTu { get; set; }

        [ForeignKey("MaLoaiVatTu")]
        public virtual LoaiVatTu? LoaiVatTu { get; set; }

        public int MaNhaCungCap { get; set; }

        [ForeignKey("MaNhaCungCap")]
        public virtual NhaCungCap? NhaCungCap { get; set; }

    }
}