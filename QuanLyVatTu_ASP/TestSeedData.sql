-- 1. Thêm Hạng Thành Viên
INSERT INTO HangThanhVien (TenHang, ChiTieuToiThieu, PhanTramChietKhau, NgayTao) VALUES
(N'Sắt',             0,          0,    GETDATE()),
(N'Đồng',      2000000,          1,    GETDATE()),
(N'Bạc',       5000000,          2,    GETDATE()),
(N'Vàng',     10000000,          5,    GETDATE()),
(N'Bạch kim', 20000000,          8,    GETDATE()),
(N'Kim cương',50000000,         12,    GETDATE());

-- 2. Thêm Mã Voucher
INSERT INTO Voucher (MaVoucher, LoaiGiamGia, GiaTriGiam, SoTienGiamToiDa, GiaTriDonHangToiThieu, 
                     ThoiGianBatDau, ThoiGianKetThuc, TongSoLuong, SoLuongDaDung, 
                     GioiHanSuDungMoiUser, TrangThaiGoc, NgayTao) VALUES
('SALE10',    'PERCENT', 10,    500000,   1000000,  '2026-01-01', '2026-12-31', 100, 0, 1, 'ACTIVE', GETDATE()),
('SALE20',    'PERCENT', 20,   1000000,   3000000,  '2026-01-01', '2026-12-31',  50, 0, 1, 'ACTIVE', GETDATE()),
('VIP30',     'PERCENT', 30,   2000000,   5000000,  '2026-01-01', '2026-12-31',  20, 0, 1, 'ACTIVE', GETDATE()),
('GIAM50K',   'FIXED',   50000,  NULL,     500000,  '2026-01-01', '2026-12-31', 200, 0, 2, 'ACTIVE', GETDATE()),
('GIAM200K',  'FIXED',  200000,  NULL,    2000000,  '2026-01-01', '2026-12-31',  30, 0, 1, 'ACTIVE', GETDATE());

-- 3. Cập nhật Khách hàng (Reset ID = 1 về hạng Bạc và có điểm)
UPDATE KhachHang
SET MaHangThanhVien = (SELECT ID FROM HangThanhVien WHERE TenHang = N'Bạc'),
    NgayLenHang = GETDATE(),
    NgayHetHanHang = DATEADD(YEAR, 1, GETDATE()),
    DiemTichLuy = 50000
WHERE ID = 1;

-- 4. Lưu mã vào ví Khách hàng ID = 1
INSERT INTO ViVoucherKhachHang (MaKhachHang, MaVoucherGoc, ThoiGianLuuMa, TrangThaiTrongVi, NgayTao) VALUES
(1, (SELECT ID FROM Voucher WHERE MaVoucher = 'SALE10'),   GETDATE(), 'AVAILABLE', GETDATE()),
(1, (SELECT ID FROM Voucher WHERE MaVoucher = 'GIAM50K'),  GETDATE(), 'AVAILABLE', GETDATE());
