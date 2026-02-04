using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class NhaCungCap : BaseEntity
    {
        [Column(TypeName = "varchar(20)")]
        public string MaHienThi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên nhà cung cấp")]
        [Column(TypeName = "nvarchar(100)")]
        public string TenNhaCungCap { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        [Column(TypeName = "nvarchar(255)")]
        public string? DiaChi { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải có 10 số và bắt đầu bằng 0")]
        [Column(TypeName = "varchar(10)")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Column(TypeName = "varchar(100)")]
        public string? Email { get; set; }

        public virtual ICollection<VatTu> VatTus { get; set; } = new List<VatTu>();
    }
}
