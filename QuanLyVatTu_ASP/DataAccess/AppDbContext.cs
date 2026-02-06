using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Models;

namespace QuanLyVatTu_ASP.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseLazyLoadingProxies()
                              .UseSqlServer("Server=NGUYEN-HOANG-DA\\NHD;Database=QuanLyVatTu;Trusted_Connection=True;TrustServerCertificate=True;");
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
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<TuongTacDanhGia> TuongTacDanhGias { get; set; }
        public DbSet<YeuThich> YeuThichs { get; set; }
        public DbSet<DiaChiNhanHang> DiaChiNhanHangs { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1. LoaiVatTu ---
            modelBuilder.Entity<LoaiVatTu>(entity =>
            {
                entity.ToTable("LoaiVatTu");
                entity.Property(e => e.MaHienThi)
                      .HasComputedColumnSql("'LVT' + RIGHT('000' + CAST([ID] AS VARCHAR(3)), 3)");
                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");
            });

            // --- 2. VatTu ---
            modelBuilder.Entity<VatTu>(entity =>
            {
                entity.ToTable("VatTu");
                entity.Property(e => e.MaHienThi)
                      .HasComputedColumnSql("'VT' + RIGHT('000' + CAST([ID] AS VARCHAR(3)), 3)");
                entity.Property(e => e.SoLuongTon).HasDefaultValue(0);
                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");
                
                // Checks
                entity.ToTable(t => t.HasCheckConstraint("CK_VatTu_GiaNhap", "[GiaNhap] >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_VatTu_GiaBan", "[GiaBan] >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_VatTu_SoLuongTon", "[SoLuongTon] >= 0"));

                entity.HasOne(d => d.LoaiVatTu)
                    .WithMany(p => p.VatTus)
                    .HasForeignKey(d => d.MaLoaiVatTu)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.NhaCungCap)
                    .WithMany(p => p.VatTus)
                    .HasForeignKey(d => d.MaNhaCungCap)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- 3. NhaCungCap ---
            modelBuilder.Entity<NhaCungCap>(entity =>
            {
                entity.ToTable("NhaCungCap");
                entity.Property(e => e.MaHienThi)
                      .HasComputedColumnSql("'NCC' + RIGHT('000' + CAST([ID] AS VARCHAR(3)), 3)");
                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");
            });

            // --- 4. NhanVien ---
            modelBuilder.Entity<NhanVien>(entity =>
            {
                entity.ToTable("NhanVien");
                entity.Property(e => e.MaHienThi)
                      .HasComputedColumnSql("'NV' + RIGHT('000' + CAST([ID] AS VARCHAR(3)), 3)");
                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.TaiKhoan).IsUnique();
                entity.HasIndex(e => new { e.NgaySinh, e.CCCD, e.SoDienThoai }).IsUnique();
            });

            // --- 5. KhachHang ---
            modelBuilder.Entity<KhachHang>(entity =>
            {
                entity.ToTable("KhachHang");
                entity.Property(e => e.MaHienThi)
                      .HasComputedColumnSql("'KH' + RIGHT('000' + CAST([ID] AS VARCHAR(3)), 3)");
                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.DangNhapGoogle).HasDefaultValue(false);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // --- 6. DonHang ---
            modelBuilder.Entity<DonHang>(entity =>
            {
                entity.ToTable("DonHang");
                // Formula: 'DH' + YEAR(NgayDat) (4 digits) + '-' + ID (3 digits)
                // Note: YEAR(NgayDat) might need conversion.
                entity.Property(e => e.MaHienThi)
                      .HasComputedColumnSql("'DH' + CONVERT(VARCHAR(4), YEAR([NgayDat])) + '-' + RIGHT('000' + CAST([ID] AS VARCHAR(3)), 3)");
                
                entity.Property(e => e.NgayDat).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.TongTien).HasDefaultValue(0m);
                entity.Property(e => e.TrangThai).HasDefaultValue("Chờ xác nhận");

                entity.HasOne(d => d.KhachHang).WithMany().HasForeignKey(d => d.KhachHangId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.NhanVien).WithMany().HasForeignKey(d => d.NhanVienId).OnDelete(DeleteBehavior.Restrict);
            });

            // --- 7. ChiTietDonHang ---
            modelBuilder.Entity<ChiTietDonHang>(entity =>
            {
                entity.ToTable("ChiTietDonHang");
                entity.Property(e => e.ThanhTien)
                      .HasComputedColumnSql("[SoLuong] * [DonGia]");
                
                entity.HasOne(d => d.DonHang)
                    .WithMany(p => p.ChiTietDonHangs)
                    .HasForeignKey(d => d.MaDonHang);

                entity.HasOne(d => d.VatTu).WithMany().HasForeignKey(d => d.MaVatTu).OnDelete(DeleteBehavior.Restrict);
            });

            // --- 8. HoaDon ---
            modelBuilder.Entity<HoaDon>(entity =>
            {
                entity.ToTable("HoaDon");
                // HoaDon has no MaHienThi in requirements, only ID.
                
                entity.Property(e => e.NgayLap).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ChietKhau).HasDefaultValue(0m);
                entity.Property(e => e.SoTienDatCoc).HasDefaultValue(0m);
                entity.Property(e => e.TyLeThueGTGT).HasDefaultValue(10m);
                entity.Property(e => e.TrangThai).HasDefaultValue("Đã thanh toán");

                // Checks
                entity.ToTable(t => t.HasCheckConstraint("CK_HoaDon_TongTienTruocThue", "[TongTienTruocThue] > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_HoaDon_ChietKhau", "[ChietKhau] >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_HoaDon_SoTienDatCoc", "[SoTienDatCoc] >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_HoaDon_TyLeThueGTGT", "[TyLeThueGTGT] IN (0, 10)"));

                // Relationships
                entity.HasOne(d => d.DonHang).WithMany().HasForeignKey(d => d.MaDonHang).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.NhanVien).WithMany().HasForeignKey(d => d.MaNhanVien).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.KhachHang).WithMany().HasForeignKey(d => d.MaKhachHang).OnDelete(DeleteBehavior.Restrict);
                
                entity.Ignore("NgayTao"); // If it exists in BaseEntity but not in table schema request
            });

            modelBuilder.Entity<ChiTietDonHang>()
                .Property(p => p.ThanhTien)
                .HasComputedColumnSql("[SoLuong] * [DonGia]", stored: true);

            // --- 9. ChiTietHoaDon ---
            modelBuilder.Entity<ChiTietHoaDon>(entity => 
            {
                entity.ToTable("ChiTietHoaDon");
                entity.Property(p => p.ThanhTien).HasComputedColumnSql("[SoLuong] * [DonGia]", stored: true);
                entity.HasOne(d => d.VatTu).WithMany().HasForeignKey(d => d.MaVatTu).OnDelete(DeleteBehavior.Restrict);
            });

            // --- 10. DanhGia ---
            modelBuilder.Entity<DanhGia>(entity =>
            {
                entity.ToTable("DanhGia");
                entity.Ignore(e => e.TenNguoiDanhGia); // Explicitly ignore column
                entity.Property(e => e.NgayDanhGia).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.LuotThich).HasDefaultValue(0);

                entity.HasOne(d => d.KhachHang)
                    .WithMany(p => p.DanhGias)
                    .HasForeignKey(d => d.MaKhachHang)
                    .OnDelete(DeleteBehavior.Restrict); 

                entity.HasOne(d => d.VatTu)
                    .WithMany(p => p.DanhGias)
                    .HasForeignKey(d => d.MaVatTu)
                    .OnDelete(DeleteBehavior.Cascade); // Xóa SP thì xóa luôn đánh giá
            });

            // --- 11. TuongTacDanhGia ---
            modelBuilder.Entity<TuongTacDanhGia>(entity =>
            {
                entity.ToTable("TuongTacDanhGia");
                entity.Property(e => e.NgayTuongTac).HasDefaultValueSql("GETDATE()");

                // Composite key for uniqueness if needed, or just unique index
                // Mỗi người chỉ được tương tác 1 lần với 1 đánh giá (Like/Dislike switch)
                entity.HasIndex(e => new { e.MaDanhGia, e.MaKhachHang }).IsUnique();

                entity.HasOne(d => d.DanhGia)
                    .WithMany(p => p.TuongTacDanhGias)
                    .HasForeignKey(d => d.MaDanhGia)
                    .OnDelete(DeleteBehavior.Cascade); // Xóa đánh giá thì xóa tương tác

                entity.HasOne(d => d.KhachHang)
                    .WithMany(p => p.TuongTacDanhGias)
                    .HasForeignKey(d => d.MaKhachHang)
                    .OnDelete(DeleteBehavior.Restrict); 
            });

            // --- 12. YeuThich ---
            modelBuilder.Entity<YeuThich>(entity =>
            {
                entity.ToTable("YeuThich");
                entity.Property(e => e.NgayThem).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.KhachHang)
                    .WithMany(p => p.YeuThichs)
                    .HasForeignKey(d => d.MaKhachHang)
                    .OnDelete(DeleteBehavior.Cascade); // Xóa User -> xóa luôn danh sách yêu thích

                entity.HasOne(d => d.VatTu)
                    .WithMany(p => p.YeuThichs)
                    .HasForeignKey(d => d.MaVatTu)
                    .OnDelete(DeleteBehavior.Cascade); // Xóa SP -> xóa khỏi danh sách yêu thích
            });
        }
    }
}
