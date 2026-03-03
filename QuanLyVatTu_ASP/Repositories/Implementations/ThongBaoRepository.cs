using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class ThongBaoRepository : GenericRepository<ThongBao>, IThongBaoRepository
    {
        public ThongBaoRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<ThongBao>> GetNotificationsAsync(int? khachHangId, int take = 20)
        {
            var query = _dbSet.AsQueryable();

            if (khachHangId.HasValue)
            {
                // Retrieve notifications specific to the user OR global notifications (KhachHangId == null)
                query = query.Where(t => t.KhachHangId == khachHangId.Value || t.KhachHangId == null);
            }
            else
            {
                // If not logged in, only fetch global notifications
                query = query.Where(t => t.KhachHangId == null);
            }

            return await query
                .OrderByDescending(t => t.NgayTao)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int? khachHangId)
        {
            var query = _dbSet.Where(t => !t.DaDoc);

            if (khachHangId.HasValue)
            {
                query = query.Where(t => t.KhachHangId == khachHangId.Value || t.KhachHangId == null);
            }
            else
            {
                query = query.Where(t => t.KhachHangId == null);
            }

            return await query.CountAsync();
        }

        public async Task MarkAllAsReadAsync(int khachHangId)
        {
            var unreadNotifications = await _dbSet
                .Where(t => (t.KhachHangId == khachHangId || t.KhachHangId == null) && !t.DaDoc)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var nt in unreadNotifications)
                {
                    nt.DaDoc = true;
                }
                _dbSet.UpdateRange(unreadNotifications);
            }
        }
    }
}
