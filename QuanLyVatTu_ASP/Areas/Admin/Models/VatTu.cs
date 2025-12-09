using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    [Table("VatTu")]
    public class VatTu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MaHienThi { get; set; }

        [Required]
        public string TenVatTu { get; set; }

        public string DonViTinh { get; set; }

        public decimal GiaNhap { get; set; }

        public decimal GiaBan { get; set; }

        public int SoLuongTon { get; set; }

        public int MaLoaiVatTu { get; set; }
        public int MaNhaCungCap { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey("MaLoaiVatTu")]
        public virtual LoaiVatTu LoaiVatTu { get; set; }

        [ForeignKey("MaNhaCungCap")]
        public virtual NhaCungCap NhaCungCap { get; set; }
        public string MoTa { get; set; }

    }
}