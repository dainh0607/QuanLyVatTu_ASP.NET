using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Repositories.Interfaces
{
    public interface IThongBaoRepository : IGenericRepository<ThongBao>
    {
        Task<List<ThongBao>> GetNotificationsAsync(int? khachHangId, int take = 20);
        Task<int> GetUnreadCountAsync(int? khachHangId);
        Task MarkAllAsReadAsync(int khachHangId);
    }
}
