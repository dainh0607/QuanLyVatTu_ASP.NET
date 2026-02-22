using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu_ASP.Migrations
{
    /// <inheritdoc />
    public partial class InitialDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHienThi = table.Column<string>(type: "varchar(20)", nullable: false, computedColumnSql: "'KH' + RIGHT('000' + CAST([ID] AS VARCHAR(3)), 3)"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "varchar(10)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AnhDaiDien = table.Column<string>(type: "varchar(255)", nullable: true),
                    TaiKhoan = table.Column<string>(type: "varchar(50)", nullable: false),
                    MatKhau = table.Column<string>(type: "varchar(255)", nullable: false),
                    DangNhapGoogle = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHang", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "LoaiVatTu",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHienThi = table.Column<string>(type: "varchar(20)", nullable: false, computedColumnSql: "'LVT' + RIGHT('000' + CAST([ID] AS VARCHAR(3)), 3)"),
                    TenLoaiVatTu = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiVatTu", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "NhaCungCap",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHienThi = table.Column<string>(type: "varchar(20)", nullable: false, computedColumnSql: "'NCC' + RIGHT('000' + CAST([ID] AS VARCHAR(3)), 3)"),
                    TenNhaCungCap = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SoDienThoai = table.Column<string>(type: "varchar(10)", nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhaCungCap", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHienThi = table.Column<string>(type: "varchar(20)", nullable: false, computedColumnSql: "'NV' + RIGHT('000' + CAST([ID] AS VARCHAR(3)), 3)"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "date", nullable: false),
                    CCCD = table.Column<string>(type: "varchar(12)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "varchar(10)", nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", nullable: true),
                    AnhDaiDien = table.Column<string>(type: "varchar(255)", nullable: true),
                    TaiKhoan = table.Column<string>(type: "varchar(50)", nullable: false),
                    MatKhau = table.Column<string>(type: "varchar(255)", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanVien", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DiaChiNhanHang",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KhachHangId = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "varchar(15)", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    KinhDo = table.Column<double>(type: "float", nullable: true),
                    ViDo = table.Column<double>(type: "float", nullable: true),
                    LoaiDiaChi = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    MacDinh = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaChiNhanHang", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DiaChiNhanHang_KhachHang_KhachHangId",
                        column: x => x.KhachHangId,
                        principalTable: "KhachHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GioHang",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHang", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GioHang_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YeuCauBaoGia",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    MaHienThi = table.Column<string>(type: "varchar(20)", nullable: true),
                    NgayHetHan = table.Column<DateTime>(type: "datetime", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    TongTienDuKien = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauBaoGia", x => x.ID);
                    table.ForeignKey(
                        name: "FK_YeuCauBaoGia_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VatTu",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHienThi = table.Column<string>(type: "varchar(20)", nullable: false, computedColumnSql: "'VT' + RIGHT('000' + CAST([ID] AS VARCHAR(3)), 3)"),
                    TenVatTu = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    HinhAnh = table.Column<string>(type: "varchar(255)", nullable: true),
                    DonViTinh = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    GiaNhap = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaBan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuongTon = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaLoaiVatTu = table.Column<int>(type: "int", nullable: false),
                    MaNhaCungCap = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VatTu", x => x.ID);
                    table.CheckConstraint("CK_VatTu_GiaBan", "[GiaBan] >= 0");
                    table.CheckConstraint("CK_VatTu_GiaNhap", "[GiaNhap] >= 0");
                    table.CheckConstraint("CK_VatTu_SoLuongTon", "[SoLuongTon] >= 0");
                    table.ForeignKey(
                        name: "FK_VatTu_LoaiVatTu_MaLoaiVatTu",
                        column: x => x.MaLoaiVatTu,
                        principalTable: "LoaiVatTu",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VatTu_NhaCungCap_MaNhaCungCap",
                        column: x => x.MaNhaCungCap,
                        principalTable: "NhaCungCap",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DonHang",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHienThi = table.Column<string>(type: "varchar(20)", nullable: true, computedColumnSql: "'DH' + CONVERT(VARCHAR(4), YEAR([NgayDat])) + '-' + RIGHT('000' + CAST([ID] AS VARCHAR(3)), 3)"),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    MaNhanVien = table.Column<int>(type: "int", nullable: true),
                    NgayDat = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    SoTienDatCoc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PhuongThucDatCoc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayDatCoc = table.Column<DateTime>(type: "datetime", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", nullable: false, defaultValue: "Chờ xác nhận"),
                    GhiChu = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHang", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DonHang_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonHang_NhanVien_MaNhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanVien",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietGioHang",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGioHang = table.Column<int>(type: "int", nullable: false),
                    MaVatTu = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietGioHang", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ChiTietGioHang_GioHang_MaGioHang",
                        column: x => x.MaGioHang,
                        principalTable: "GioHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietGioHang_VatTu_MaVatTu",
                        column: x => x.MaVatTu,
                        principalTable: "VatTu",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietYeuCauBaoGia",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaYeuCau = table.Column<int>(type: "int", nullable: false),
                    MaVatTu = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGiaDuKien = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietYeuCauBaoGia", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ChiTietYeuCauBaoGia_VatTu_MaVatTu",
                        column: x => x.MaVatTu,
                        principalTable: "VatTu",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietYeuCauBaoGia_YeuCauBaoGia_MaYeuCau",
                        column: x => x.MaYeuCau,
                        principalTable: "YeuCauBaoGia",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanhGia",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    MaVatTu = table.Column<int>(type: "int", nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    ChatLuongSanPham = table.Column<int>(type: "int", nullable: false),
                    BinhLuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhanHoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayPhanHoi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LuotThich = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGia", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DanhGia_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DanhGia_VatTu_MaVatTu",
                        column: x => x.MaVatTu,
                        principalTable: "VatTu",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YeuThich",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    MaVatTu = table.Column<int>(type: "int", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuThich", x => x.ID);
                    table.ForeignKey(
                        name: "FK_YeuThich_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YeuThich_VatTu_MaVatTu",
                        column: x => x.MaVatTu,
                        principalTable: "VatTu",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHang",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonHang = table.Column<int>(type: "int", nullable: false),
                    MaVatTu = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    SoTienDatCoc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false, computedColumnSql: "[SoLuong] * [DonGia]", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHang", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_DonHang_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "DonHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_VatTu_MaVatTu",
                        column: x => x.MaVatTu,
                        principalTable: "VatTu",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonHang = table.Column<int>(type: "int", nullable: false),
                    MaNhanVien = table.Column<int>(type: "int", nullable: false),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    TongTienTruocThue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TyLeThueGTGT = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 10m),
                    TienThueGTGT = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TongTienSauThue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChietKhau = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    SoTienDatCoc = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", nullable: false, defaultValue: "Đã thanh toán")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDon", x => x.ID);
                    table.CheckConstraint("CK_HoaDon_ChietKhau", "[ChietKhau] >= 0");
                    table.CheckConstraint("CK_HoaDon_SoTienDatCoc", "[SoTienDatCoc] >= 0");
                    table.CheckConstraint("CK_HoaDon_TongTienTruocThue", "[TongTienTruocThue] > 0");
                    table.CheckConstraint("CK_HoaDon_TyLeThueGTGT", "[TyLeThueGTGT] IN (0, 10)");
                    table.ForeignKey(
                        name: "FK_HoaDon_DonHang_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "DonHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoaDon_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoaDon_NhanVien_MaNhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanVien",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoaDonVAT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoHoaDon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaDonHang = table.Column<int>(type: "int", nullable: false),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    TenCongTy = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    MaSoThue = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    DiaChiDKKD = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    EmailNhanHoaDon = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    TenNguoiBan = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    MaSoThueBan = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    DiaChiNguoiBan = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    TongTienTruocThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThueSuat = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TienThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongTienSauThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonVAT", x => x.ID);
                    table.ForeignKey(
                        name: "FK_HoaDonVAT_DonHang_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "DonHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoaDonVAT_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TuongTacDanhGia",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDanhGia = table.Column<int>(type: "int", nullable: false),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    DaThich = table.Column<bool>(type: "bit", nullable: false),
                    NgayTuongTac = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuongTacDanhGia", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TuongTacDanhGia_DanhGia_MaDanhGia",
                        column: x => x.MaDanhGia,
                        principalTable: "DanhGia",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TuongTacDanhGia_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietHoaDon",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoaDon = table.Column<int>(type: "int", nullable: false),
                    MaVatTu = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: true),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false, computedColumnSql: "[SoLuong] * [DonGia]", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietHoaDon", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDon_HoaDon_MaHoaDon",
                        column: x => x.MaHoaDon,
                        principalTable: "HoaDon",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDon_VatTu_MaVatTu",
                        column: x => x.MaVatTu,
                        principalTable: "VatTu",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHang_MaDonHang",
                table: "ChiTietDonHang",
                column: "MaDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHang_MaVatTu",
                table: "ChiTietDonHang",
                column: "MaVatTu");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietGioHang_MaGioHang",
                table: "ChiTietGioHang",
                column: "MaGioHang");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietGioHang_MaVatTu",
                table: "ChiTietGioHang",
                column: "MaVatTu");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDon_MaHoaDon",
                table: "ChiTietHoaDon",
                column: "MaHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDon_MaVatTu",
                table: "ChiTietHoaDon",
                column: "MaVatTu");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietYeuCauBaoGia_MaVatTu",
                table: "ChiTietYeuCauBaoGia",
                column: "MaVatTu");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietYeuCauBaoGia_MaYeuCau",
                table: "ChiTietYeuCauBaoGia",
                column: "MaYeuCau");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaKhachHang",
                table: "DanhGia",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaVatTu",
                table: "DanhGia",
                column: "MaVatTu");

            migrationBuilder.CreateIndex(
                name: "IX_DiaChiNhanHang_KhachHangId",
                table: "DiaChiNhanHang",
                column: "KhachHangId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MaKhachHang",
                table: "DonHang",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MaNhanVien",
                table: "DonHang",
                column: "MaNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_GioHang_MaKhachHang",
                table: "GioHang",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaDonHang",
                table: "HoaDon",
                column: "MaDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaKhachHang",
                table: "HoaDon",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaNhanVien",
                table: "HoaDon",
                column: "MaNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonVAT_MaDonHang",
                table: "HoaDonVAT",
                column: "MaDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonVAT_MaKhachHang",
                table: "HoaDonVAT",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_Email",
                table: "KhachHang",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_TaiKhoan",
                table: "KhachHang",
                column: "TaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_NgaySinh_CCCD_SoDienThoai",
                table: "NhanVien",
                columns: new[] { "NgaySinh", "CCCD", "SoDienThoai" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_TaiKhoan",
                table: "NhanVien",
                column: "TaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TuongTacDanhGia_MaDanhGia_MaKhachHang",
                table: "TuongTacDanhGia",
                columns: new[] { "MaDanhGia", "MaKhachHang" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TuongTacDanhGia_MaKhachHang",
                table: "TuongTacDanhGia",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_VatTu_MaLoaiVatTu",
                table: "VatTu",
                column: "MaLoaiVatTu");

            migrationBuilder.CreateIndex(
                name: "IX_VatTu_MaNhaCungCap",
                table: "VatTu",
                column: "MaNhaCungCap");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauBaoGia_MaKhachHang",
                table: "YeuCauBaoGia",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_YeuThich_MaKhachHang_MaVatTu",
                table: "YeuThich",
                columns: new[] { "MaKhachHang", "MaVatTu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YeuThich_MaVatTu",
                table: "YeuThich",
                column: "MaVatTu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHang");

            migrationBuilder.DropTable(
                name: "ChiTietGioHang");

            migrationBuilder.DropTable(
                name: "ChiTietHoaDon");

            migrationBuilder.DropTable(
                name: "ChiTietYeuCauBaoGia");

            migrationBuilder.DropTable(
                name: "DiaChiNhanHang");

            migrationBuilder.DropTable(
                name: "HoaDonVAT");

            migrationBuilder.DropTable(
                name: "TuongTacDanhGia");

            migrationBuilder.DropTable(
                name: "YeuThich");

            migrationBuilder.DropTable(
                name: "GioHang");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "YeuCauBaoGia");

            migrationBuilder.DropTable(
                name: "DanhGia");

            migrationBuilder.DropTable(
                name: "DonHang");

            migrationBuilder.DropTable(
                name: "VatTu");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "NhanVien");

            migrationBuilder.DropTable(
                name: "LoaiVatTu");

            migrationBuilder.DropTable(
                name: "NhaCungCap");
        }
    }
}
