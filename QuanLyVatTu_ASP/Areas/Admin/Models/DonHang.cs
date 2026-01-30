using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class DonHang : BaseEntity
    {
        [Column(TypeName = "varchar(20)")]
        public string? MaHienThi { get; set; }

        [Column("MaKhachHang")]
        public int KhachHangId { get; set; }

        [Column("MaNhanVien")]
        public int? NhanVienId { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoTienDatCoc { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        [MaxLength(50)]
        [MaxLength(50)]
        public string? PhuongThucDatCoc { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? NgayDatCoc { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string TrangThai { get; set; } = "Chờ xác nhận";

        [Column(TypeName = "nvarchar(255)")]
        public string? GhiChu { get; set; }

        [ForeignKey("KhachHangId")]
        public KhachHang? KhachHang { get; set; }

        [ForeignKey("NhanVienId")]

        [ForeignKey("NhanVienId")]

        [ForeignKey("NhanVienId")]
        public NhanVien? NhanVien { get; set; }

        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}
