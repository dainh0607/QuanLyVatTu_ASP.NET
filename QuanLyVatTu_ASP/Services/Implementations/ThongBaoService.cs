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
        public async Task BroadcastNotificationAsync(string tieuDe, string noiDung, string? linkDich, string doiTuongNhan)
        {
            var query = _unitOfWork.KhachHangRepository.GetAll();

            // Nếu không phải là "ALL" (Gửi tất cả) thì phải lọc theo ID hạng thành viên
            if (doiTuongNhan != "ALL")
            {
                if (int.TryParse(doiTuongNhan, out int hangId))
                {
                    query = query.Where(x => x.MaHangThanhVien == hangId);
                }
            }

            // Lấy danh sách khách hàng hợp lệ (Chưa xóa và đồng ý nhận thông báo Khuyến mãi)
            var targetUsers = await query
                .Where(x => x.NhanThongBaoKhuyenMai == true)
                .Select(x => x.ID)
                .ToListAsync();

            if (!targetUsers.Any()) return; // Không có ai thỏa mãn

            var notifications = new List<ThongBao>();
            var now = DateTime.Now;

            foreach (var userId in targetUsers)
            {
                notifications.Add(new ThongBao
                {
                    KhachHangId = userId,
                    TieuDe = tieuDe,
                    NoiDung = noiDung,
                    LoaiThongBao = "KhuyenMai", // Thông báo từ Admin thường là Khuyến mãi / Tin tức
                    LinkDich = linkDich,
                    DaDoc = false,
                    DaXoa = false,
                    NgayTao = now
                });
            }

            // Không có BulkInsert trong UnitOfWork hiện tại, ta AddRange thông thường
            await _unitOfWork.ThongBaoRepository.AddRangeAsync(notifications);
            await _unitOfWork.SaveAsync();
        }
    }
}
