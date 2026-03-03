using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class ThongBaoService : IThongBaoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ThongBaoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ThongBao>> GetUserNotificationsAsync(int? khachHangId, int take = 20)
        {
            var rawNotifications = await _unitOfWork.ThongBaoRepository.GetNotificationsAsync(khachHangId, 100);
            rawNotifications = rawNotifications.Where(n => !n.DaXoa).ToList();

            if (khachHangId.HasValue)
            {
                var kh = await _unitOfWork.KhachHangRepository.GetByIdAsync(khachHangId.Value);
                if (kh != null)
                {
                    if (!kh.NhanThongBaoDonHang) rawNotifications.RemoveAll(n => n.LoaiThongBao == "DonHang");
                    if (!kh.NhanThongBaoKhuyenMai) rawNotifications.RemoveAll(n => n.LoaiThongBao == "KhuyenMai" || n.LoaiThongBao == "Voucher");
                    if (!kh.NhanThongBaoHangThanhVien) rawNotifications.RemoveAll(n => n.LoaiThongBao == "HangThanhVien");
                }
            }

            return rawNotifications.Take(take).ToList();
        }

        public async Task<int> GetUnreadCountAsync(int? khachHangId)
        {
            var count = 0;
            // Dễ nhất là gọi GetUserNotificationsAsync và đếm
            var notifications = await GetUserNotificationsAsync(khachHangId, 100);
            return notifications.Count(n => !n.DaDoc);
        }

        public async Task MarkAsReadAsync(int notificationId, int? khachHangId)
        {
            var notification = await _unitOfWork.ThongBaoRepository.GetByIdAsync(notificationId);
            if (notification != null && (notification.KhachHangId == khachHangId || notification.KhachHangId == null))
            {
                notification.DaDoc = true;
                _unitOfWork.ThongBaoRepository.Update(notification);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int khachHangId)
        {
            await _unitOfWork.ThongBaoRepository.MarkAllAsReadAsync(khachHangId);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteNotificationAsync(int notificationId, int? khachHangId)
        {
            var notification = await _unitOfWork.ThongBaoRepository.GetByIdAsync(notificationId);
            // Ẩn hiển thị (Soft Delete)
            if (notification != null && (notification.KhachHangId == khachHangId || khachHangId == null))
            {
                notification.DaXoa = true;
                _unitOfWork.ThongBaoRepository.Update(notification);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task CreateSystemNotificationAsync(int? khachHangId, string tieuDe, string noiDung, string? linkDich = null)
        {
            var notification = new ThongBao
            {
                KhachHangId = khachHangId,
                TieuDe = tieuDe,
                NoiDung = noiDung,
                LoaiThongBao = "HeThong",
                LinkDich = linkDich,
                DaDoc = false,
                NgayTao = DateTime.Now
            };

            await _unitOfWork.ThongBaoRepository.AddAsync(notification);
            await _unitOfWork.SaveAsync();
        }

        public async Task CreateVoucherNotificationAsync(int khachHangId, string tieuDe, string noiDung, string? linkDich = null)
        {
            var notification = new ThongBao
            {
                KhachHangId = khachHangId,
                TieuDe = tieuDe,
                NoiDung = noiDung,
                LoaiThongBao = "Voucher",
                LinkDich = linkDich,
                DaDoc = false,
                DaXoa = false,
                NgayTao = DateTime.Now
            };

            await _unitOfWork.ThongBaoRepository.AddAsync(notification);
            await _unitOfWork.SaveAsync();
        }

        public async Task CreateTierNotificationAsync(int khachHangId, string tieuDe, string noiDung, string? linkDich = null)
        {
            var notification = new ThongBao
            {
                KhachHangId = khachHangId,
                TieuDe = tieuDe,
                NoiDung = noiDung,
                LoaiThongBao = "HangThanhVien",
                LinkDich = linkDich,
                DaDoc = false,
                DaXoa = false,
                NgayTao = DateTime.Now
            };

            await _unitOfWork.ThongBaoRepository.AddAsync(notification);
            await _unitOfWork.SaveAsync();
        }
        public async Task CreateOrderNotificationAsync(int khachHangId, string tieuDe, string noiDung, int donHangId)
        {
            var notification = new ThongBao
            {
                KhachHangId = khachHangId,
                TieuDe = tieuDe,
                NoiDung = noiDung,
                LoaiThongBao = "DonHang",
                LinkDich = $"/Customer/Profile#notifications",
                DaDoc = false,
                DaXoa = false,
                NgayTao = DateTime.Now
            };

            await _unitOfWork.ThongBaoRepository.AddAsync(notification);
            await _unitOfWork.SaveAsync();
        }
    }
}
