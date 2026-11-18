/* =====================================================================
   SugarChills - QL_MatHangAnUong
   Script tao CSDL + du lieu mau (SQL Server), khop voi thu muc Models
   Class: LoaiSanPham, SanPham, NguoiDung, KhuyenMai, DonHang,
          ChiTietDonHang, CuaHang
   Chay toan bo script trong SSMS (an toan chay lai nhieu lan).

   *** BAN GOP (1 FILE DUY NHAT) ***
   Gop tu 3 file: QL_MatHangAnUong.sql + TaoBang_LienLacs.sql + TaoBang_DanhGia.sql
   - Co them bang LienLacs  (tin nhan lien he)       -> GIU NGUYEN du lieu neu da co.
   - Co them bang DanhGia   (danh gia san pham)      -> bi XOA va tao lai (phu thuoc SanPham/NguoiDung).
   - CANH BAO: script xoa va tao lai SanPham, NguoiDung, DonHang... => mat du lieu dang co
     (tai khoan/don hang tu dang ky). Chi chay khi muon dua CSDL ve du lieu mau.
   - Thu tu: bang con (DanhGia) phai bi xoa TRUOC bang cha (SanPham, NguoiDung).

   Ghi chu du lieu mau (da thong nhat voi code):
   - HinhThucThanhToan chi gom 'COD' va 'QR' (giong GioHangController).
   - Phi giao hang 15.000d, mien phi khi TamTinh >= 150.000d.
   - TienGiam tinh dung nghiep vu KhuyenMai.TinhTienGiam:
       TongTien    : % tren TamTinh
       PhiGiaoHang : % tren phi giao hang (CHILL15 = freeship)
   - DonGia = gia ban thuc te + phu thu size (M +6.000, L +12.000) + topping.
   - Don 'Hoan thanh' hoac thanh toan QR: DaThanhToan = 1, SoTienConLai = 0.
   - Don 'Cho xac nhan' o hom nay; don cu thi 'Hoan thanh' / 'Da huy'.
   ===================================================================== */
IF DB_ID(N'QL_MatHangAnUong') IS NULL
    CREATE DATABASE QL_MatHangAnUong;
GO
USE QL_MatHangAnUong;
GO

-- Xoa bang cu (theo thu tu khoa ngoai) de chay lai duoc
IF OBJECT_ID(N'dbo.DanhGia',        N'U') IS NOT NULL DROP TABLE dbo.DanhGia;       -- bang con cua SanPham + NguoiDung
IF OBJECT_ID(N'dbo.ChiTietDonHang', N'U') IS NOT NULL DROP TABLE dbo.ChiTietDonHang;
IF OBJECT_ID(N'dbo.DonHang',        N'U') IS NOT NULL DROP TABLE dbo.DonHang;
IF OBJECT_ID(N'dbo.SanPham',        N'U') IS NOT NULL DROP TABLE dbo.SanPham;
IF OBJECT_ID(N'dbo.LoaiSanPham',    N'U') IS NOT NULL DROP TABLE dbo.LoaiSanPham;
IF OBJECT_ID(N'dbo.NguoiDung',      N'U') IS NOT NULL DROP TABLE dbo.NguoiDung;
IF OBJECT_ID(N'dbo.KhuyenMai',      N'U') IS NOT NULL DROP TABLE dbo.KhuyenMai;
IF OBJECT_ID(N'dbo.CuaHang',        N'U') IS NOT NULL DROP TABLE dbo.CuaHang;
GO

/* ===================== TAO BANG ===================== */

CREATE TABLE LoaiSanPham (
    MaLoai   INT IDENTITY(1,1) PRIMARY KEY,
    TenLoai  NVARCHAR(100) NOT NULL UNIQUE,
    MoTa     NVARCHAR(500) NULL,
    HinhAnh  NVARCHAR(500) NULL,
    HienThi  BIT NOT NULL DEFAULT 1,
    ThuTu    INT NOT NULL DEFAULT 0
);

CREATE TABLE SanPham (
    MaSP          INT IDENTITY(1,1) PRIMARY KEY,
    TenSP         NVARCHAR(150) NOT NULL UNIQUE,
    MaLoai        INT NOT NULL,
    Gia           DECIMAL(18,0) NOT NULL CHECK (Gia BETWEEN 1000 AND 10000000),
    GiaKhuyenMai  DECIMAL(18,0) NULL,
    HinhAnh       NVARCHAR(500) NULL,
    MoTa          NVARCHAR(1000) NULL,
    SoLuongTon    INT NOT NULL DEFAULT 100 CHECK (SoLuongTon >= 0),
    DangBan       BIT NOT NULL DEFAULT 1,
    NoiBat        BIT NOT NULL DEFAULT 0,
    NgayTao       DATETIME NOT NULL DEFAULT GETDATE(),
    LuotBan       INT NOT NULL DEFAULT 0 CHECK (LuotBan >= 0),
    CONSTRAINT FK_SanPham_Loai FOREIGN KEY (MaLoai) REFERENCES LoaiSanPham(MaLoai),
    CONSTRAINT CK_SanPham_GiaKM CHECK (GiaKhuyenMai IS NULL OR GiaKhuyenMai = 0 OR GiaKhuyenMai < Gia)
);

CREATE TABLE NguoiDung (
    MaND        INT IDENTITY(1,1) PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    Email       NVARCHAR(150) NOT NULL UNIQUE,
    MatKhau     NVARCHAR(100) NOT NULL,
    DienThoai   NVARCHAR(10)  NOT NULL CHECK (DienThoai LIKE '0[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
    DiaChi      NVARCHAR(250) NULL,
    VaiTro      NVARCHAR(20)  NOT NULL CHECK (VaiTro IN (N'Customer', N'Seller')),
    NgayDangKy  DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE KhuyenMai (
    MaKM          INT IDENTITY(1,1) PRIMARY KEY,
    TenKM         NVARCHAR(150) NOT NULL,
    MaGiamGia     NVARCHAR(50)  NOT NULL UNIQUE,
    PhanTramGiam  INT NOT NULL CHECK (PhanTramGiam BETWEEN 0 AND 100),
    GiamToiDa     DECIMAL(18,0) NOT NULL DEFAULT 0,
    DonToiThieu   DECIMAL(18,0) NOT NULL DEFAULT 0,
    NgayBatDau    DATE NOT NULL,
    NgayKetThuc   DATE NOT NULL,
    MoTa          NVARCHAR(500) NULL,
    KichHoat      BIT NOT NULL DEFAULT 1,
    LoaiGiam      NVARCHAR(20) NOT NULL DEFAULT N'TongTien'
                  CHECK (LoaiGiam IN (N'TongTien', N'PhiGiaoHang')),
    CONSTRAINT CK_KhuyenMai_Ngay CHECK (NgayKetThuc >= NgayBatDau)
);

CREATE TABLE DonHang (
    MaDH               INT IDENTITY(1001,1) PRIMARY KEY,
    MaND               INT NULL,
    TenNguoiNhan       NVARCHAR(100) NOT NULL,
    DienThoai          NVARCHAR(10)  NOT NULL CHECK (DienThoai LIKE '0[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
    DiaChi             NVARCHAR(250) NOT NULL,
    GhiChu             NVARCHAR(500) NULL,
    HinhThucThanhToan  NVARCHAR(50)  NOT NULL DEFAULT N'COD',
    NgayDat            DATETIME NOT NULL DEFAULT GETDATE(),
    TamTinh            DECIMAL(18,0) NOT NULL DEFAULT 0,
    TienGiam           DECIMAL(18,0) NOT NULL DEFAULT 0,
    PhiGiaoHang        DECIMAL(18,0) NOT NULL DEFAULT 0,
    TongTien           DECIMAL(18,0) NOT NULL DEFAULT 0,
    MaGiamGiaApDung    NVARCHAR(50) NULL,
    TrangThai          NVARCHAR(50) NOT NULL DEFAULT N'Chờ xác nhận'
                       CHECK (TrangThai IN (N'Chờ xác nhận', N'Đã xác nhận', N'Đang giao', N'Hoàn thành', N'Đã hủy')),
    DaThanhToan        BIT NOT NULL DEFAULT 0,
    SoTienConLai       DECIMAL(18,0) NOT NULL DEFAULT 0,
    CONSTRAINT FK_DonHang_NguoiDung FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND)
);

CREATE TABLE ChiTietDonHang (
    MaCT     INT IDENTITY(1,1) PRIMARY KEY,
    MaDH     INT NOT NULL,
    MaSP     INT NOT NULL,
    TenSP    NVARCHAR(150) NULL,
    SoLuong  INT NOT NULL CHECK (SoLuong BETWEEN 1 AND 100),
    DonGia   DECIMAL(18,0) NOT NULL CHECK (DonGia >= 0),
    TuyChon  NVARCHAR(200) NULL,
    CONSTRAINT FK_CT_DonHang FOREIGN KEY (MaDH) REFERENCES DonHang(MaDH) ON DELETE CASCADE,
    CONSTRAINT FK_CT_SanPham FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);

CREATE TABLE CuaHang (
    MaCH         INT IDENTITY(1,1) PRIMARY KEY,
    TenCH        NVARCHAR(150) NOT NULL,
    DiaChi       NVARCHAR(250) NOT NULL,
    TinhThanh    NVARCHAR(100) NOT NULL,
    QuanHuyen    NVARCHAR(100) NOT NULL,   -- thuc chat la Phuong/Xa
    SoDienThoai  NVARCHAR(20) NULL,
    GioMoCua     NVARCHAR(50) NULL
);

CREATE INDEX IX_SanPham_MaLoai      ON SanPham(MaLoai);
CREATE INDEX IX_DonHang_MaND        ON DonHang(MaND);
CREATE INDEX IX_DonHang_NgayDat     ON DonHang(NgayDat);
CREATE INDEX IX_DonHang_TrangThai   ON DonHang(TrangThai);
CREATE INDEX IX_CT_MaDH             ON ChiTietDonHang(MaDH);
CREATE INDEX IX_CT_MaSP             ON ChiTietDonHang(MaSP);
CREATE INDEX IX_CuaHang_TinhThanh   ON CuaHang(TinhThanh);
GO

/* ===================== BANG BO SUNG: LienLacs, DanhGia ===================== */

-- LienLacs: tin nhan lien he cua khach (khong co khoa ngoai -> khong bi xoa khi chay lai)
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
    PRINT N'Da tao bang LienLacs';
END
ELSE
    PRINT N'Bang LienLacs da ton tai - giu nguyen du lieu';
GO

-- DanhGia: moi khach chi danh gia 1 lan / 1 mon (UNIQUE MaND+MaSP); chi khach da mua + don Hoan thanh moi duoc danh gia (kiem tra o Controller)
IF OBJECT_ID(N'dbo.DanhGia', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DanhGia (
        MaDG      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DanhGia PRIMARY KEY,
        MaSP      INT            NOT NULL,
        MaND      INT            NOT NULL,
        SoSao     INT            NOT NULL,
        NoiDung   NVARCHAR(500)  NULL,
        NgayDanh  DATETIME       NOT NULL CONSTRAINT DF_DanhGia_NgayDanh DEFAULT (GETDATE()),

        CONSTRAINT CK_DanhGia_SoSao CHECK (SoSao BETWEEN 1 AND 5),
        CONSTRAINT UQ_DanhGia_ND_SP UNIQUE (MaND, MaSP),
        CONSTRAINT FK_DanhGia_SanPham   FOREIGN KEY (MaSP) REFERENCES dbo.SanPham(MaSP)    ON DELETE CASCADE,
        CONSTRAINT FK_DanhGia_NguoiDung FOREIGN KEY (MaND) REFERENCES dbo.NguoiDung(MaND)  ON DELETE CASCADE
    );

    CREATE INDEX IX_DanhGia_MaSP ON dbo.DanhGia(MaSP);
    PRINT N'Da tao bang DanhGia';
END
GO

/* ===================== DU LIEU MAU ===================== */

-- LoaiSanPham
SET IDENTITY_INSERT LoaiSanPham ON;
INSERT INTO LoaiSanPham (MaLoai, TenLoai, MoTa, HinhAnh, HienThi, ThuTu) VALUES
(1, N'Trà sữa', N'Trà sữa pha từ trà ủ nóng mỗi ngày', N'/Content/Images/Loai/trasua.jpg', 1, 1),
(2, N'Trà trái cây', N'Trà thanh mát cùng trái cây tươi', N'https://images.unsplash.com/photo-1497534446932-c925b458314e?auto=format&fit=crop&w=600&q=80', 1, 2),
(3, N'Cà phê', N'Cà phê rang xay nguyên chất', N'https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?auto=format&fit=crop&w=600&q=80', 1, 3),
(4, N'Kem & Đá xay', N'Đá xay và kem tươi mát lạnh', N'https://images.unsplash.com/photo-1497034825429-c343d7c6a68f?auto=format&fit=crop&w=600&q=80', 1, 4),
(5, N'Bánh ngọt', N'Bánh ngọt nhà làm ăn kèm thức uống', N'https://images.unsplash.com/photo-1578985545062-69928b1d9587?auto=format&fit=crop&w=600&q=80', 1, 5),
(6, N'Món thêm', N'Topping và món ăn vặt', N'https://images.unsplash.com/photo-1541599468348-e96984315921?auto=format&fit=crop&w=600&q=80', 1, 6);
SET IDENTITY_INSERT LoaiSanPham OFF;

-- SanPham (NgayTao = hom nay - (MaSP+1) ngay)
SET IDENTITY_INSERT SanPham ON;
INSERT INTO SanPham (MaSP, TenSP, MaLoai, Gia, GiaKhuyenMai, HinhAnh, MoTa, SoLuongTon, DangBan, NoiBat, NgayTao, LuotBan) VALUES
(1, N'Trà sữa trân châu đường đen', 1, 45000, 39000, N'https://images.unsplash.com/photo-1558857563-b371033873b8?auto=format&fit=crop&w=800&q=80', N'Trà sữa béo nhẹ kết hợp trân châu đường đen dai mềm, nấu mới mỗi 2 tiếng.', 100, 1, 1, DATEADD(DAY, -2, GETDATE()), 320),
(2, N'Trà sữa hồng trà kem cheese', 1, 49000, NULL, N'/Content/Images/SanPham/hongtrakemcheese.jpg', N'Hồng trà đậm vị phủ lớp kem cheese mặn ngọt hài hòa.', 100, 1, 1, DATEADD(DAY, -3, GETDATE()), 245),
(3, N'Trà sữa khoai môn', 1, 45000, NULL, N'/Content/Images/SanPham/tskhoaimon.jpg', N'Khoai môn nghiền nguyên chất, thơm bùi, thêm trân châu trắng.', 100, 1, 0, DATEADD(DAY, -4, GETDATE()), 180),
(4, N'Trà sữa matcha Nhật Bản', 1, 52000, 45000, N'/Content/Images/SanPham/matchanb.jpg', N'Matcha Uji nguyên chất, vị chát nhẹ đặc trưng, ít ngọt.', 100, 1, 1, DATEADD(DAY, -5, GETDATE()), 210),
(5, N'Trà sữa socola', 1, 47000, NULL, N'/Content/Images/SanPham/socola.jpg', N'Socola Bỉ đậm đà, hợp cho bạn thích vị ngọt sâu.', 100, 1, 0, DATEADD(DAY, -6, GETDATE()), 95),
(6, N'Trà sữa oolong nướng', 1, 49000, NULL, N'/Content/Images/SanPham/olongnuong.jpg', N'Oolong nướng thơm mùi khói nhẹ, hậu vị ngọt thanh.', 100, 1, 0, DATEADD(DAY, -7, GETDATE()), 130),
(7, N'Trà đào cam sả', 2, 45000, 39000, N'/Content/Images/SanPham/tradaocamsa.jpg', N'Đào ngâm giòn, cam tươi và sả thơm — món bán chạy nhất mùa hè.', 100, 1, 1, DATEADD(DAY, -8, GETDATE()), 410),
(8, N'Trà vải hoa hồng', 2, 45000, NULL, N'/Content/Images/SanPham/travaihh.jpg', N'Vải ngọt mọng cùng hương hoa hồng dịu nhẹ.', 100, 1, 1, DATEADD(DAY, -9, GETDATE()), 260),
(9, N'Trà dâu tây tuyết', 2, 52000, NULL, N'/Content/Images/SanPham/tradautuyet.png', N'Dâu Đà Lạt xay cùng trà xanh, phủ tuyết sữa.', 100, 1, 0, DATEADD(DAY, -10, GETDATE()), 155),
(10, N'Trà chanh giã tay', 2, 35000, 29000, N'/Content/Images/SanPham/trachanhgiatay.jpg', N'Chanh tươi giã tay cùng trà xanh, giải nhiệt tức thì.', 100, 1, 0, DATEADD(DAY, -11, GETDATE()), 300),
(11, N'Trà ổi hồng bạc hà', 2, 45000, NULL, N'/Content/Images/SanPham/oihongbacha.jpg', N'Ổi hồng ép tươi thêm lá bạc hà the mát.', 100, 1, 0, DATEADD(DAY, -12, GETDATE()), 120),
(12, N'Cà phê sữa đá', 3, 32000, NULL, N'/Content/Images/SanPham/cfsuada.jpg', N'Robusta rang đậm pha phin, thêm sữa đặc và đá.', 100, 1, 1, DATEADD(DAY, -13, GETDATE()), 380),
(13, N'Bạc xỉu', 3, 35000, NULL, N'/Content/Images/SanPham/bacxiu.jpg', N'Nhiều sữa, ít cà phê, hợp bạn mới tập uống cà phê.', 100, 1, 0, DATEADD(DAY, -14, GETDATE()), 190),
(14, N'Cà phê muối', 3, 39000, 35000, N'/Content/Images/SanPham/cfmuoi.jpg', N'Lớp kem muối béo mặn trên nền cà phê đậm.', 100, 1, 1, DATEADD(DAY, -15, GETDATE()), 275),
(15, N'Latte hạnh nhân', 3, 49000, NULL, N'/Content/Images/SanPham/lattehn.jpg', N'Espresso cùng sữa hạnh nhân, ít đường.', 100, 1, 0, DATEADD(DAY, -16, GETDATE()), 88),
(16, N'Cold brew cam quế', 3, 55000, NULL, N'/Content/Images/SanPham/coldbrew.jpg', N'Cà phê ủ lạnh 18 tiếng, thêm cam và quế.', 100, 1, 0, DATEADD(DAY, -17, GETDATE()), 70),
(17, N'Đá xay socola chip', 4, 59000, 49000, N'/Content/Images/SanPham/sclchip.jpg', N'Socola chip đá xay phủ kem tươi và vụn bánh.', 100, 1, 1, DATEADD(DAY, -18, GETDATE()), 205),
(18, N'Đá xay matcha đậu đỏ', 4, 59000, NULL, N'/Content/Images/SanPham/matchadaudo.jpg', N'Matcha đá xay cùng đậu đỏ ninh mềm.', 100, 1, 0, DATEADD(DAY, -19, GETDATE()), 140),
(19, N'Kem tươi vani', 4, 29000, NULL, N'/Content/Images/SanPham/kemvani.jpg', N'Kem tươi vani Madagascar, ngọt dịu.', 100, 1, 0, DATEADD(DAY, -20, GETDATE()), 175),
(20, N'Kem dâu bạc hà', 4, 32000, NULL, N'/Content/Images/SanPham/daubacha.jpg', N'Hai vị kem dâu và bạc hà trong một ly.', 100, 1, 0, DATEADD(DAY, -21, GETDATE()), 96),
(21, N'Bánh su kem trứng muối', 5, 25000, NULL, N'/Content/Images/SanPham/sukemtrungmuoi.jpg', N'Vỏ su giòn, nhân kem trứng muối béo mặn.', 100, 1, 1, DATEADD(DAY, -22, GETDATE()), 230),
(22, N'Bánh tiramisu', 5, 45000, 39000, N'https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?auto=format&fit=crop&w=800&q=80', N'Tiramisu chuẩn Ý với mascarpone và cacao.', 100, 1, 1, DATEADD(DAY, -23, GETDATE()), 165),
(23, N'Bánh mousse dâu', 5, 42000, NULL, N'/Content/Images/SanPham/moussedau.jpg', N'Mousse dâu mềm mịn, chua ngọt cân bằng.', 100, 1, 0, DATEADD(DAY, -24, GETDATE()), 110),
(24, N'Bánh croissant bơ', 5, 32000, NULL, N'https://images.unsplash.com/photo-1555507036-ab1f4038808a?auto=format&fit=crop&w=800&q=80', N'Croissant nướng mỗi sáng, thơm mùi bơ Pháp.', 100, 1, 0, DATEADD(DAY, -25, GETDATE()), 145),
(25, N'Trân châu đường đen (phần)', 6, 10000, NULL, N'https://images.unsplash.com/photo-1558857563-b371033873b8?auto=format&fit=crop&w=800&q=80', N'Một phần trân châu đường đen nấu mới.', 100, 1, 0, DATEADD(DAY, -26, GETDATE()), 420),
(26, N'Khoai tây lắc phô mai', 6, 35000, NULL, N'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?auto=format&fit=crop&w=800&q=80', N'Khoai tây chiên giòn lắc bột phô mai.', 100, 1, 0, DATEADD(DAY, -27, GETDATE()), 150);
SET IDENTITY_INSERT SanPham OFF;

-- NguoiDung (mat khau de dang van ban thuong nhu code hien tai)
SET IDENTITY_INSERT NguoiDung ON;
INSERT INTO NguoiDung (MaND, HoTen, Email, MatKhau, DienThoai, DiaChi, VaiTro, NgayDangKy) VALUES
(1, N'Chủ shop SugarChills', N'seller@sugarchills.vn', N'123456', N'0901234567', N'12 Nguyễn Văn Bảo, Gò Vấp, TP.HCM', N'Seller', DATEADD(DAY, -30, GETDATE())),
(2, N'Nguyễn Thị Ngọc', N'khach@gmail.com', N'123456', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', N'Customer', DATEADD(DAY, -30, GETDATE())),
(3, N'Trần Minh Khoa', N'khoa.tran@gmail.com', N'123456', N'0987654321', N'203 Quang Trung, Gò Vấp, TP.HCM', N'Customer', DATEADD(DAY, -30, GETDATE()));
SET IDENTITY_INSERT NguoiDung OFF;

-- KhuyenMai (CHILL15 la ma freeship; TRUNGTHU het han; FLASH50 dang tat)
SET IDENTITY_INSERT KhuyenMai ON;
INSERT INTO KhuyenMai (MaKM, TenKM, MaGiamGia, PhanTramGiam, GiamToiDa, DonToiThieu, NgayBatDau, NgayKetThuc, MoTa, KichHoat, LoaiGiam) VALUES
(1, N'Chào bạn mới', N'SUGAR10', 10, 20000, 0, DATEADD(DAY, -60, CAST(GETDATE() AS DATE)), DATEADD(DAY, 60, CAST(GETDATE() AS DATE)), N'Giảm 10% (tối đa 20.000đ) cho mọi đơn hàng.', 1, N'TongTien'),
(2, N'Freeship đơn từ 100k', N'CHILL15', 100, 15000, 100000, DATEADD(DAY, -45, CAST(GETDATE() AS DATE)), DATEADD(DAY, 30, CAST(GETDATE() AS DATE)), N'Miễn phí giao hàng cho đơn từ 100.000đ.', 1, N'PhiGiaoHang'),
(3, N'Happy Hour 14h-17h', N'HAPPY20', 20, 50000, 150000, DATEADD(DAY, -30, CAST(GETDATE() AS DATE)), DATEADD(DAY, 20, CAST(GETDATE() AS DATE)), N'Giảm 20% (tối đa 50.000đ) cho đơn từ 150.000đ.', 1, N'TongTien'),
(4, N'Tết Trung Thu 2025', N'TRUNGTHU', 25, 60000, 200000, DATEADD(DAY, -120, CAST(GETDATE() AS DATE)), DATEADD(DAY, -90, CAST(GETDATE() AS DATE)), N'Chương trình đã kết thúc — dùng để minh họa khuyến mãi hết hạn.', 1, N'TongTien'),
(5, N'Flash sale (tạm tắt)', N'FLASH50', 50, 30000, 0, DATEADD(DAY, -5, CAST(GETDATE() AS DATE)), DATEADD(DAY, 30, CAST(GETDATE() AS DATE)), N'Chưa kích hoạt — dùng để test mã không hiệu lực.', 0, N'TongTien');
SET IDENTITY_INSERT KhuyenMai OFF;

-- CuaHang
SET IDENTITY_INSERT CuaHang ON;
INSERT INTO CuaHang (MaCH, TenCH, DiaChi, TinhThanh, QuanHuyen, SoDienThoai, GioMoCua) VALUES
(1, N'SugarChills Gò Vấp', N'12 Nguyễn Văn Bảo', N'TP. Hồ Chí Minh', N'Gò Vấp', N'1900 6789', N'07:00 - 22:00'),
(2, N'SugarChills Sài Gòn', N'45 Nguyễn Huệ', N'TP. Hồ Chí Minh', N'Sài Gòn', N'1900 6789', N'07:00 - 23:00'),
(3, N'SugarChills Tân Hưng', N'116 Nguyễn Thị Thập', N'TP. Hồ Chí Minh', N'Tân Hưng', N'1900 6789', N'07:00 - 22:00'),
(4, N'SugarChills Tây Thạnh', N'140 Lê Trọng Tấn', N'TP. Hồ Chí Minh', N'Tây Thạnh', N'1900 6789', N'06:30 - 22:00'),
(5, N'SugarChills Vũng Tàu', N'15 Thùy Vân', N'TP. Hồ Chí Minh', N'Vũng Tàu', N'1900 6789', N'07:00 - 22:30'),
(6, N'SugarChills Cầu Giấy', N'25 Trần Duy Hưng', N'Hà Nội', N'Cầu Giấy', N'1900 6789', N'07:30 - 22:00'),
(7, N'SugarChills Hai Bà Trưng', N'88 Bà Triệu', N'Hà Nội', N'Hai Bà Trưng', N'1900 6789', N'07:30 - 22:00'),
(8, N'SugarChills Hải Châu', N'20 Bạch Đằng', N'Đà Nẵng', N'Hải Châu', N'1900 6789', N'07:00 - 22:30'),
(9, N'SugarChills Ngô Quyền', N'10 Điện Biên Phủ', N'Hải Phòng', N'Ngô Quyền', N'1900 6789', N'07:30 - 22:00'),
(10, N'SugarChills Ninh Kiều', N'5 Hòa Bình', N'Cần Thơ', N'Ninh Kiều', N'1900 6789', N'07:00 - 22:00'),
(11, N'SugarChills Nha Trang', N'68 Trần Phú', N'Khánh Hòa', N'Nha Trang', N'1900 6789', N'07:00 - 23:00'),
(12, N'SugarChills Huế', N'30 Lê Lợi', N'Huế', N'Phú Xuân', N'1900 6789', N'07:30 - 22:00'),
(13, N'SugarChills Biên Hòa', N'88 Đồng Khởi', N'Đồng Nai', N'Biên Hòa', N'1900 6789', N'07:00 - 22:00');
SET IDENTITY_INSERT CuaHang OFF;

-- DonHang (40 don trong 30 ngay gan nhat; tien da tinh dung nghiep vu)
SET IDENTITY_INSERT DonHang ON;
INSERT INTO DonHang (MaDH, MaND, TenNguoiNhan, DienThoai, DiaChi, GhiChu, HinhThucThanhToan, NgayDat, TamTinh, TienGiam, PhiGiaoHang, TongTien, MaGiamGiaApDung, TrangThai, DaThanhToan, SoTienConLai) VALUES
(1001, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'COD', DATEADD(HOUR, -10, DATEADD(DAY, -29, GETDATE())), 108000, 0, 15000, 123000, NULL, N'Hoàn thành', 1, 0),
(1002, 3, N'Trần Minh Khoa', N'0987654321', N'203 Quang Trung, Gò Vấp, TP.HCM', NULL, N'COD', DATEADD(HOUR, -4, DATEADD(DAY, -29, GETDATE())), 220000, 0, 0, 220000, NULL, N'Hoàn thành', 1, 0),
(1003, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'QR', DATEADD(HOUR, -10, DATEADD(DAY, -28, GETDATE())), 29000, 0, 15000, 44000, NULL, N'Hoàn thành', 1, 0),
(1004, 3, N'Trần Minh Khoa', N'0987654321', N'203 Quang Trung, Gò Vấp, TP.HCM', NULL, N'COD', DATEADD(HOUR, -8, DATEADD(DAY, -28, GETDATE())), 188000, 0, 0, 188000, NULL, N'Hoàn thành', 1, 0),
(1005, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'COD', DATEADD(HOUR, -2, DATEADD(DAY, -26, GETDATE())), 243000, 0, 0, 243000, NULL, N'Hoàn thành', 1, 0),
(1006, 3, N'Trần Minh Khoa', N'0987654321', N'203 Quang Trung, Gò Vấp, TP.HCM', NULL, N'COD', DATEADD(HOUR, -9, DATEADD(DAY, -25, GETDATE())), 243000, 0, 0, 243000, NULL, N'Hoàn thành', 1, 0),
(1007, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'QR', DATEADD(HOUR, -8, DATEADD(DAY, -25, GETDATE())), 176000, 35200, 0, 140800, N'HAPPY20', N'Hoàn thành', 1, 0),
(1008, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', N'Giao giờ hành chính', N'QR', DATEADD(HOUR, -10, DATEADD(DAY, -24, GETDATE())), 250000, 0, 0, 250000, NULL, N'Hoàn thành', 1, 0),
(1009, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'COD', DATEADD(HOUR, -3, DATEADD(DAY, -24, GETDATE())), 84000, 0, 15000, 99000, NULL, N'Hoàn thành', 1, 0),
(1010, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'COD', DATEADD(HOUR, -3, DATEADD(DAY, -23, GETDATE())), 66000, 6600, 15000, 74400, N'SUGAR10', N'Hoàn thành', 1, 0),
(1011, 3, N'Trần Minh Khoa', N'0987654321', N'203 Quang Trung, Gò Vấp, TP.HCM', N'Gọi trước khi giao', N'COD', DATEADD(HOUR, -1, DATEADD(DAY, -23, GETDATE())), 45000, 0, 15000, 60000, NULL, N'Đã hủy', 0, 60000),
(1012, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'COD', DATEADD(HOUR, -10, DATEADD(DAY, -22, GETDATE())), 96000, 0, 15000, 111000, NULL, N'Đã hủy', 0, 111000),
(1013, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', N'Gọi trước khi giao', N'COD', DATEADD(HOUR, -11, DATEADD(DAY, -21, GETDATE())), 271000, 0, 0, 271000, NULL, N'Hoàn thành', 1, 0),
(1014, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'COD', DATEADD(HOUR, -8, DATEADD(DAY, -21, GETDATE())), 110000, 0, 15000, 125000, NULL, N'Hoàn thành', 1, 0),
(1015, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'COD', DATEADD(HOUR, -10, DATEADD(DAY, -20, GETDATE())), 333000, 0, 0, 333000, NULL, N'Hoàn thành', 1, 0),
(1016, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', N'Không lấy ống hút', N'QR', DATEADD(HOUR, -6, DATEADD(DAY, -17, GETDATE())), 98000, 0, 15000, 113000, NULL, N'Hoàn thành', 1, 0),
(1017, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'COD', DATEADD(HOUR, -3, DATEADD(DAY, -17, GETDATE())), 218000, 43600, 0, 174400, N'HAPPY20', N'Hoàn thành', 1, 0),
(1018, 3, N'Trần Minh Khoa', N'0987654321', N'203 Quang Trung, Gò Vấp, TP.HCM', N'Gọi trước khi giao', N'COD', DATEADD(HOUR, -7, DATEADD(DAY, -16, GETDATE())), 328000, 50000, 0, 278000, N'HAPPY20', N'Hoàn thành', 1, 0),
(1019, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'QR', DATEADD(HOUR, -3, DATEADD(DAY, -15, GETDATE())), 293000, 0, 0, 293000, NULL, N'Hoàn thành', 1, 0),
(1020, 3, N'Trần Minh Khoa', N'0987654321', N'203 Quang Trung, Gò Vấp, TP.HCM', NULL, N'QR', DATEADD(HOUR, -6, DATEADD(DAY, -14, GETDATE())), 167000, 0, 0, 167000, NULL, N'Hoàn thành', 1, 0),
(1021, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'QR', DATEADD(HOUR, -3, DATEADD(DAY, -13, GETDATE())), 10000, 0, 15000, 25000, NULL, N'Hoàn thành', 1, 0),
(1022, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'COD', DATEADD(HOUR, -9, DATEADD(DAY, -12, GETDATE())), 137000, 0, 15000, 152000, NULL, N'Hoàn thành', 1, 0),
(1023, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', N'Giao giờ hành chính', N'COD', DATEADD(HOUR, -2, DATEADD(DAY, -12, GETDATE())), 159000, 0, 0, 159000, NULL, N'Hoàn thành', 1, 0),
(1024, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'COD', DATEADD(HOUR, -9, DATEADD(DAY, -11, GETDATE())), 109000, 0, 15000, 124000, NULL, N'Hoàn thành', 1, 0),
(1025, 3, N'Trần Minh Khoa', N'0987654321', N'203 Quang Trung, Gò Vấp, TP.HCM', NULL, N'COD', DATEADD(HOUR, -8, DATEADD(DAY, -11, GETDATE())), 168000, 16800, 0, 151200, N'SUGAR10', N'Hoàn thành', 1, 0),
(1026, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'QR', DATEADD(HOUR, -6, DATEADD(DAY, -11, GETDATE())), 55000, 0, 15000, 70000, NULL, N'Đã hủy', 1, 0),
(1027, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'COD', DATEADD(HOUR, -8, DATEADD(DAY, -10, GETDATE())), 45000, 0, 15000, 60000, NULL, N'Hoàn thành', 1, 0),
(1028, 3, N'Trần Minh Khoa', N'0987654321', N'203 Quang Trung, Gò Vấp, TP.HCM', N'Gọi trước khi giao', N'QR', DATEADD(HOUR, -8, DATEADD(DAY, -9, GETDATE())), 273000, 50000, 0, 223000, N'HAPPY20', N'Hoàn thành', 1, 0),
(1029, 3, N'Trần Minh Khoa', N'0987654321', N'203 Quang Trung, Gò Vấp, TP.HCM', NULL, N'COD', DATEADD(HOUR, -8, DATEADD(DAY, -7, GETDATE())), 158000, 0, 0, 158000, NULL, N'Hoàn thành', 1, 0),
(1030, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', N'Gọi trước khi giao', N'COD', DATEADD(HOUR, -2, DATEADD(DAY, -6, GETDATE())), 166000, 33200, 0, 132800, N'HAPPY20', N'Hoàn thành', 1, 0),
(1031, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', N'Giao giờ hành chính', N'COD', DATEADD(HOUR, -8, DATEADD(DAY, -4, GETDATE())), 25000, 0, 15000, 40000, NULL, N'Đang giao', 0, 40000),
(1032, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'QR', DATEADD(HOUR, -11, DATEADD(DAY, -3, GETDATE())), 277000, 20000, 0, 257000, N'SUGAR10', N'Đang giao', 1, 0),
(1033, 3, N'Trần Minh Khoa', N'0987654321', N'203 Quang Trung, Gò Vấp, TP.HCM', NULL, N'QR', DATEADD(HOUR, -11, DATEADD(DAY, -2, GETDATE())), 73000, 0, 15000, 88000, NULL, N'Đã xác nhận', 1, 0),
(1034, 3, N'Trần Minh Khoa', N'0987654321', N'203 Quang Trung, Gò Vấp, TP.HCM', NULL, N'COD', DATEADD(HOUR, -11, DATEADD(DAY, -1, GETDATE())), 184000, 18400, 0, 165600, N'SUGAR10', N'Đã hủy', 0, 165600),
(1035, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'QR', DATEADD(HOUR, -1, DATEADD(DAY, -1, GETDATE())), 64000, 0, 15000, 79000, NULL, N'Đang giao', 1, 0),
(1036, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', N'Giao giờ hành chính', N'COD', DATEADD(HOUR, -1, DATEADD(DAY, -1, GETDATE())), 179000, 17900, 0, 161100, N'SUGAR10', N'Đã xác nhận', 0, 161100),
(1037, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'COD', DATEADD(HOUR, -5, DATEADD(DAY, -0, GETDATE())), 292000, 0, 0, 292000, NULL, N'Chờ xác nhận', 0, 292000),
(1038, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', NULL, N'COD', DATEADD(HOUR, -2, DATEADD(DAY, -0, GETDATE())), 82000, 8200, 15000, 88800, N'SUGAR10', N'Chờ xác nhận', 0, 88800),
(1039, 3, N'Trần Minh Khoa', N'0987654321', N'203 Quang Trung, Gò Vấp, TP.HCM', NULL, N'COD', DATEADD(HOUR, -2, DATEADD(DAY, -0, GETDATE())), 105000, 15000, 15000, 105000, N'CHILL15', N'Chờ xác nhận', 0, 105000),
(1040, 2, N'Nguyễn Thị Ngọc', N'0912345678', N'45 Lê Văn Việt, TP. Thủ Đức, TP.HCM', N'Gọi trước khi giao', N'COD', DATEADD(HOUR, -1, DATEADD(DAY, -0, GETDATE())), 188000, 18800, 0, 169200, N'SUGAR10', N'Chờ xác nhận', 0, 169200);
SET IDENTITY_INSERT DonHang OFF;

-- ChiTietDonHang
INSERT INTO ChiTietDonHang (MaDH, MaSP, TenSP, SoLuong, DonGia, TuyChon) VALUES
(1001, 18, N'Đá xay matcha đậu đỏ', 1, 59000, N'Size S | Đường 100% | Đá 30%'),
(1001, 17, N'Đá xay socola chip', 1, 49000, N'Size S | Đường 50% | Đá 30%'),
(1002, 17, N'Đá xay socola chip', 1, 55000, N'Size M | Đường 30% | Đá 70%'),
(1002, 18, N'Đá xay matcha đậu đỏ', 2, 65000, N'Size M | Đường 30% | Đá 50%'),
(1002, 26, N'Khoai tây lắc phô mai', 1, 35000, NULL),
(1003, 10, N'Trà chanh giã tay', 1, 29000, N'Size S | Đường 30% | Đá 100%'),
(1004, 17, N'Đá xay socola chip', 2, 55000, N'Size M | Đường 50% | Đá 30%'),
(1004, 7, N'Trà đào cam sả', 1, 39000, N'Size S | Đường 70% | Đá 100%'),
(1004, 1, N'Trà sữa trân châu đường đen', 1, 39000, N'Size S | Đường 70% | Đá 70%'),
(1005, 5, N'Trà sữa socola', 3, 47000, N'Size S | Đường 70% | Đá 50%'),
(1005, 7, N'Trà đào cam sả', 2, 51000, N'Size L | Đường 50% | Đá 100%'),
(1006, 16, N'Cold brew cam quế', 1, 55000, N'Size S | Đường 50% | Đá 50%'),
(1006, 26, N'Khoai tây lắc phô mai', 1, 35000, NULL),
(1006, 11, N'Trà ổi hồng bạc hà', 3, 51000, N'Size M | Đường 50% | Đá 70%'),
(1007, 13, N'Bạc xỉu', 2, 35000, N'Size S | Đường 100% | Đá 50%'),
(1007, 17, N'Đá xay socola chip', 1, 55000, N'Size M | Đường 30% | Đá 100%'),
(1007, 8, N'Trà vải hoa hồng', 1, 51000, N'Size M | Đường 50% | Đá 30%'),
(1008, 9, N'Trà dâu tây tuyết', 3, 58000, N'Size M | Đường 100% | Đá 70%'),
(1008, 20, N'Kem dâu bạc hà', 2, 38000, N'Size M | Đường 100% | Đá 50%'),
(1009, 20, N'Kem dâu bạc hà', 2, 32000, N'Size S | Đường 70% | Đá 30%'),
(1009, 25, N'Trân châu đường đen (phần)', 2, 10000, NULL),
(1010, 21, N'Bánh su kem trứng muối', 1, 25000, NULL),
(1010, 13, N'Bạc xỉu', 1, 41000, N'Size M | Đường 70% | Đá 70%'),
(1011, 8, N'Trà vải hoa hồng', 1, 45000, N'Size S | Đường 30% | Đá 30%'),
(1012, 24, N'Bánh croissant bơ', 1, 32000, NULL),
(1012, 9, N'Trà dâu tây tuyết', 1, 64000, N'Size L | Đường 100% | Đá 100%'),
(1013, 24, N'Bánh croissant bơ', 3, 32000, NULL),
(1013, 25, N'Trân châu đường đen (phần)', 1, 10000, NULL),
(1013, 6, N'Trà sữa oolong nướng', 3, 55000, N'Size M | Đường 30% | Đá 100%'),
(1014, 17, N'Đá xay socola chip', 1, 55000, N'Size M | Đường 70% | Đá 50%'),
(1014, 6, N'Trà sữa oolong nướng', 1, 55000, N'Size M | Đường 100% | Đá 100%'),
(1015, 9, N'Trà dâu tây tuyết', 3, 58000, N'Size M | Đường 70% | Đá 70%'),
(1015, 4, N'Trà sữa matcha Nhật Bản', 3, 53000, N'Size S | Đường 100% | Đá 30% | Topping: Trân châu đen'),
(1016, 6, N'Trà sữa oolong nướng', 2, 49000, N'Size S | Đường 70% | Đá 100%'),
(1017, 4, N'Trà sữa matcha Nhật Bản', 1, 73000, N'Size L | Đường 30% | Đá 70% | Topping: Trân châu trắng, Trân châu đen'),
(1017, 23, N'Bánh mousse dâu', 2, 42000, NULL),
(1017, 6, N'Trà sữa oolong nướng', 1, 61000, N'Size L | Đường 30% | Đá 50%'),
(1018, 10, N'Trà chanh giã tay', 3, 29000, N'Size S | Đường 70% | Đá 50%'),
(1018, 9, N'Trà dâu tây tuyết', 3, 58000, N'Size M | Đường 70% | Đá 50%'),
(1018, 16, N'Cold brew cam quế', 1, 67000, N'Size L | Đường 50% | Đá 100%'),
(1019, 3, N'Trà sữa khoai môn', 3, 57000, N'Size L | Đường 100% | Đá 100%'),
(1019, 16, N'Cold brew cam quế', 2, 61000, N'Size M | Đường 100% | Đá 50%'),
(1020, 2, N'Trà sữa hồng trà kem cheese', 2, 61000, N'Size L | Đường 100% | Đá 100%'),
(1020, 1, N'Trà sữa trân châu đường đen', 1, 45000, N'Size M | Đường 30% | Đá 100%'),
(1021, 25, N'Trân châu đường đen (phần)', 1, 10000, NULL),
(1022, 11, N'Trà ổi hồng bạc hà', 2, 51000, N'Size M | Đường 50% | Đá 100%'),
(1022, 26, N'Khoai tây lắc phô mai', 1, 35000, NULL),
(1023, 5, N'Trà sữa socola', 3, 53000, N'Size M | Đường 70% | Đá 70%'),
(1024, 19, N'Kem tươi vani', 2, 35000, N'Size M | Đường 30% | Đá 50%'),
(1024, 7, N'Trà đào cam sả', 1, 39000, N'Size S | Đường 30% | Đá 50%'),
(1025, 1, N'Trà sữa trân châu đường đen', 1, 51000, N'Size L | Đường 100% | Đá 70%'),
(1025, 22, N'Bánh tiramisu', 3, 39000, NULL),
(1026, 17, N'Đá xay socola chip', 1, 55000, N'Size M | Đường 50% | Đá 70%'),
(1027, 3, N'Trà sữa khoai môn', 1, 45000, N'Size S | Đường 50% | Đá 50%'),
(1028, 11, N'Trà ổi hồng bạc hà', 2, 45000, N'Size S | Đường 30% | Đá 50%'),
(1028, 2, N'Trà sữa hồng trà kem cheese', 3, 61000, N'Size L | Đường 100% | Đá 70%'),
(1029, 22, N'Bánh tiramisu', 2, 39000, NULL),
(1029, 23, N'Bánh mousse dâu', 1, 42000, NULL),
(1029, 20, N'Kem dâu bạc hà', 1, 38000, N'Size M | Đường 30% | Đá 30%'),
(1030, 12, N'Cà phê sữa đá', 2, 38000, N'Size M | Đường 100% | Đá 100%'),
(1030, 10, N'Trà chanh giã tay', 1, 41000, N'Size L | Đường 50% | Đá 30%'),
(1030, 15, N'Latte hạnh nhân', 1, 49000, N'Size S | Đường 100% | Đá 70%'),
(1031, 21, N'Bánh su kem trứng muối', 1, 25000, NULL),
(1032, 14, N'Cà phê muối', 2, 47000, N'Size L | Đường 100% | Đá 50%'),
(1032, 2, N'Trà sữa hồng trà kem cheese', 3, 61000, N'Size L | Đường 30% | Đá 50%'),
(1033, 12, N'Cà phê sữa đá', 1, 38000, N'Size M | Đường 50% | Đá 30%'),
(1033, 26, N'Khoai tây lắc phô mai', 1, 35000, NULL),
(1034, 3, N'Trà sữa khoai môn', 2, 53000, N'Size S | Đường 100% | Đá 100% | Topping: Trân châu trắng'),
(1034, 22, N'Bánh tiramisu', 2, 39000, NULL),
(1035, 12, N'Cà phê sữa đá', 2, 32000, N'Size S | Đường 100% | Đá 70%'),
(1036, 18, N'Đá xay matcha đậu đỏ', 1, 65000, N'Size M | Đường 100% | Đá 30%'),
(1036, 8, N'Trà vải hoa hồng', 2, 57000, N'Size L | Đường 100% | Đá 70%'),
(1037, 24, N'Bánh croissant bơ', 1, 32000, NULL),
(1037, 2, N'Trà sữa hồng trà kem cheese', 2, 61000, N'Size L | Đường 70% | Đá 100%'),
(1037, 1, N'Trà sữa trân châu đường đen', 2, 69000, N'Size L | Đường 70% | Đá 30% | Topping: Trân châu đen, Pudding trứng'),
(1038, 13, N'Bạc xỉu', 2, 41000, N'Size M | Đường 30% | Đá 70%'),
(1039, 10, N'Trà chanh giã tay', 3, 35000, N'Size M | Đường 30% | Đá 100%'),
(1040, 18, N'Đá xay matcha đậu đỏ', 2, 59000, N'Size S | Đường 30% | Đá 50%'),
(1040, 19, N'Kem tươi vani', 2, 35000, N'Size M | Đường 100% | Đá 50%');
GO

/* ===================== KIEM TRA ===================== */
SELECT N'LoaiSanPham' AS Bang, COUNT(*) AS SoDong FROM LoaiSanPham UNION ALL
SELECT N'SanPham', COUNT(*) FROM SanPham UNION ALL
SELECT N'NguoiDung', COUNT(*) FROM NguoiDung UNION ALL
SELECT N'KhuyenMai', COUNT(*) FROM KhuyenMai UNION ALL
SELECT N'DonHang', COUNT(*) FROM DonHang UNION ALL
SELECT N'ChiTietDonHang', COUNT(*) FROM ChiTietDonHang UNION ALL
SELECT N'CuaHang', COUNT(*) FROM CuaHang UNION ALL
SELECT N'LienLacs', COUNT(*) FROM LienLacs UNION ALL
SELECT N'DanhGia', COUNT(*) FROM DanhGia;

-- (phai rong) TamTinh khac tong chi tiet
SELECT d.MaDH, d.TamTinh, SUM(c.SoLuong * c.DonGia) AS TongChiTiet
FROM DonHang d JOIN ChiTietDonHang c ON c.MaDH = d.MaDH
GROUP BY d.MaDH, d.TamTinh
HAVING d.TamTinh <> SUM(c.SoLuong * c.DonGia);

-- (phai rong) phi giao hang / tong tien sai quy tac
SELECT MaDH, TamTinh, PhiGiaoHang, TongTien FROM DonHang
WHERE (TamTinh >= 150000 AND PhiGiaoHang <> 0)
   OR (TamTinh <  150000 AND PhiGiaoHang <> 15000)
   OR TongTien <> TamTinh - TienGiam + PhiGiaoHang;

-- Doanh thu 30 ngay (chi tinh don Hoan thanh - giong ThongKeController)
SELECT COUNT(*) AS SoDonHoanThanh, SUM(TongTien) AS DoanhThu
FROM DonHang WHERE TrangThai = N'Hoàn thành';
