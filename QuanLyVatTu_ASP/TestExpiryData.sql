-- SCRIPT TEST HẾT HẠN VOUCHER VÀ HẠNG THÀNH VIÊN
-- CHẠY SCRIPT NÀY TRONG SQL SERVER MANG TÍNH CHẤT DEMO CƠ CHẾ RỚT HẠNG VÀ HẾT HẠN

USE QuanLyVatTu;
GO

-----------------------------------------------------------
-- 1. TEST VOUCHER HẾT HẠN
-----------------------------------------------------------
PRINT N'--- ĐANG CẬP NHẬT VOUCHER (SALE10) HẾT HẠN VÀO NGÀY HÔM QUA ---'
-- Lấy Voucher giảm 10% (giả sử ID = 2 hoặc tên = SALE10) và ép cho nó hết hạn vào ngày hôm qua
UPDATE Voucher
SET ThoiGianKetThuc = DATEADD(day, -1, GETDATE())
WHERE MaVoucher = 'SALE10';

-- Cập nhật toàn bộ các ví voucher của khách hàng chứa mã này thành "Đã hết hạn" 
-- (Thực tế khi Load giao diện, code C# sẽ tự ẩn hoặc disable voucher này nếu query ThoiGianKetThuc < Now)


-----------------------------------------------------------
-- 2. TEST CHUẨN BỊ RỚT HẠNG THÀNH VIÊN (365 NGÀY)
-----------------------------------------------------------
PRINT N'--- ĐANG ÉP KHÁCH HÀNG ID=1 (Cường) ĐẾN NGÀY XÉT DUYỆT HẠNG (Hết hạn trong hôm nay) ---'

-- Set ngày hết hạn hạng của KH ID 1 về chính ngày hôm nay để đêm nay (hoặc ngay bây giờ nếu tự trigger Job) hệ thống sẽ quét
UPDATE KhachHang
SET NgayHetHanHang = CAST(GETDATE() AS DATE), -- Hết hạn chính xác vào ngày hôm nay
    NgayLenHang = DATEADD(day, -365, GETDATE()) -- Giả lập đã lên hạng từ cách đây tròn 1 năm
WHERE ID = 1;


-- Xóa hoặc lùi ngày các đơn hàng cũ của khách này cách đây HƠN 1 NĂM để tổng chi tiêu 365 ngày qua = 0
-- (Đảm bảo chắc chắn rớt hạng vì không đủ chi tiêu)
PRINT N'--- ĐANG LÙI NGÀY ĐẶT HÀNG CỦA KHÁCH HÀNG 1 VỀ 2 NĂM TRƯỚC ĐỂ KHÔNG ĐẠT KPI CHI TIÊU ---'
UPDATE DonHang
SET NgayDat = DATEADD(day, -400, GETDATE())
WHERE KhachHangId = 1;

PRINT N'--- HOÀN TẤT CẬP NHẬT. HÃY ĐỌC HƯỚNG DẪN BÊN DƯỚI ĐỂ TEST ---';
