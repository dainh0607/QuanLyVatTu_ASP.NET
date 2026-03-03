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
        IYeuThichRepository YeuThichRepository { get; }
        IGioHangRepository GioHangRepository { get; }
        IChiTietGioHangRepository ChiTietGioHangRepository { get; }
        IVoucherRepository VoucherRepository { get; }
        IViVoucherRepository ViVoucherRepository { get; }
        ILichSuSuDungVoucherRepository LichSuSuDungVoucherRepository { get; }
        ILichSuTichDiemRepository LichSuTichDiemRepository { get; }
        IHangThanhVienRepository HangThanhVienRepository { get; }
        IThongBaoRepository ThongBaoRepository { get; }

        int Save();
        Task<int> SaveAsync(); // Thêm phương thức async
    }
}
