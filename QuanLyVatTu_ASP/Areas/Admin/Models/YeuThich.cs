using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    [Table("YeuThich")]
    [Index(nameof(MaKhachHang), nameof(MaVatTu), IsUnique = true)] 
    public class YeuThich
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [ForeignKey("KhachHang")]
        public int MaKhachHang { get; set; }

        [ForeignKey("VatTu")]
        public int MaVatTu { get; set; }

        public DateTime NgayThem { get; set; } = DateTime.Now;

        public virtual KhachHang? KhachHang { get; set; }
        public virtual VatTu? VatTu { get; set; }
    }
}