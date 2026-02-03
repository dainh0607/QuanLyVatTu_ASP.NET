using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu_ASP.Migrations
{
    /// <inheritdoc />
    public partial class addTableDanhGia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnhDaiDien",
                table: "NhanVien",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnhDaiDien",
                table: "KhachHang",
                type: "varchar(255)",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaKhachHang",
                table: "DanhGia",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaVatTu",
                table: "DanhGia",
                column: "MaVatTu");

            migrationBuilder.CreateIndex(
                name: "IX_TuongTacDanhGia_MaDanhGia_MaKhachHang",
                table: "TuongTacDanhGia",
                columns: new[] { "MaDanhGia", "MaKhachHang" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TuongTacDanhGia_MaKhachHang",
                table: "TuongTacDanhGia",
                column: "MaKhachHang");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TuongTacDanhGia");

            migrationBuilder.DropTable(
                name: "DanhGia");

            migrationBuilder.DropColumn(
                name: "AnhDaiDien",
                table: "NhanVien");

            migrationBuilder.DropColumn(
                name: "AnhDaiDien",
                table: "KhachHang");
        }
    }
}
