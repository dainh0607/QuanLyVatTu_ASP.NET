using QuanLyVatTu_ASP.Areas.Admin.ViewModels.ThongKe;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyVatTu_ASP.Services.Interfaces
{
    public interface IThongKeService
    {
        Task<DashboardViewModel> GetDashboardStatsAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? status,
            string? paymentMethod,
            int? nhanVienId,
            int? khachHangId);

        Task<(SelectList NhanViens, SelectList KhachHangs, List<string> TrangThais, List<string> PhuongThucs)> GetFilterDropdownsAsync(int? selectedNhanVien, int? selectedKhachHang);
    }
}