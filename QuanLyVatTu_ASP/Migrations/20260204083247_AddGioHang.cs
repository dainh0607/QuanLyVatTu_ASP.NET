using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu_ASP.Migrations
{
    /// <inheritdoc />
    public partial class AddGioHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietGioHang_MaGioHang",
                table: "ChiTietGioHang",
                column: "MaGioHang");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietGioHang_MaVatTu",
                table: "ChiTietGioHang",
                column: "MaVatTu");

            migrationBuilder.CreateIndex(
                name: "IX_GioHang_MaKhachHang",
                table: "GioHang",
                column: "MaKhachHang");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietGioHang");

            migrationBuilder.DropTable(
                name: "GioHang");
        }
    }
}
