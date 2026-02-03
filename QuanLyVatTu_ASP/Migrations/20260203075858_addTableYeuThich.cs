using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu_ASP.Migrations
{
    /// <inheritdoc />
    public partial class addTableYeuThich : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HinhAnh",
                table: "VatTu",
                type: "varchar(255)",
                nullable: true);

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
                name: "YeuThich");

            migrationBuilder.DropColumn(
                name: "HinhAnh",
                table: "VatTu");
        }
    }
}
