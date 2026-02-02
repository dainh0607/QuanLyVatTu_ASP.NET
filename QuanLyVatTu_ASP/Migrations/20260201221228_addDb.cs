using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu_ASP.Migrations
{
    /// <inheritdoc />
    public partial class addDb : Migration
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
                    DiaChi = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    TaiKhoan = table.Column<string>(type: "varchar(50)", nullable: false),
                    MatKhau = table.Column<string>(type: "varchar(50)", nullable: false),
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
                    TaiKhoan = table.Column<string>(type: "varchar(50)", nullable: false),
                    MatKhau = table.Column<string>(type: "varchar(50)", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanVien", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "VatTu",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHienThi = table.Column<string>(type: "varchar(20)", nullable: false, computedColumnSql: "'VT' + RIGHT('000' + CAST([ID] AS VARCHAR(3)), 3)"),
                    TenVatTu = table.Column<string>(type: "nvarchar(100)", nullable: false),
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
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
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
                    TongTienTruocThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TyLeThueGTGT = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 10m),
                    TienThueGTGT = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TongTienSauThue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChietKhau = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    SoTienDatCoc = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
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
                name: "ChiTietHoaDon",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoaDon = table.Column<int>(type: "int", nullable: false),
                    MaVatTu = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                name: "IX_ChiTietHoaDon_MaHoaDon",
                table: "ChiTietHoaDon",
                column: "MaHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDon_MaVatTu",
                table: "ChiTietHoaDon",
                column: "MaVatTu");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MaKhachHang",
                table: "DonHang",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MaNhanVien",
                table: "DonHang",
                column: "MaNhanVien");

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
                name: "IX_VatTu_MaLoaiVatTu",
                table: "VatTu",
                column: "MaLoaiVatTu");

            migrationBuilder.CreateIndex(
                name: "IX_VatTu_MaNhaCungCap",
                table: "VatTu",
                column: "MaNhaCungCap");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHang");

            migrationBuilder.DropTable(
                name: "ChiTietHoaDon");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "VatTu");

            migrationBuilder.DropTable(
                name: "DonHang");

            migrationBuilder.DropTable(
                name: "LoaiVatTu");

            migrationBuilder.DropTable(
                name: "NhaCungCap");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "NhanVien");
        }
    }
}
