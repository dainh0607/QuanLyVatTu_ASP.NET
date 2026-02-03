using QuanLyVatTu_ASP.Areas.Admin.ViewModels.NhaCungCap;
using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Services.Interfaces
{
    public interface INhaCungCapService
    {
        Task<NhaCungCapIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize);

        Task<NhaCungCapCreateEditViewModel?> GetByIdForEditAsync(int id);

        Task<string?> CreateAsync(NhaCungCapCreateEditViewModel model);

        Task<string?> UpdateAsync(int id, NhaCungCapCreateEditViewModel model);

        Task<string?> DeleteAsync(int id);

        Task<List<NhaCungCap>> GetLookupAsync();

        Task<string> GetNextMaHienThiAsync();
    }
}