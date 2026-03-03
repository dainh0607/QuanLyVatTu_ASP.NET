using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu_ASP.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationAndPrivacySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NhanThongBaoDonHang",
                table: "KhachHang",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NhanThongBaoHangThanhVien",
                table: "KhachHang",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NhanThongBaoKhuyenMai",
                table: "KhachHang",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrangThaiKhoa",
                table: "KhachHang",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NhanThongBaoDonHang",
                table: "KhachHang");

            migrationBuilder.DropColumn(
                name: "NhanThongBaoHangThanhVien",
                table: "KhachHang");

            migrationBuilder.DropColumn(
                name: "NhanThongBaoKhuyenMai",
                table: "KhachHang");

            migrationBuilder.DropColumn(
                name: "TrangThaiKhoa",
                table: "KhachHang");
        }
    }
}
