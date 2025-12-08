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

        public DbSet<VatTu> VatTus { get; set; }
        public DbSet<LoaiVatTu> LoaiVatTus { get; set; }
        public DbSet<NhaCungCap> NhaCungCaps { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<DonHang> DonHang { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VatTu>().ToTable("VatTu");
            modelBuilder.Entity<LoaiVatTu>().ToTable("LoaiVatTu");
            modelBuilder.Entity<NhaCungCap>().ToTable("NhaCungCap");
            modelBuilder.Entity<NhanVien>().ToTable("NhanVien");
            modelBuilder.Entity<KhachHang>().ToTable("KhachHang");
            modelBuilder.Entity<DonHang>().ToTable("DonHang");
            modelBuilder.Entity<ChiTietDonHang>().ToTable("ChiTietDonHang");
            modelBuilder.Entity<HoaDon>().ToTable("HoaDon", tb => tb.HasTrigger("Trigger_TinhToanHoaDon"));
            modelBuilder.Entity<ChiTietHoaDon>().ToTable("ChiTietHoaDon");

            modelBuilder.Entity<ChiTietHoaDon>(entity =>
            {
                entity.HasOne(d => d.HoaDon)
                    .WithMany(p => p.ChiTietHoaDons)
                    .HasForeignKey(d => d.MaHoaDon)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.VatTu)
                    .WithMany()
                    .HasForeignKey(d => d.MaVatTu)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<HoaDon>(entity =>
            {
                entity.HasOne(d => d.DonHang)
                    .WithMany()
                    .HasForeignKey(d => d.MaDonHang);

                entity.HasOne(d => d.NhanVien)
                    .WithMany()
                    .HasForeignKey(d => d.MaNhanVien);

                entity.HasOne(d => d.KhachHang)
                    .WithMany()
                    .HasForeignKey(d => d.MaKhachHang);

                entity.Ignore("NgayTao");

                entity.Property(e => e.TienThueGTGT).ValueGeneratedOnAddOrUpdate();
                entity.Property(e => e.TongTienSauThue).ValueGeneratedOnAddOrUpdate();
                entity.Property(e => e.TongTienTruocThue).HasColumnType("decimal(18, 0)");
                entity.Property(e => e.SoTienDatCoc).HasColumnType("decimal(18, 0)");
            });

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.Name)
                        .Property(property.Name)
                        .HasColumnType("decimal(18, 0)");
                }
            }

            modelBuilder.Entity<VatTu>()
                .HasOne(d => d.LoaiVatTu)
                .WithMany(p => p.VatTus)
                .HasForeignKey(d => d.MaLoaiVatTu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VatTu>()
                .HasOne(d => d.NhaCungCap)
                .WithMany(p => p.VatTus)
                .HasForeignKey(d => d.MaNhaCungCap)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NhanVien>().HasIndex(u => u.TaiKhoan).IsUnique();
            modelBuilder.Entity<KhachHang>().HasIndex(u => u.TaiKhoan).IsUnique();

            modelBuilder.Entity<ChiTietDonHang>()
             .Property(p => p.ThanhTien)
             .HasComputedColumnSql();
        }
    }
}