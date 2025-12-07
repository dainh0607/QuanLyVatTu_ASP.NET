using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }


        public override int SaveChanges()
        {
            TinhToanHoaDon();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            TinhToanHoaDon();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void TinhToanHoaDon()
        {
            var hoaDons = ChangeTracker.Entries<HoaDon>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity);

            foreach (var hd in hoaDons)
            {
                hd.TinhThueVaTongTien();
            }
        }

        public DbSet<VatTu> VatTus { get; set; }
        public DbSet<LoaiVatTu> LoaiVatTus { get; set; }
        public DbSet<NhaCungCap> NhaCungCaps { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<DonHang> DonHang { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }
    }
}
