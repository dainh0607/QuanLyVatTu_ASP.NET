using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels.VatTu
{
    public class VatTuCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên vật tư")]
        [Display(Name = "Tên vật tư")]
        public string TenVatTu { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập đơn vị tính")]
        [Display(Name = "Đơn vị tính")]
        public string DonViTinh { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập giá nhập")]
        [Display(Name = "Giá nhập")]
        public decimal GiaNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Display(Name = "Giá bán")]
        public decimal GiaBan { get; set; }

        [Display(Name = "Số lượng tồn")]
        public int SoLuongTon { get; set; } = 0;

        [Required(ErrorMessage = "Vui lòng chọn loại vật tư")]
        [Display(Name = "Loại vật tư")]
        public int MaLoaiVatTu { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")]
        [Display(Name = "Nhà cung cấp")]
        public int MaNhaCungCap { get; set; }

        [Display(Name = "Mã vật tư")]
        public string MaHienThi => Id > 0 ? $"VT{Id:000}" : "Tự động tạo";
    }
}