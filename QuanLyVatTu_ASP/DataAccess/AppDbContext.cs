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
                              .UseSqlServer("Server=MSI\\SQLEXPRESS;Database=QuanLyVatTu;Trusted_Connection=True;TrustServerCertificate=True;");
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
        // HoaDonVAT đã được gộp vào HoaDon
        public DbSet<YeuCauBaoGia> YeuCauBaoGia { get; set; }
        public DbSet<ChiTietYeuCauBaoGia> ChiTietYeuCauBaoGia { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<ViVoucherKhachHang> ViVoucherKhachHangs { get; set; }
        public DbSet<LichSuSuDungVoucher> LichSuSuDungVouchers { get; set; }
        public DbSet<HangThanhVien> HangThanhViens { get; set; }
        public DbSet<LichSuTichDiem> LichSuTichDiems { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }

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
                entity.HasOne(d => d.VoucherApDung).WithMany().HasForeignKey(d => d.MaVoucherId).OnDelete(DeleteBehavior.SetNull);

                // Checkout breakdown defaults
                entity.Property(e => e.SoTienChietKhauHang).HasDefaultValue(0m);
                entity.Property(e => e.SoTienGiamVoucher).HasDefaultValue(0m);
                entity.Property(e => e.SoDiemSuDung).HasDefaultValue(0);
                entity.Property(e => e.SoTienGiamDiem).HasDefaultValue(0m);
                entity.Property(e => e.TongTienThucTra).HasDefaultValue(0m);
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

            // --- 8. HoaDon (merged with HoaDonVAT) ---
            modelBuilder.Entity<HoaDon>(entity =>
            {
                entity.ToTable("HoaDon");
                
                entity.Property(e => e.NgayLap).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ChietKhau).HasDefaultValue(0m);
                entity.Property(e => e.SoTienDatCoc).HasDefaultValue(0m);
                entity.Property(e => e.TyLeThueGTGT).HasDefaultValue(10m);
                entity.Property(e => e.TrangThai).HasDefaultValue("Đã thanh toán");

                // VAT fields defaults
                entity.Property(e => e.IsVATInvoice).HasDefaultValue(false);
                entity.Property(e => e.ThueSuat).HasDefaultValue(10m);

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

            // --- 13. HoaDonVAT đã được gộp vào HoaDon (xem phần 8) ---

            // --- 14. YeuCauBaoGia ---
            modelBuilder.Entity<YeuCauBaoGia>(entity =>
            {
                entity.ToTable("YeuCauBaoGia");
                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.KhachHang)
                    .WithMany() // No collection in KhachHang for simplicity for now
                    .HasForeignKey(d => d.KhachHangId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ChiTietYeuCauBaoGia>(entity =>
            {
                entity.ToTable("ChiTietYeuCauBaoGia");

                entity.HasOne(d => d.YeuCauBaoGia)
                    .WithMany(p => p.ChiTietYeuCauBaoGias)
                    .HasForeignKey(d => d.YeuCauBaoGiaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.VatTu)
                    .WithMany()
                    .HasForeignKey(d => d.VatTuId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- 15. Voucher ---
            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.ToTable("Voucher");

                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");
                
                // Unique index: mỗi MaVoucher là duy nhất trên toàn hệ thống
                entity.HasIndex(e => e.MaVoucher).IsUnique();

                // CHECK CONSTRAINTS - Ràng buộc nghiệp vụ chặt chẽ
                entity.ToTable(t => t.HasCheckConstraint("CK_Voucher_TongSoLuong", "[TongSoLuong] >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Voucher_SoLuongDaDung", "[SoLuongDaDung] >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Voucher_SoLuong_HopLe", "[SoLuongDaDung] <= [TongSoLuong]"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Voucher_LoaiGiamGia", "[LoaiGiamGia] IN ('PERCENT', 'FIXED')"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Voucher_TrangThaiGoc", "[TrangThaiGoc] IN ('ACTIVE', 'EXPIRED', 'REVOKED')"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Voucher_GiaTriGiam", "([LoaiGiamGia] = 'FIXED' AND [GiaTriGiam] > 0) OR ([LoaiGiamGia] = 'PERCENT' AND [GiaTriGiam] > 0 AND [GiaTriGiam] <= 100)"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Voucher_ThoiGian_HopLe", "[ThoiGianBatDau] < [ThoiGianKetThuc]"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Voucher_GioiHanSuDung", "[GioiHanSuDungMoiUser] >= 1"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Voucher_GiaTriDonHangToiThieu", "[GiaTriDonHangToiThieu] >= 0"));

                // FK - Xóa NhanVien thì set null (không mất dữ liệu voucher)
                entity.HasOne(d => d.NhanVienTao)
                    .WithMany()
                    .HasForeignKey(d => d.MaNhanVienTao)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // --- 16. ViVoucherKhachHang (Ví Voucher khách hàng) ---
            modelBuilder.Entity<ViVoucherKhachHang>(entity =>
            {
                entity.ToTable("ViVoucherKhachHang");

                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ThoiGianLuuMa).HasDefaultValueSql("GETDATE()");

                // UNIQUE INDEX: Mỗi khách hàng chỉ được lưu 1 mã duy nhất 1 lần
                // -> Đảm bảo Logic Thu thập: check cặp user_id + voucher_id
                entity.HasIndex(e => new { e.MaKhachHang, e.MaVoucherGoc })
                    .IsUnique()
                    .HasDatabaseName("IX_ViVoucher_KhachHang_Voucher_Unique");

                // CHECK: Trạng thái chỉ được là 1 trong 3 giá trị
                entity.ToTable(t => t.HasCheckConstraint("CK_ViVoucherKhachHang_TrangThai", "[TrangThaiTrongVi] IN ('AVAILABLE', 'USED', 'EXPIRED')"));

                // FK -> KhachHang: Xóa khách hàng -> xóa luôn ví voucher
                entity.HasOne(d => d.KhachHang)
                    .WithMany(p => p.ViVoucherKhachHangs)
                    .HasForeignKey(d => d.MaKhachHang)
                    .OnDelete(DeleteBehavior.Cascade);

                // FK -> Voucher: Xóa voucher gốc -> xóa luôn trong ví
                entity.HasOne(d => d.VoucherGoc)
                    .WithMany(p => p.ViVoucherKhachHangs)
                    .HasForeignKey(d => d.MaVoucherGoc)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- 17. LichSuSuDungVoucher (Lịch sử sử dụng & Snapshot) ---
            modelBuilder.Entity<LichSuSuDungVoucher>(entity =>
            {
                entity.ToTable("LichSuSuDungVoucher");

                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ThoiGianSuDung).HasDefaultValueSql("GETDATE()");

                // INDEX: Tra cứu nhanh lượt dùng của 1 khách hàng với 1 voucher cụ thể
                // -> Phục vụ kiểm tra usage_limit_per_user khi checkout
                entity.HasIndex(e => new { e.MaKhachHang, e.MaVoucherGoc })
                    .HasDatabaseName("IX_LichSuSuDung_KhachHang_Voucher");

                // INDEX: Tra cứu nhanh theo đơn hàng (phục vụ logic hủy đơn & hoàn mã)
                entity.HasIndex(e => e.MaDonHang)
                    .HasDatabaseName("IX_LichSuSuDung_DonHang");

                // INDEX: Một đơn hàng chỉ áp dụng 1 voucher
                entity.HasIndex(e => new { e.MaDonHang, e.MaVoucherGoc })
                    .IsUnique()
                    .HasDatabaseName("IX_LichSuSuDung_DonHang_Voucher_Unique");

                // CHECK CONSTRAINTS
                entity.ToTable(t => t.HasCheckConstraint("CK_LichSuSuDungVoucher_TrangThai", "[TrangThaiSuDung] IN ('APPLIED', 'REFUNDED', 'BURNED')"));
                entity.ToTable(t => t.HasCheckConstraint("CK_LichSuSuDungVoucher_SoTienGiam", "[SoTienGiamSnapshot] >= 0"));

                // FK -> Voucher: RESTRICT - không cho xóa voucher gốc nếu đã có lịch sử
                entity.HasOne(d => d.VoucherGoc)
                    .WithMany(p => p.LichSuSuDungVouchers)
                    .HasForeignKey(d => d.MaVoucherGoc)
                    .OnDelete(DeleteBehavior.Restrict);

                // FK -> DonHang: RESTRICT - không cho xóa đơn hàng nếu đã có lịch sử voucher
                entity.HasOne(d => d.DonHang)
                    .WithMany(p => p.LichSuSuDungVouchers)
                    .HasForeignKey(d => d.MaDonHang)
                    .OnDelete(DeleteBehavior.Restrict);

                // FK -> KhachHang: RESTRICT - không cho xóa khách nếu đã có lịch sử
                entity.HasOne(d => d.KhachHang)
                    .WithMany(p => p.LichSuSuDungVouchers)
                    .HasForeignKey(d => d.MaKhachHang)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- 18. HangThanhVien (Tiers) ---
            modelBuilder.Entity<HangThanhVien>(entity =>
            {
                entity.ToTable("HangThanhVien");
                
                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");

                // Unique Name
                entity.HasIndex(e => e.TenHang).IsUnique();

                // Check Constraints
                entity.ToTable(t => t.HasCheckConstraint("CK_HangThanhVien_ChiTieuToiThieu", "[ChiTieuToiThieu] >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_HangThanhVien_PhanTramChietKhau", "[PhanTramChietKhau] >= 0 AND [PhanTramChietKhau] <= 100"));

                // FK -> KhachHang: KhachHang.MaHangThanhVien SetNull khi xóa hạng
                entity.HasMany(p => p.KhachHangs)
                    .WithOne(d => d.HangThanhVien)
                    .HasForeignKey(d => d.MaHangThanhVien)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // --- 19. LichSuTichDiem (Point_History) ---
            modelBuilder.Entity<LichSuTichDiem>(entity =>
            {
                entity.ToTable("LichSuTichDiem");

                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");

                // Idempotency: Không cho phép 1 Đơn Hàng sinh ra 2 giao dịch EARN (duplicate cộng điểm)
                // Tuy nhiên, có thể sinh 1 EARN và 1 REDEEM. Vì vậy Unique Key là bộ đôi Mã Đơn Hàng + Loại Giao Dịch
                entity.HasIndex(e => new { e.MaDonHang, e.LoaiGiaoDich })
                    .IsUnique()
                    .HasDatabaseName("IX_LichSuTichDiem_DonHang_LoaiGiaoDich_Unique");

                // Check Constraints
                entity.ToTable(t => t.HasCheckConstraint("CK_LichSuTichDiem_LoaiGiaoDich", "[LoaiGiaoDich] IN ('EARN', 'REDEEM', 'REFUND', 'CLAWBACK')"));
                // Điểm EARN/REFUND là số dương, REDEEM/CLAWBACK là số dương nhưng khi tính toán thực tế quy luật là trừ đi. Ở mức DB có thể linh hoạt (để số nguyên chấp nhận âm dương tùy model bussiness), nhưng Log LoaiGiaoDich đã rõ ràng.

                // FK -> KhachHang: Cascade xóa
                entity.HasOne(d => d.KhachHang)
                    .WithMany(p => p.LichSuTichDiems)
                    .HasForeignKey(d => d.MaKhachHang)
                    .OnDelete(DeleteBehavior.Cascade);

                // FK -> DonHang: Restrict xóa
                entity.HasOne(d => d.DonHang)
                    .WithMany(p => p.LichSuTichDiems)
                    .HasForeignKey(d => d.MaDonHang)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
