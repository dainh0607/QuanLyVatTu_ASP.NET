using QuanLyVatTu_ASP.Areas.Admin.ViewModels.VatTu;
using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Services.Interfaces
{
    public interface IVatTuService
    {
        Task<VatTuIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize);

        Task<VatTuCreateEditViewModel?> GetByIdForEditAsync(int id);

        Task<string?> CreateAsync(VatTuCreateEditViewModel model);

        Task<string?> UpdateAsync(int id, VatTuCreateEditViewModel model);

        Task<string?> DeleteAsync(int id);

        Task<(List<LoaiVatTu> LoaiList, List<NhaCungCap> NccList)> GetDropdownDataAsync();

        Task<string> GetNextMaHienThiAsync();

        Task<VatTu?> GetByIdAsync(int id);
    }
}