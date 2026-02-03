using QuanLyVatTu_ASP.Areas.Admin.ViewModels.NhanVien;
using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Services.Interfaces
{
    public interface INhanVienService
    {
        Task<NhanVienIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize);

        Task<NhanVienCreateEditViewModel?> GetByIdForEditAsync(int id);

        Task<string?> CreateAsync(NhanVienCreateEditViewModel model);

        Task<string?> UpdateAsync(int id, NhanVienCreateEditViewModel model);

        Task<string?> DeleteAsync(int id);

        Task<UserProfileViewModel?> GetByEmailAsync(string email);
    }
}
