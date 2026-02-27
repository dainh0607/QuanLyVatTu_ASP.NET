using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Repositories.Interfaces
{
    public interface IHangThanhVienRepository : IGenericRepository<HangThanhVien>
    {
        /// <summary>
        /// Lấy tất cả hạng, sắp xếp theo mức chi tiêu tối thiểu tăng dần
        /// </summary>
        Task<IEnumerable<HangThanhVien>> GetAllOrderedAsync();

        /// <summary>
        /// Xác định hạng phù hợp dựa trên tổng chi tiêu
        /// </summary>
        Task<HangThanhVien?> GetTierForSpentAsync(decimal totalSpent);
    }
}
