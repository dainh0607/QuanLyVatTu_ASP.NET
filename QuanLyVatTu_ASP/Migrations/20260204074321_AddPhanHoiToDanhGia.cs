using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu_ASP.Migrations
{
    /// <inheritdoc />
    public partial class AddPhanHoiToDanhGia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayPhanHoi",
                table: "DanhGia",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhanHoi",
                table: "DanhGia",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayPhanHoi",
                table: "DanhGia");

            migrationBuilder.DropColumn(
                name: "PhanHoi",
                table: "DanhGia");
        }
    }
}
