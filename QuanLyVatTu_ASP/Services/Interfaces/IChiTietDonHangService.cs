using QuanLyVatTu_ASP.Areas.Admin.ViewModels;

namespace QuanLyVatTu_ASP.Services.Interfaces
{
    public interface IChiTietDonHangService
    {
        Task<ChiTietDonHangViewModel?> GetDetailViewModelAsync(int maDonHang, string search);

        Task<string?> AddVatTuAsync(int maDonHang, int maVatTu, int soLuong);

        Task<string?> UpdateSoLuongAsync(int maDonHang, int maVatTu, int soLuong);

        Task<string?> RemoveVatTuAsync(int maDonHang, List<int> selectedIds);
    }
}