using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IVatTuRepository VatTuRepository { get; }
        ILoaiVatTuRepository LoaiVatTuRepository { get; }
        INhaCungCapRepository NhaCungCapRepository { get; }
        IKhachHangRepository KhachHangRepository { get; }                       
        INhanVienRepository NhanVienRepository { get; }
        IDonHangRepository DonHangRepository { get; }
        IChiTietDonHangRepository ChiTietDonHangRepository { get; }
        IHoaDonRepository HoaDonRepository { get; }
        IChiTietHoaDonRepository ChiTietHoaDonRepository { get; }

        int Save();
    }
}
