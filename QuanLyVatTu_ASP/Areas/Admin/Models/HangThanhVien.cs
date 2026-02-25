using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class HangThanhVien : BaseEntity
    {
        [Required(ErrorMessage = "Tên hạng không được để trống")]
        [Column("TenHang", TypeName = "nvarchar(100)")]
        public string TenHang { get; set; } = null!;

        [Required(ErrorMessage = "Chi tiêu tối thiểu không được để trống")]
        [Column("ChiTieuToiThieu", TypeName = "decimal(18,2)")]
        public decimal ChiTieuToiThieu { get; set; } = 0;

        [Required(ErrorMessage = "Phần trăm chiết khấu không được để trống")]
        [Column("PhanTramChietKhau", TypeName = "decimal(5,2)")]
        public decimal PhanTramChietKhau { get; set; } = 0;

        public virtual ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();
    }
}
