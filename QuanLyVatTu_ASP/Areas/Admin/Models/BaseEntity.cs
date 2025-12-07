namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class BaseEntity
    {
        public int ID { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
