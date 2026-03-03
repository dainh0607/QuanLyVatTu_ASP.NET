using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu_ASP.Migrations
{
    /// <inheritdoc />
    public partial class AddDaXoaToThongBao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "ThongBao",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "ThongBao");
        }
    }
}
