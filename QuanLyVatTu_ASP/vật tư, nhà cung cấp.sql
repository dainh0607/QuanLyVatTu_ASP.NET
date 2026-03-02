INSERT INTO LoaiVatTu (MaHienThi, TenLoaiVatTu, MoTa, NgayTao) VALUES
('LV01', N'Máy cắt', N'Các loại máy cắt dùng trong xây dựng', GETDATE()),
('LV02', N'Máy đầm', N'Các loại máy đầm nền, đầm cóc', GETDATE()),
('LV03', N'Máy mài', N'Các loại máy mài sàn bê tông', GETDATE()),
('LV04', N'Máy xoa nền', N'Các loại máy xoa nền bê tông', GETDATE()),
('LV05', N'Máy đa năng', N'Các loại máy đa năng', GETDATE());

INSERT INTO NhaCungCap ( TenNhaCungCap, DiaChi, SoDienThoai, Email) VALUES
( N'Công ty Thiết bị Dewalt VN', N'Hà Nội', '0901234567', 'contact@dewalt.vn'),
( N'Đại lý Máy xây dựng Z730', N'TP. Hồ Chí Minh', '0912345678', 'sales@z730.com'),
( N'Tổng kho Makita Miền Bắc', N'Hải Phòng', '0923456789', 'info@makita.com.vn'),
( N'Thiết bị Xây dựng Kunwoo', N'Đà Nẵng', '0934567890', 'kunwoo@gmail.com'),
( N'Cơ khí Điện máy 220V', N'Bình Dương', '0945678901', 'dienmay220v@yahoo.com'),
( N'Công ty Đông Phong', N'Nam Định', '0956789012', 'dongphong@dongphong.vn'),
( N'Động cơ Honda Việt Nam', N'Vĩnh Phúc', '0967890123', 'honda_engine@honda.com.vn'),
( N'Phân phối Mikasa Nhật Bản', N'Cần Thơ', '0978901234', 'mikasa@japan-equip.com'),
( N'Máy xây dựng NIKI Pro', N'Quảng Ninh', '0989012345', 'niki@nikipro.com'),
( N'Thiết bị Total Toàn Cầu', N'Đồng Nai', '0990123456', 'total_vn@total.com');

INSERT INTO VatTu (TenVatTu, HinhAnh, DonViTinh, GiaNhap, GiaBan, SoLuongTon, MaLoaiVatTu, MaNhaCungCap, NgayTao) 
VALUES
(N'Máy cắt cầm tay Dewalt', N'Máy cắt cầm tay Dewalt DCS356N-KR.jpg', N'Cái', 1000000, 1200000, 10, 1, 1, GETDATE()),
(N'Máy cắt ron gạch Model Z730', N'Máy cắt ron gạch, cắt mạch gạch - Model Z730.webp', N'Cái', 2000000, 2500000, 5, 1, 2, GETDATE()),
(N'Máy cắt sắt bàn Makita M2403B', N'Máy cắt sắt bàn Makita M2403B Chính hãng.png', N'Cái', 3000000, 3500000, 8, 1, 3, GETDATE()),
(N'Máy cắt sắt xây dựng Kunwoo', N'Máy cắt sắt xây dựng Kunwoo KMC-25W.webp', N'Cái', 4000000, 4500000, 6, 1, 4, GETDATE()),
(N'Máy cắt sắt bàn Makita M2400B', N'may-cat-sat-ban-makita-m2400b-355mm-1573542527.jpg', N'Cái', 20000000, 20500000, 12, 1, 10, GETDATE()),
(N'Máy cắt sắt GQ40', N'may-cat-sat-gq40.jpg', N'Cái', 21000000, 21500000, 13, 1, 1, GETDATE()),
(N'Máy mài cắt cầm tay Makita GA5020', N'may-mai-cat-cam-tay-makita-ga5020-125mm.webp', N'Cái', 22000000, 22500000, 14, 1, 2, GETDATE()),

(N'Máy đầm cóc chạy điện 220V', N'Máy đầm cóc chạy điện 220V.jpg', N'Cái', 5000000, 5500000, 7, 2, 5, GETDATE()),
(N'Máy đầm cóc Đông Phong', N'Máy đầm cóc chạy xăng Đông Phong.jpg', N'Cái', 6000000, 6500000, 4, 2, 6, GETDATE()),
(N'Máy đầm cóc Honda RM80', N'Máy đầm cóc chạy xăng Honda RM80.jpg', N'Cái', 7000000, 7500000, 3, 2, 7, GETDATE()),
(N'Máy đầm cóc Honda HTR-70T1', N'MÁY ĐẦM CÓC HONDA HTR-70T1 (GXR100).png', N'Cái', 8000000, 8500000, 2, 2, 8, GETDATE()),
(N'Máy đầm cóc Mikasa MT72FW', N'Máy đầm cóc Mikasa MT72FW Nhật.jpg', N'Cái', 9000000, 9500000, 1, 2, 9, GETDATE()),
(N'Máy đầm cóc Niki NK55', N'Máy đầm cóc Niki NK55 lắp động cơ Robin EH09.jpg', N'Cái', 10000000, 10500000, 2, 2, 10, GETDATE()),
(N'Máy đầm cóc NIKI Pro MT55', N'Máy Đầm Cóc NIKI Pro MT55.jpg', N'Cái', 11000000, 11500000, 3, 2, 1, GETDATE()),
(N'Máy đầm cóc Wacker Neuson MS64A', N'Máy Đầm Cóc Wacker Neuson MS64A.png', N'Cái', 12000000, 12500000, 4, 2, 2, GETDATE()),

(N'Máy mài sàn bê tông DMS 350', N'Máy Mài Sàn Bê Tông DMS 350-7.5 KW.jpg', N'Cái', 13000000, 13500000, 5, 3, 3, GETDATE()),
(N'Máy mài sàn bê tông JS S650', N'Máy mài sàn bê tông JS S650.png', N'Cái', 14000000, 14500000, 6, 3, 4, GETDATE()),
(N'Máy mài sàn bê tông Xing Yi X9', N'Máy mài sàn bê tông Xing Yi X9.jpg', N'Cái', 15000000, 15500000, 7, 3, 5, GETDATE()),
(N'Máy mài sàn hút bụi MS460S', N'Máy Mài Sàn Hút Bụi MS460S.png', N'Cái', 16000000, 16500000, 8, 3, 6, GETDATE()),

(N'Máy xoa nền bê tông L2 (220V)', N'Máy xoa nền bê tông L2 chạy điện 2.2KW (220V).jpg', N'Cái', 17000000, 17500000, 9, 4, 7, GETDATE()),
(N'Máy xoa nền đôi công suất lớn', N'Máy Xoa Nền Đôi Công Suất Lớn Làm Mịn Sàn.jpg', N'Cái', 18000000, 18500000, 10, 4, 8, GETDATE()),
(N'Máy xoa nền bê tông dùng xăng TP936', N'TP936-2 - Máy xoa nền bê tông dùng xăng.webp', N'Cái', 23000000, 23500000, 15, 4, 3, GETDATE()),

(N'Máy cắt đa năng Total 750W', N'may-cat-da-nang-cam-tay-750W-Total-TMFS7501.jpg', N'Cái', 19000000, 19500000, 11, 5, 9, GETDATE());