using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels;

namespace QuanLyVatTu_ASP.Services.Interfaces
{
    public interface IHoaDonService
    {
        Task<HoaDonViewModel> GetOrdersForIndexAsync(string keyword, int page, int pageSize);
        Task<(string? Error, int NewInvoiceId)> CreateInvoiceFromOrderAsync(int donHangId);
        Task<HoaDonDetailViewModel?> GetInvoiceDetailAsync(int id);
    }
}