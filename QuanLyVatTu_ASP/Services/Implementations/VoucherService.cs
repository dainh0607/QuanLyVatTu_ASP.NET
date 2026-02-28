using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Models;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class VoucherService : IVoucherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public VoucherService(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        // ==========================================
        // A. Nghiệp vụ Thu thập ("Lưu mã")
        // ==========================================
        public async Task<ServiceResult> SaveVoucherToWalletAsync(int khachHangId, int voucherId)
        {
            // 1. Kiểm tra voucher tồn tại và còn hiệu lực
            var voucher = await _unitOfWork.VoucherRepository.GetByIdAsync(voucherId, tracking: true);
            if (voucher == null)
                return ServiceResult.Fail("Mã voucher không tồn tại.");

            if (voucher.TrangThaiGoc != "ACTIVE")
                return ServiceResult.Fail("Mã voucher không còn hoạt động.");

            if (voucher.ThoiGianKetThuc <= DateTime.Now)
                return ServiceResult.Fail("Mã voucher đã hết hạn.");

            // 2. Giới hạn cá nhân: mỗi người chỉ được lưu 1 lần
            var alreadySaved = await _unitOfWork.ViVoucherRepository.ExistsAsync(khachHangId, voucherId);
            if (alreadySaved)
                return ServiceResult.Fail("Bạn đã lưu mã này rồi.");

            // 3. Giới hạn hệ thống: total_quantity
            // Đếm tổng số ví đã lưu mã này (chính xác hơn SoLuongDaDung vì đây là lượt lưu, không phải lượt dùng)
            var totalSaved = await _context.ViVoucherKhachHangs
                .CountAsync(v => v.MaVoucherGoc == voucherId);
            if (totalSaved >= voucher.TongSoLuong)
                return ServiceResult.Fail("Mã voucher đã hết số lượng phát hành.");

            // 4. Lưu vào ví
            var viVoucher = new ViVoucherKhachHang
            {
                MaKhachHang = khachHangId,
                MaVoucherGoc = voucherId,
                ThoiGianLuuMa = DateTime.Now,
                TrangThaiTrongVi = "AVAILABLE"
            };

            await _unitOfWork.ViVoucherRepository.AddAsync(viVoucher);
            await _unitOfWork.SaveAsync();

            return ServiceResult.Ok("Lưu mã thành công!");
        }

        // ==========================================
        // B. Nghiệp vụ Áp dụng Voucher tại Checkout
        // ==========================================
        public async Task<ServiceResult<decimal>> ApplyVoucherAsync(int khachHangId, int voucherId, int donHangId, decimal orderTotal)
        {
            // Sử dụng Transaction + Row Lock để chống Race Condition
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Lock row voucher để chống tranh chấp
                var voucher = await _context.Vouchers
                    .FromSqlRaw("SELECT * FROM Voucher WITH (UPDLOCK, ROWLOCK) WHERE ID = {0}", voucherId)
                    .FirstOrDefaultAsync();

                if (voucher == null)
                    return ServiceResult<decimal>.Fail("Voucher không tồn tại.");

                // 2. Kiểm tra trạng thái mã trong ví khách phải là AVAILABLE
                var viVoucher = await _unitOfWork.ViVoucherRepository
                    .GetByKhachHangAndVoucherAsync(khachHangId, voucherId);
                if (viVoucher == null || viVoucher.TrangThaiTrongVi != "AVAILABLE")
                    return ServiceResult<decimal>.Fail("Mã voucher không khả dụng trong ví của bạn.");

                // 3. Kiểm tra usage_limit_per_user
                var usageCount = await _unitOfWork.LichSuSuDungVoucherRepository
                    .CountUsageAsync(khachHangId, voucherId);
                if (usageCount >= voucher.GioiHanSuDungMoiUser)
                    return ServiceResult<decimal>.Fail("Bạn đã sử dụng hết lượt dùng cho mã này.");

                // 4. Kiểm tra total_quantity hệ thống
                if (voucher.SoLuongDaDung >= voucher.TongSoLuong)
                    return ServiceResult<decimal>.Fail("Mã voucher đã hết lượt sử dụng trên hệ thống.");

                // 5. Kiểm tra giá trị đơn hàng tối thiểu
                if (orderTotal < voucher.GiaTriDonHangToiThieu)
                    return ServiceResult<decimal>.Fail($"Đơn hàng phải từ {voucher.GiaTriDonHangToiThieu:N0}₫ để sử dụng mã này.");

                // 6. Kiểm tra thời hạn
                if (voucher.ThoiGianKetThuc <= DateTime.Now || voucher.ThoiGianBatDau > DateTime.Now)
                    return ServiceResult<decimal>.Fail("Mã voucher không còn trong thời hạn sử dụng.");

                // 7. Tính toán số tiền giảm
                decimal soTienGiam = 0;
                if (voucher.LoaiGiamGia == "PERCENT")
                {
                    soTienGiam = orderTotal * voucher.GiaTriGiam / 100;
                    // Áp dụng giới hạn giảm tối đa
                    if (voucher.SoTienGiamToiDa.HasValue && soTienGiam > voucher.SoTienGiamToiDa.Value)
                        soTienGiam = voucher.SoTienGiamToiDa.Value;
                }
                else // FIXED
                {
                    soTienGiam = voucher.GiaTriGiam;
                }

                // Không giảm quá tổng tiền
                if (soTienGiam > orderTotal)
                    soTienGiam = orderTotal;

                // 8. Lấy tên khách hàng cho snapshot
                var khachHang = await _unitOfWork.KhachHangRepository.GetByIdAsync(khachHangId);
                string tenKhachHang = khachHang?.HoTen ?? "Khách hàng";

                // 9. Insert lịch sử sử dụng (APPLIED)
                var lichSu = new LichSuSuDungVoucher
                {
                    MaVoucherGoc = voucherId,
                    MaDonHang = donHangId,
                    MaKhachHang = khachHangId,
                    TenKhachHangSnapshot = tenKhachHang,
                    SoTienGiamSnapshot = soTienGiam,
                    ThoiGianSuDung = DateTime.Now,
                    TrangThaiSuDung = "APPLIED"
                };
                await _unitOfWork.LichSuSuDungVoucherRepository.AddAsync(lichSu);

                // 10. Update Voucher.SoLuongDaDung
                voucher.SoLuongDaDung += 1;

                // 11. Update ViVoucher.TrangThaiTrongVi = USED
                viVoucher.TrangThaiTrongVi = "USED";

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                return ServiceResult<decimal>.Ok(soTienGiam, $"Áp dụng mã giảm giá thành công! Giảm {soTienGiam:N0}₫");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult<decimal>.Fail("Lỗi khi áp dụng voucher: " + ex.Message);
            }
        }

        // ==========================================
        // C. Nghiệp vụ Hủy đơn & Trả/Phạt mã
        // ==========================================
        public async Task<ServiceResult> HandleOrderCancelVoucherAsync(int donHangId, string trangThaiDonHang)
        {
            var lichSu = await _unitOfWork.LichSuSuDungVoucherRepository.GetByDonHangAsync(donHangId);
            if (lichSu == null)
                return ServiceResult.Ok("Đơn hàng không sử dụng voucher.");

            // Nếu đã xử lý rồi (REFUNDED hoặc BURNED), bỏ qua
            if (lichSu.TrangThaiSuDung != "APPLIED")
                return ServiceResult.Ok("Voucher đã được xử lý trước đó.");

            var voucher = await _unitOfWork.VoucherRepository.GetByIdAsync(lichSu.MaVoucherGoc, tracking: true);
            var viVoucher = await _unitOfWork.ViVoucherRepository
                .GetByKhachHangAndVoucherAsync(lichSu.MaKhachHang, lichSu.MaVoucherGoc);

            if (trangThaiDonHang == "Chờ xác nhận")
            {
                // -------- HỦY SỚM: Hoàn mã --------
                lichSu.TrangThaiSuDung = "REFUNDED";

                if (viVoucher != null)
                    viVoucher.TrangThaiTrongVi = "AVAILABLE";

                if (voucher != null && voucher.SoLuongDaDung > 0)
                    voucher.SoLuongDaDung -= 1;
            }
            else
            {
                // -------- HỦY MUỘN (Đang xử lý / Đã giao): Thiêu hủy mã --------
                lichSu.TrangThaiSuDung = "BURNED";

                if (viVoucher != null)
                    viVoucher.TrangThaiTrongVi = "USED";

                // Giữ nguyên SoLuongDaDung (không giảm bộ đếm)
            }

            await _unitOfWork.SaveAsync();

            return ServiceResult.Ok(trangThaiDonHang == "Chờ xác nhận"
                ? "Mã voucher đã được hoàn lại vào ví."
                : "Mã voucher đã bị thiêu hủy.");
        }

        // ==========================================
        // Lấy danh sách ví / khả dụng
        // ==========================================
        public async Task<IEnumerable<ViVoucherKhachHang>> GetWalletAsync(int khachHangId)
        {
            return await _unitOfWork.ViVoucherRepository.GetByKhachHangAsync(khachHangId);
        }

        public async Task<IEnumerable<ViVoucherKhachHang>> GetAvailableForCheckoutAsync(int khachHangId)
        {
            return await _unitOfWork.ViVoucherRepository.GetAvailableAsync(khachHangId);
        }
    }
}
