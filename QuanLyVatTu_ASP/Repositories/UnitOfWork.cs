using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories.Interfaces;
using QuanLyVatTu_ASP.Repositories.Implementations;

namespace QuanLyVatTu_ASP.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IVatTuRepository VatTuRepository { get; private set; }
        public ILoaiVatTuRepository LoaiVatTuRepository { get; private set; }
        public INhaCungCapRepository NhaCungCapRepository { get; private set; }
        public IKhachHangRepository KhachHangRepository { get; private set; }
        public INhanVienRepository NhanVienRepository { get; private set; }
        public IDonHangRepository DonHangRepository { get; private set; }
        public IChiTietDonHangRepository ChiTietDonHangRepository { get; private set; }
        public IHoaDonRepository HoaDonRepository { get; private set; }
        public IChiTietHoaDonRepository ChiTietHoaDonRepository { get; private set; }
        public IDanhGiaRepository DanhGiaRepository { get; private set; }
        public IYeuThichRepository YeuThichRepository { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            VatTuRepository = new VatTuRepository(_context);
            LoaiVatTuRepository = new LoaiVatTuRepository(_context);
            NhaCungCapRepository = new NhaCungCapRepository(_context);
            KhachHangRepository = new KhachHangRepository(_context);
            NhanVienRepository = new NhanVienRepository(_context);
            DonHangRepository = new DonHangRepository(_context);
            ChiTietDonHangRepository = new ChiTietDonHangRepository(_context);
            HoaDonRepository = new HoaDonRepository(_context);
            ChiTietHoaDonRepository = new ChiTietHoaDonRepository(_context);
            DanhGiaRepository = new DanhGiaRepository(_context);
            YeuThichRepository = new YeuThichRepository(_context);
        }

        public int Save()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
