using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Services.Interfaces
{
    public interface IThongBaoService
    {
        Task<List<ThongBao>> GetUserNotificationsAsync(int? khachHangId, int take = 20);
        Task<int> GetUnreadCountAsync(int? khachHangId);
        Task MarkAsReadAsync(int notificationId, int? khachHangId);
        Task MarkAllAsReadAsync(int khachHangId);
        Task DeleteNotificationAsync(int notificationId, int? khachHangId);
        Task CreateSystemNotificationAsync(int? khachHangId, string tieuDe, string noiDung, string? linkDich = null);
        Task CreateVoucherNotificationAsync(int khachHangId, string tieuDe, string noiDung, string? linkDich = null);
        Task CreateTierNotificationAsync(int khachHangId, string tieuDe, string noiDung, string? linkDich = null);
        Task CreateOrderNotificationAsync(int khachHangId, string tieuDe, string noiDung, int donHangId);
        Task BroadcastNotificationAsync(string tieuDe, string noiDung, string? linkDich, string doiTuongNhan);
    }
}
