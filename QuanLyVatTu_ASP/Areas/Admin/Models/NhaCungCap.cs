using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class NhaCungCap : BaseEntity
    {
        [Column(TypeName = "varchar(20)")]
        public string MaHienThi { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string TenNhaCungCap { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(255)")]
        public string? DiaChi { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string? SoDienThoai { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? Email { get; set; }

        public ICollection<VatTu> VatTus { get; set; } = new List<VatTu>();
    }
}
