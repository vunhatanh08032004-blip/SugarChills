-- =============================================================
-- SugarChills — Bảng lưu tin nhắn liên hệ từ khách hàng
-- Chạy 1 lần trong SQL Server Management Studio (SSMS)
-- =============================================================
USE QL_MatHangAnUong;
GO

IF OBJECT_ID(N'dbo.LienLacs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LienLacs (
        MaLienLac   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        HoTen       NVARCHAR(100)  NOT NULL,
        SoDienThoai VARCHAR(15)    NULL,
        Email       NVARCHAR(150)  NULL,
        NoiDung     NVARCHAR(2000) NOT NULL,
        NgayGui     DATETIME       NOT NULL CONSTRAINT DF_LienLacs_NgayGui DEFAULT (GETDATE()),
        DaDoc       BIT            NOT NULL CONSTRAINT DF_LienLacs_DaDoc  DEFAULT (0)
    );
    PRINT N'Đã tạo bảng LienLacs';
END
ELSE
    PRINT N'Bảng LienLacs đã tồn tại — bỏ qua';
GO
