using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class LoaiVatTu : BaseEntity
    {
        [Column(TypeName = "varchar(20)")]
        public string MaHienThi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên loại vật tư")]
        [Column(TypeName = "nvarchar(100)")]
        public string TenLoaiVatTu { get; set; } = null!;

        [Column(TypeName = "nvarchar(255)")]
        public string? MoTa { get; set; }

        public virtual ICollection<VatTu> VatTus { get; set; } = new List<VatTu>();
    }
}
