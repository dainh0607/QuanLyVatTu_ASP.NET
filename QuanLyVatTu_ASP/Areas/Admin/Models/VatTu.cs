namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class VatTu
    {
        public int Id { get; set; }
        public string TenVatTu { get; set; }
        public decimal GiaBan { get; set; }
        public string DonViTinh { get; set; }
        public int SoLuongTon { get; set; }

        // Add the missing property 'MoTa' to fix the error  
        public string MoTa { get; set; }
    }
}
