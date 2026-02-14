using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    [Table("YeuCauBaoGia")]
    public class YeuCauBaoGia : BaseEntity
    {
        [Column("MaKhachHang")]
        public int KhachHangId { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string? MaHienThi { get; set; }



        [Column(TypeName = "datetime")]
        public DateTime? NgayHetHan { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string TrangThai { get; set; } = "Mới"; // Mới, Đã báo giá, Đã duyệt, Đã hủy

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TongTienDuKien { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string? GhiChu { get; set; }

        [ForeignKey("KhachHangId")]
        public virtual KhachHang? KhachHang { get; set; }

        public virtual ICollection<ChiTietYeuCauBaoGia> ChiTietYeuCauBaoGias { get; set; } = new List<ChiTietYeuCauBaoGia>();
    }
}
