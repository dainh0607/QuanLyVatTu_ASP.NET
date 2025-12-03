using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels.LoaiVatTu
{
    public class LoaiVatTuCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên loại vật tư")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        [Display(Name = "Tên loại vật tư")]
        public string TenLoaiVatTu { get; set; } = null!;

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Mã loại")]
        public string MaHienThi => Id > 0 ? $"LVT{Id:000}" : "Tự động tạo";
    }
}
