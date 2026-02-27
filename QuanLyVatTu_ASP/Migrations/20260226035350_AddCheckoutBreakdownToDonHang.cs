using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu_ASP.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutBreakdownToDonHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaVoucherId",
                table: "DonHang",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoDiemSuDung",
                table: "DonHang",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SoTienChietKhauHang",
                table: "DonHang",
                type: "decimal(18,2)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SoTienGiamDiem",
                table: "DonHang",
                type: "decimal(18,2)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SoTienGiamVoucher",
                table: "DonHang",
                type: "decimal(18,2)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TongTienThucTra",
                table: "DonHang",
                type: "decimal(18,2)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MaVoucherId",
                table: "DonHang",
                column: "MaVoucherId");

            migrationBuilder.AddForeignKey(
                name: "FK_DonHang_Voucher_MaVoucherId",
                table: "DonHang",
                column: "MaVoucherId",
                principalTable: "Voucher",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonHang_Voucher_MaVoucherId",
                table: "DonHang");

            migrationBuilder.DropIndex(
                name: "IX_DonHang_MaVoucherId",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "MaVoucherId",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "SoDiemSuDung",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "SoTienChietKhauHang",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "SoTienGiamDiem",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "SoTienGiamVoucher",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "TongTienThucTra",
                table: "DonHang");
        }
    }
}
