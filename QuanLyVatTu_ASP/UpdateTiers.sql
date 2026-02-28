UPDATE HangThanhVien SET TenHang = N'Sắt', ChiTieuToiThieu = 0, PhanTramChietKhau = 0 WHERE ID = 1;
UPDATE HangThanhVien SET TenHang = N'Đồng', ChiTieuToiThieu = 2000000, PhanTramChietKhau = 1 WHERE ID = 2;
UPDATE HangThanhVien SET TenHang = N'Bạc', ChiTieuToiThieu = 5000000, PhanTramChietKhau = 2 WHERE ID = 3;
UPDATE HangThanhVien SET TenHang = N'Vàng', ChiTieuToiThieu = 10000000, PhanTramChietKhau = 5 WHERE ID = 4;
UPDATE HangThanhVien SET TenHang = N'Bạch kim', ChiTieuToiThieu = 20000000, PhanTramChietKhau = 8 WHERE ID = 5;

IF NOT EXISTS (SELECT 1 FROM HangThanhVien WHERE TenHang = N'Kim cương') 
BEGIN
    INSERT INTO HangThanhVien (TenHang, ChiTieuToiThieu, PhanTramChietKhau, NgayTao) 
    VALUES (N'Kim cương', 50000000, 12, GETDATE());
END

UPDATE KhachHang
SET MaHangThanhVien = (SELECT ID FROM HangThanhVien WHERE TenHang = N'Bạc')
WHERE ID = 1;
