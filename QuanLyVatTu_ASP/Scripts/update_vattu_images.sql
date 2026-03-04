-- =====================================================
-- Script: Cập nhật HinhAnh cho bảng VatTu
-- DB: QuanLyVatTu
-- Cách dùng: Mở SSMS -> chọn DB QuanLyVatTu -> chạy script này
-- Tác dụng: Gán ảnh từ wwwroot/images/vattu vào cột HinhAnh
--           Nếu số VatTu > 38 thì ảnh được xoay vòng tự động
-- =====================================================

USE QuanLyVatTu;
GO

-- Xem trạng thái TRƯỚC khi update
-- SELECT ID, TenVatTu, HinhAnh FROM VatTu ORDER BY ID;

-- =====================================================
-- Bảng tạm chứa danh sách ảnh (38 file)
-- =====================================================
IF OBJECT_ID('tempdb..#Images') IS NOT NULL DROP TABLE #Images;
CREATE TABLE #Images (RowNum INT IDENTITY(1,1) PRIMARY KEY, FilePath NVARCHAR(300));

INSERT INTO #Images (FilePath) VALUES
('/images/vattu/084e6721-314c-4a10-82ae-658a70b10914.webp'),
('/images/vattu/0cc1ef40-284a-4753-a556-2eb9569b1f6e.webp'),
('/images/vattu/1246774d-430c-4dd3-854b-dc444be10745.webp'),
('/images/vattu/18f55e36-a5b4-4ae1-9f20-5d25b201893a.webp'),
('/images/vattu/1969c063-f91d-42b8-a1de-a4f616ae1ae2.webp'),
('/images/vattu/1ab1265f-9885-42c2-ab9d-956ce423f958.jpg'),
('/images/vattu/2a2e2212-bf39-49c4-ba1d-74a9952f5d4f.webp'),
('/images/vattu/2bb96624-3363-4d75-a32a-7bf500582eb2.webp'),
('/images/vattu/41372293-d12a-4877-975b-7a87e8fcf260.webp'),
('/images/vattu/4322856a-99b9-4b81-8ccd-87470755b448.webp'),
('/images/vattu/4516306e-e440-41fc-94a4-521619d7596d.webp'),
('/images/vattu/496fa054-d54b-4eb8-bcad-f47d78a4da7d.webp'),
('/images/vattu/4a6c46cd-6a07-4304-a536-bd94530d4f74.webp'),
('/images/vattu/4c7696f3-5d38-4bb6-8e83-276b8db14a78.webp'),
('/images/vattu/58409ae2-a3a3-4521-aee9-3a05423e488b.webp'),
('/images/vattu/64409474-ddc3-4090-9424-52b09e8d1898.webp'),
('/images/vattu/659cc825-b652-46c2-ae28-d59990148755.webp'),
('/images/vattu/8089d096-aec8-4a6c-a5ea-6e5e59cd540a.webp'),
('/images/vattu/84f88b12-4d29-479c-983c-5ca9ca417594.webp'),
('/images/vattu/8820997d-dbbb-42aa-b900-058606e97e55.webp'),
('/images/vattu/8f74570e-cabc-420d-bd7e-1c9da810bb69.webp'),
('/images/vattu/9448db02-2e24-4399-8803-a521a1c5eaca.webp'),
('/images/vattu/958d65b2-3f4a-4d22-b3e2-8a0d6aa58888.webp'),
('/images/vattu/9fc938cc-bd7f-4f9f-867b-d91ca39a1ea0.webp'),
('/images/vattu/b558b009-5c89-464a-8420-73e7b065bf66.webp'),
('/images/vattu/bba8fc9c-46a5-4298-8066-f50f6b89a11f.webp'),
('/images/vattu/c1c8aca2-c9fe-4f19-a543-00f2c453a3d4.webp'),
('/images/vattu/c317cf09-330e-4b8a-96a4-e849975f150d.webp'),
('/images/vattu/c7c0bc4c-5d21-452e-87d5-56ac00db6fdf.webp'),
('/images/vattu/cf11799f-f090-4a7b-aa38-a807ffbfe363.webp'),
('/images/vattu/d5b00e9b-4f46-40fc-8169-1438cb62439d.webp'),
('/images/vattu/dbe17724-a4fd-41eb-8994-9f8b194b7c83.webp'),
('/images/vattu/e678edbf-8568-4f73-b1a4-ec568743a2a0.webp'),
('/images/vattu/edc22010-72d6-462d-be3d-0dfc19472bb7.webp'),
('/images/vattu/eedf78fb-3b42-4cb9-a2e9-f70e26c8bd62.webp'),
('/images/vattu/f71ebe51-0c3b-4672-94fe-aad933f54b66.webp'),
('/images/vattu/fb090c73-ec19-46d6-be67-b18bfc8d007d.webp'),
('/images/vattu/fc53ad58-42c1-4b5a-82a1-c93f2de214e4.webp');

-- =====================================================
-- UPDATE: Gán ảnh theo thứ tự ID (xoay vòng nếu > 38 sp)
-- =====================================================
DECLARE @TongSoAnh INT = (SELECT COUNT(*) FROM #Images);

UPDATE v
SET v.HinhAnh = img.FilePath
FROM (
    SELECT 
        vt.ID,
        -- Xoay vòng: (RowNum - 1) % TotalImages + 1
        ((ROW_NUMBER() OVER (ORDER BY vt.ID) - 1) % @TongSoAnh) + 1 AS ImageIndex
    FROM VatTu vt
) AS mapped
INNER JOIN VatTu v ON v.ID = mapped.ID
INNER JOIN #Images img ON img.RowNum = mapped.ImageIndex;

-- Xem kết quả SAU khi update
SELECT ID, TenVatTu, HinhAnh FROM VatTu ORDER BY ID;

PRINT CONCAT('✅ Cập nhật xong! Tổng số VatTu đã được gán ảnh: ', (SELECT COUNT(*) FROM VatTu WHERE HinhAnh IS NOT NULL));

DROP TABLE #Images;
GO
