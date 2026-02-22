using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu_ASP.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMatKhauLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix KhachHang.MatKhau length
            migrationBuilder.AlterColumn<string>(
                name: "MatKhau",
                table: "KhachHang",
                type: "varchar(255)",
                nullable: false);
            
            // Keep NhanVien change if relevant, but prioritize KhachHang fix
            /*
            migrationBuilder.AlterColumn<string>(
                name: "MatKhau",
                table: "NhanVien",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");
            */
            
            // Include ChatLuongSanPham if it was valid new functionality, but the error about KhachHang exists makes me skipped unrelated stuff if not needed
            /*
            migrationBuilder.AddColumn<int>(
                name: "ChatLuongSanPham",
                table: "DanhGia",
                type: "int",
                nullable: false,
                defaultValue: 0);
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatLuongSanPham",
                table: "DanhGia");

            migrationBuilder.AlterColumn<string>(
                name: "MatKhau",
                table: "NhanVien",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");
        }
    }
}
