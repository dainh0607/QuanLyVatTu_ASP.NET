BEGIN TRANSACTION;
CREATE TABLE [Voucher] (
    [ID] int NOT NULL IDENTITY,
    [MaVoucher] varchar(50) NOT NULL,
    [LoaiGiamGia] varchar(20) NOT NULL,
    [GiaTriGiam] decimal(18,2) NOT NULL,
    [SoTienGiamToiDa] decimal(18,2) NULL,
    [GiaTriDonHangToiThieu] decimal(18,2) NOT NULL,
    [ThoiGianBatDau] datetime NOT NULL,
    [ThoiGianKetThuc] datetime NOT NULL,
    [TongSoLuong] int NOT NULL,
    [SoLuongDaDung] int NOT NULL,
    [GioiHanSuDungMoiUser] int NOT NULL,
    [MaNhanVienTao] int NULL,
    [TrangThaiGoc] varchar(20) NOT NULL,
    [NgayTao] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_Voucher] PRIMARY KEY ([ID]),
    CONSTRAINT [CK_Voucher_GiaTriDonHangToiThieu] CHECK ([GiaTriDonHangToiThieu] >= 0),
    CONSTRAINT [CK_Voucher_GiaTriGiam] CHECK (([LoaiGiamGia] = 'FIXED' AND [GiaTriGiam] > 0) OR ([LoaiGiamGia] = 'PERCENT' AND [GiaTriGiam] > 0 AND [GiaTriGiam] <= 100)),
    CONSTRAINT [CK_Voucher_GioiHanSuDung] CHECK ([GioiHanSuDungMoiUser] >= 1),
    CONSTRAINT [CK_Voucher_LoaiGiamGia] CHECK ([LoaiGiamGia] IN ('PERCENT', 'FIXED')),
    CONSTRAINT [CK_Voucher_SoLuong_HopLe] CHECK ([SoLuongDaDung] <= [TongSoLuong]),
    CONSTRAINT [CK_Voucher_SoLuongDaDung] CHECK ([SoLuongDaDung] >= 0),
    CONSTRAINT [CK_Voucher_ThoiGian_HopLe] CHECK ([ThoiGianBatDau] < [ThoiGianKetThuc]),
    CONSTRAINT [CK_Voucher_TongSoLuong] CHECK ([TongSoLuong] >= 0),
    CONSTRAINT [CK_Voucher_TrangThaiGoc] CHECK ([TrangThaiGoc] IN ('ACTIVE', 'EXPIRED', 'REVOKED')),
    CONSTRAINT [FK_Voucher_NhanVien_MaNhanVienTao] FOREIGN KEY ([MaNhanVienTao]) REFERENCES [NhanVien] ([ID]) ON DELETE SET NULL
);

CREATE TABLE [LichSuSuDungVoucher] (
    [ID] int NOT NULL IDENTITY,
    [MaVoucherGoc] int NOT NULL,
    [MaDonHang] int NOT NULL,
    [MaKhachHang] int NOT NULL,
    [TenKhachHangSnapshot] nvarchar(255) NOT NULL,
    [SoTienGiamSnapshot] decimal(18,2) NOT NULL,
    [ThoiGianSuDung] datetime NOT NULL DEFAULT (GETDATE()),
    [TrangThaiSuDung] varchar(20) NOT NULL,
    [NgayTao] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_LichSuSuDungVoucher] PRIMARY KEY ([ID]),
    CONSTRAINT [CK_LichSuSuDungVoucher_SoTienGiam] CHECK ([SoTienGiamSnapshot] >= 0),
    CONSTRAINT [CK_LichSuSuDungVoucher_TrangThai] CHECK ([TrangThaiSuDung] IN ('APPLIED', 'REFUNDED', 'BURNED')),
    CONSTRAINT [FK_LichSuSuDungVoucher_DonHang_MaDonHang] FOREIGN KEY ([MaDonHang]) REFERENCES [DonHang] ([ID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LichSuSuDungVoucher_KhachHang_MaKhachHang] FOREIGN KEY ([MaKhachHang]) REFERENCES [KhachHang] ([ID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LichSuSuDungVoucher_Voucher_MaVoucherGoc] FOREIGN KEY ([MaVoucherGoc]) REFERENCES [Voucher] ([ID]) ON DELETE NO ACTION
);

CREATE TABLE [ViVoucherKhachHang] (
    [ID] int NOT NULL IDENTITY,
    [MaKhachHang] int NOT NULL,
    [MaVoucherGoc] int NOT NULL,
    [ThoiGianLuuMa] datetime NOT NULL DEFAULT (GETDATE()),
    [TrangThaiTrongVi] varchar(20) NOT NULL,
    [NgayTao] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_ViVoucherKhachHang] PRIMARY KEY ([ID]),
    CONSTRAINT [CK_ViVoucherKhachHang_TrangThai] CHECK ([TrangThaiTrongVi] IN ('AVAILABLE', 'USED', 'EXPIRED')),
    CONSTRAINT [FK_ViVoucherKhachHang_KhachHang_MaKhachHang] FOREIGN KEY ([MaKhachHang]) REFERENCES [KhachHang] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ViVoucherKhachHang_Voucher_MaVoucherGoc] FOREIGN KEY ([MaVoucherGoc]) REFERENCES [Voucher] ([ID]) ON DELETE CASCADE
);

CREATE INDEX [IX_LichSuSuDung_DonHang] ON [LichSuSuDungVoucher] ([MaDonHang]);

CREATE UNIQUE INDEX [IX_LichSuSuDung_DonHang_Voucher_Unique] ON [LichSuSuDungVoucher] ([MaDonHang], [MaVoucherGoc]);

CREATE INDEX [IX_LichSuSuDung_KhachHang_Voucher] ON [LichSuSuDungVoucher] ([MaKhachHang], [MaVoucherGoc]);

CREATE INDEX [IX_LichSuSuDungVoucher_MaVoucherGoc] ON [LichSuSuDungVoucher] ([MaVoucherGoc]);

CREATE UNIQUE INDEX [IX_ViVoucher_KhachHang_Voucher_Unique] ON [ViVoucherKhachHang] ([MaKhachHang], [MaVoucherGoc]);

CREATE INDEX [IX_ViVoucherKhachHang_MaVoucherGoc] ON [ViVoucherKhachHang] ([MaVoucherGoc]);

CREATE INDEX [IX_Voucher_MaNhanVienTao] ON [Voucher] ([MaNhanVienTao]);

CREATE UNIQUE INDEX [IX_Voucher_MaVoucher] ON [Voucher] ([MaVoucher]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260225143336_HoanThienVoucherSystem', N'10.0.0');

COMMIT;
GO

