using QuanLyVatTu_ASP.Areas.Admin.ViewModels.KhachHangViewModels;

namespace QuanLyVatTu_ASP.Services.Interfaces
{
    public interface IKhachHangService
    {
        Task<KhachHangIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize);

        Task<KhachHangCreateEditViewModel?> GetByIdForEditAsync(int id);

        Task<string?> CreateAsync(KhachHangCreateEditViewModel model);

        Task<string?> UpdateAsync(int id, KhachHangCreateEditViewModel model);

        Task<string?> DeleteAsync(int id);
    }
}