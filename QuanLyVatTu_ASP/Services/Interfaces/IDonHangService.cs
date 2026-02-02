using QuanLyVatTu_ASP.Areas.Admin.ViewModels;
using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Services.Interfaces
{
    public interface IDonHangService
    {
        Task<DonHangIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize);

        Task<DonHangCreateEditViewModel?> GetByIdForEditAsync(int id);

        Task CreateAsync(DonHangCreateEditViewModel model);

        Task<bool> UpdateAsync(int id, DonHangCreateEditViewModel model);

        Task DeleteAsync(int id);

        Task<List<KhachHang>> GetKhachHangLookupAsync();
        Task<List<NhanVien>> GetNhanVienLookupAsync();
    }
}