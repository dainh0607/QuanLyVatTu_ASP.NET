namespace QuanLyVatTu_ASP.Models.ViewModels
{
    public class ChangePasswordModel
    {
        public int Id { get; set; }
        public string MatKhauCu { get; set; } // Old Password
        public string MatKhauMoi { get; set; } // New Password
    }
}
