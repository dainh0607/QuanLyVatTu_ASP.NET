namespace QuanLyVatTu_ASP.Models.ViewModels
{
    public class ChangePasswordModel
    {
        public int Id { get; set; }
        public string MatKhauCu { get; set; } = string.Empty; // Old Password
        public string MatKhauMoi { get; set; } = string.Empty; // New Password
    }
}
