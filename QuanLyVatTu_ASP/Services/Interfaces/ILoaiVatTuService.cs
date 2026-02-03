using QuanLyVatTu_ASP.Areas.Admin.ViewModels.Admin.LoaiVatTu;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.LoaiVatTu; // Namespace chứa ViewModel

namespace QuanLyVatTu_ASP.Services.Interfaces
{
    public interface ILoaiVatTuService
    {
        Task<LoaiVatTuIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize);

        Task<LoaiVatTuCreateEditViewModel?> GetByIdForEditAsync(int id);

        Task<string?> CreateAsync(LoaiVatTuCreateEditViewModel model);

        Task<string?> UpdateAsync(int id, LoaiVatTuCreateEditViewModel model);

        Task<string?> DeleteAsync(int id);

        Task<List<QuanLyVatTu_ASP.Areas.Admin.Models.LoaiVatTu>> GetLookupAsync();

        Task<string> GetNextMaHienThiAsync();
    }
}