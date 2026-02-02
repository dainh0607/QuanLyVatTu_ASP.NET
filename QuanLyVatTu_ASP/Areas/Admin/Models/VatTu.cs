using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class VatTu : BaseEntity
    {
        [Column(TypeName = "varchar(20)")]
        public string MaHienThi { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng nhập tên vật tư")]
        [Column(TypeName = "nvarchar(100)")]
        public string TenVatTu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đơn vị tính")]
        [Column(TypeName = "nvarchar(50)")]
        public string DonViTinh { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá nhập")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? GiaNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? GiaBan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn")]
        public int? SoLuongTon { get; set; }

        [NotMapped]
        public string? MoTa
        {
            // Trả về Mô tả của Loại vật tư (nếu LoaiVatTu không null)
            get { return LoaiVatTu?.MoTa; }
        }

        [Required(ErrorMessage = "Vui lòng chọn loại vật tư")]
        public int MaLoaiVatTu { get; set; }

        [ForeignKey("MaLoaiVatTu")]
        public virtual LoaiVatTu? LoaiVatTu { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")]
        public int MaNhaCungCap { get; set; }

        [ForeignKey("MaNhaCungCap")]
        public virtual NhaCungCap? NhaCungCap { get; set; }
    }
}