using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu_ASP.Migrations
{
    /// <inheritdoc />
    public partial class AddPointAndTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiemTichLuy",
                table: "KhachHang",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaHangThanhVien",
                table: "KhachHang",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHetHanHang",
                table: "KhachHang",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayLenHang",
                table: "KhachHang",
                type: "datetime",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HangThanhVien",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenHang = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    ChiTieuToiThieu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhanTramChietKhau = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HangThanhVien", x => x.ID);
                    table.CheckConstraint("CK_HangThanhVien_ChiTieuToiThieu", "[ChiTieuToiThieu] >= 0");
                    table.CheckConstraint("CK_HangThanhVien_PhanTramChietKhau", "[PhanTramChietKhau] >= 0 AND [PhanTramChietKhau] <= 100");
                });

            migrationBuilder.CreateTable(
                name: "LichSuTichDiem",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    MaDonHang = table.Column<int>(type: "int", nullable: false),
                    SoDiem = table.Column<int>(type: "int", nullable: false),
                    LoaiGiaoDich = table.Column<string>(type: "varchar(20)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuTichDiem", x => x.ID);
                    table.CheckConstraint("CK_LichSuTichDiem_LoaiGiaoDich", "[LoaiGiaoDich] IN ('EARN', 'REDEEM', 'REFUND', 'CLAWBACK')");
                    table.ForeignKey(
                        name: "FK_LichSuTichDiem_DonHang_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "DonHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuTichDiem_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_MaHangThanhVien",
                table: "KhachHang",
                column: "MaHangThanhVien");

            migrationBuilder.CreateIndex(
                name: "IX_HangThanhVien_TenHang",
                table: "HangThanhVien",
                column: "TenHang",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichSuTichDiem_DonHang_LoaiGiaoDich_Unique",
                table: "LichSuTichDiem",
                columns: new[] { "MaDonHang", "LoaiGiaoDich" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichSuTichDiem_MaKhachHang",
                table: "LichSuTichDiem",
                column: "MaKhachHang");

            migrationBuilder.AddForeignKey(
                name: "FK_KhachHang_HangThanhVien_MaHangThanhVien",
                table: "KhachHang",
                column: "MaHangThanhVien",
                principalTable: "HangThanhVien",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KhachHang_HangThanhVien_MaHangThanhVien",
                table: "KhachHang");

            migrationBuilder.DropTable(
                name: "HangThanhVien");

            migrationBuilder.DropTable(
                name: "LichSuTichDiem");

            migrationBuilder.DropIndex(
                name: "IX_KhachHang_MaHangThanhVien",
                table: "KhachHang");

            migrationBuilder.DropColumn(
                name: "DiemTichLuy",
                table: "KhachHang");

            migrationBuilder.DropColumn(
                name: "MaHangThanhVien",
                table: "KhachHang");

            migrationBuilder.DropColumn(
                name: "NgayHetHanHang",
                table: "KhachHang");

            migrationBuilder.DropColumn(
                name: "NgayLenHang",
                table: "KhachHang");
        }
    }
}
