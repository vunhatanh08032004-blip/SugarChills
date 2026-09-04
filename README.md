# SugarChills — Website quản lý mặt hàng ăn uống

> Đồ án môn **Lập trình Web** — Website bán và quản lý đồ ăn/thức uống (trà sữa, cà phê, kem, bánh ngọt)
> xây dựng bằng **ASP.NET MVC 5 (.NET Framework 4.7.2)**, C#, Razor và Bootstrap 5.
>
> Tên solution/project: `QL_MatHangAnUong` — Tên thương hiệu hiển thị trên web: **SugarChills**.

---

## Mục lục

1. [Giới thiệu](#1-giới-thiệu)
2. [Công nghệ sử dụng](#2-công-nghệ-sử-dụng)
3. [Yêu cầu môi trường](#3-yêu-cầu-môi-trường)
4. [Cách chạy project](#4-cách-chạy-project)
5. [Tài khoản dùng thử](#5-tài-khoản-dùng-thử)
6. [Cấu trúc thư mục](#6-cấu-trúc-thư-mục)
7. [Kiến trúc & luồng xử lý](#7-kiến-trúc--luồng-xử-lý)
8. [Mô hình dữ liệu](#8-mô-hình-dữ-liệu)
9. [Bảng chức năng & định tuyến (URL)](#9-bảng-chức-năng--định-tuyến-url)
10. [Nghiệp vụ chi tiết](#10-nghiệp-vụ-chi-tiết)
11. [Validation & ràng buộc dữ liệu](#11-validation--ràng-buộc-dữ-liệu)
12. [Phân quyền & bảo mật](#12-phân-quyền--bảo-mật)
13. [Giao diện (View & tài nguyên tĩnh)](#13-giao-diện-view--tài-nguyên-tĩnh)
14. [Đối chiếu thang điểm (rubric)](#14-đối-chiếu-thang-điểm-rubric)
15. [Lộ trình chuyển sang Entity Framework 6 + SQL Server](#15-lộ-trình-chuyển-sang-entity-framework-6--sql-server)
16. [Hạn chế đã biết & hướng phát triển](#16-hạn-chế-đã-biết--hướng-phát-triển)
17. [Quy ước Git](#17-quy-ước-git)
18. [Xử lý sự cố thường gặp](#18-xử-lý-sự-cố-thường-gặp)
19. [Nhóm thực hiện](#19-nhóm-thực-hiện)

---

## 1. Giới thiệu

SugarChills mô phỏng đầy đủ quy trình mua bán trực tuyến của một quán đồ uống:

```
Người bán quản lý sản phẩm → Khách tìm kiếm/lọc → Xem chi tiết → Chọn tùy chọn (size, đường, đá, topping)
→ Thêm vào giỏ → Nhập mã giảm giá → Đặt hàng → Người bán xử lý đơn → Cập nhật trạng thái → Thống kê doanh thu
```

Hệ thống phục vụ **3 nhóm người dùng**, nhưng chỉ có **2 vai trò tài khoản** trong CSDL
(`NguoiDung.VaiTro` = `"Customer"` hoặc `"Seller"` — xem `Models/NguoiDung.cs`):

| Nhóm | Là role? | Làm được gì |
|---|---|---|
| **Khách vãng lai (Guest)** | ❌ Không — chỉ là người **chưa đăng nhập** | Xem trang chủ, thực đơn, tìm kiếm, lọc theo loại, xem chi tiết, xem sản phẩm liên quan, **thêm vào giỏ hàng** |
| **Khách hàng (Customer)** | ✅ `"Customer"` | Toàn bộ quyền của Guest + **đặt hàng**, xem/theo dõi đơn của mình, tự hủy đơn khi còn "Chờ xác nhận" |
| **Người bán (Seller)** | ✅ `"Seller"` | Quản lý sản phẩm, loại, đơn hàng, khuyến mãi và xem thống kê doanh thu |

> **Điểm đáng chú ý khi bảo vệ:** giỏ hàng lưu trong **Session** nên khách vãng lai vẫn chọn món được;
> chỉ khi bấm **Đặt hàng** (`/GioHang/ThanhToan`) mới bị filter `[KiemTraDangNhap]` yêu cầu đăng nhập,
> và sau khi đăng nhập sẽ được đưa **quay lại đúng trang đang dở** nhờ tham số `returnUrl`.

---

## 2. Công nghệ sử dụng

| Thành phần | Phiên bản | Ghi chú |
|---|---|---|
| .NET Framework | **4.7.2** | ASP.NET Web Application (không phải .NET Core) |
| ASP.NET MVC | **5.2.9** | `Microsoft.AspNet.Mvc` |
| Razor View Engine | **3.2.9** | Toàn bộ View là `.cshtml` |
| ASP.NET Web Pages | 3.2.9 | |
| Web Optimization (Bundling) | 1.1.3 | Gom/nén CSS-JS trong `BundleConfig.cs` |
| Bootstrap | **5.2.3** | Dùng bản `bootstrap.bundle.js` (đã kèm Popper) |
| jQuery | **3.7.0** | |
| jQuery Validation + Unobtrusive | 1.19.5 / 3.2.11 | Validation phía client |
| Newtonsoft.Json | 13.0.3 | |
| Modernizr | 2.8.3 | |
| Ngôn ngữ | C# | LINQ dùng xuyên suốt |

**Chưa cài trong `packages.config` (dự kiến giai đoạn sau):** Entity Framework 6 và SQL Server —
xem [mục 15](#15-lộ-trình-chuyển-sang-entity-framework-6--sql-server).

Ở giai đoạn hiện tại, dữ liệu nằm trong lớp `Models/KhoDuLieu.cs` (**in-memory**), được thiết kế
để **thay ruột bằng `DbContext` mà không phải sửa Controller/View**.

---

## 3. Yêu cầu môi trường

* **Windows** + **Visual Studio 2019 / 2022** (workload *ASP.NET and web development*).
* **.NET Framework 4.7.2 Developer Pack**.
* **IIS Express** (đi kèm Visual Studio).
* Không cần cài SQL Server ở giai đoạn hiện tại.

---

## 4. Cách chạy project

```text
1. Mở file  QL_MatHangAnUong.slnx  bằng Visual Studio.
2. (Nếu Solution Explorer thiếu file mới) Chuột phải project → Unload Project → Reload Project.
3. (Nếu thiếu thư viện) Chuột phải Solution → Restore NuGet Packages.
4. Nhấn F5 (hoặc Ctrl+F5) → trình duyệt tự mở trang chủ SugarChills.
```

Không cần cấu hình connection string, không cần tạo database.

> **Lưu ý:** dữ liệu nằm trong RAM (`KhoDuLieu`), nên **mỗi lần khởi động lại project thì sản phẩm,
> đơn hàng, tài khoản vừa thêm sẽ trở về dữ liệu mẫu ban đầu**. Đây là hành vi mong muốn ở giai đoạn
> dựng giao diện, không phải lỗi.

---

## 5. Tài khoản dùng thử

Dữ liệu mẫu được tạo trong `KhoDuLieu.TaoDuLieuMau()`:

| Vai trò | Email | Mật khẩu | Đăng nhập xong vào đâu |
|---|---|---|---|
| **Người bán (Seller)** | `seller@sugarchills.vn` | `123456` | Vào thẳng khu quản trị `/ThongKe` |
| **Khách hàng** | `khach@gmail.com` | `123456` | Trang chủ |
| **Khách hàng** | `khoa.tran@gmail.com` | `123456` | Trang chủ |

**Mã giảm giá mẫu** (dùng ở giỏ hàng):

| Mã | Giảm | Giảm tối đa | Đơn tối thiểu |
|---|---|---|---|
| `SUGAR10` | 10% | 20.000đ | 0đ |
| `CHILL15` | 15% | 30.000đ | 100.000đ |
| `HAPPY20` | 20% | 50.000đ | 150.000đ |
| `TRUNGTHU` | 25% | 60.000đ | 200.000đ |

Dữ liệu mẫu còn có **6 loại sản phẩm** (Trà sữa, Trà trái cây, Cà phê, Kem & Đá xay, Bánh ngọt, Món thêm)
và **27 sản phẩm**.

---

## 6. Cấu trúc thư mục

```
QL_MatHangAnUong/                      ← thư mục solution
├── QL_MatHangAnUong.slnx
├── HUONG_DAN_SUGARCHILLS.md           ← ghi chú khung giao diện giai đoạn 1
├── README.md                          ← file bạn đang đọc
└── QL_MatHangAnUong/                  ← project ASP.NET MVC 5
    │
    ├── App_Start/
    │   ├── RouteConfig.cs             Route mặc định {controller}/{action}/{id} → Home/Index
    │   ├── BundleConfig.cs            Bundle jquery, jqueryval, bootstrap, sugarchills, ~/Content/css
    │   └── FilterConfig.cs            Đăng ký HandleErrorAttribute toàn cục
    │
    ├── Models/                        ← M trong MVC
    │   ├── LoaiSanPham.cs             Entity loại sản phẩm
    │   ├── SanPham.cs                 Entity sản phẩm
    │   ├── NguoiDung.cs               Entity tài khoản (Customer / Seller)
    │   ├── DonHang.cs                 Entity đơn hàng + hằng số trạng thái
    │   ├── ChiTietDonHang.cs          Entity dòng chi tiết đơn
    │   ├── KhuyenMai.cs               Entity khuyến mãi + hàm TinhTienGiam()
    │   ├── GioHang.cs                 GioHangItem, GioHang (Session) và bảng phụ thu TuyChonSanPham
    │   ├── KhoDuLieu.cs               ⭐ Kho dữ liệu tạm (thay bằng DbContext ở giai đoạn 2)
    │   └── ViewModels/ViewModels.cs   ViewModel cho từng màn hình
    │
    ├── Controllers/                   ← C trong MVC
    │   ├── BaseController.cs          Controller cha: nạp menu, số lượng giỏ, người dùng cho _Layout
    │   ├── HomeController.cs          Trang chủ, Giới thiệu, Liên hệ
    │   ├── SanPhamController.cs       Thực đơn: danh sách, tìm kiếm, lọc, chi tiết, gợi ý AJAX
    │   ├── GioHangController.cs       Giỏ hàng, mã giảm giá, đặt hàng
    │   ├── DonHangController.cs       Đơn hàng phía khách
    │   ├── TaiKhoanController.cs      Đăng ký / Đăng nhập / Đăng xuất / Thông tin
    │   ├── QuanLySanPhamController.cs [Seller] CRUD sản phẩm
    │   ├── QuanLyLoaiController.cs    [Seller] CRUD loại
    │   ├── QuanLyDonHangController.cs [Seller] xử lý & cập nhật trạng thái đơn
    │   ├── QuanLyKhuyenMaiController.cs [Seller] CRUD khuyến mãi
    │   └── ThongKeController.cs       [Seller] thống kê doanh thu (trang chủ khu quản trị)
    │
    ├── Filters/
    │   └── PhanQuyenAttribute.cs      [KiemTraDangNhap] và [KiemTraNguoiBan]
    │
    ├── Helpers/
    │   ├── PhienLamViec.cs            Gom mọi thao tác Session (người dùng, giỏ hàng)
    │   └── DinhDang.cs                Định dạng tiền tệ / ngày giờ kiểu Việt Nam
    │
    ├── Views/                         ← V trong MVC
    │   ├── Shared/_Layout.cshtml          Layout khách hàng (menu 2 cấp)
    │   ├── Shared/_LayoutSeller.cshtml    Layout khu quản trị (sidebar)
    │   ├── Shared/_CardSanPham.cshtml     Partial card sản phẩm dùng lại nhiều nơi
    │   ├── Shared/Error.cshtml
    │   ├── Home/         Index, GioiThieu, LienHe (+ About/Contact cũ của template, không còn dùng)
    │   ├── SanPham/      Index, ChiTiet
    │   ├── GioHang/      Index, ThanhToan, DatHangThanhCong
    │   ├── DonHang/      Index, ChiTiet
    │   ├── TaiKhoan/     DangKy, DangNhap, ThongTin, KhongCoQuyen
    │   ├── QuanLySanPham/  Index, Them, Sua, Xoa, ChiTiet, _FormSanPham
    │   ├── QuanLyLoai/     Index, Them, Sua, Xoa, _FormLoai
    │   ├── QuanLyDonHang/  Index, ChiTiet
    │   ├── QuanLyKhuyenMai/ Index, Them, Sua, Xoa, _FormKhuyenMai
    │   └── ThongKe/        Index
    │
    ├── Content/
    │   ├── sugarchills.css            ⭐ Toàn bộ thiết kế pastel hồng/kem
    │   ├── Site.css                   (đã rút gọn)
    │   └── bootstrap*.css
    ├── Scripts/
    │   ├── sugarchills.js             ⭐ Thêm giỏ AJAX, gợi ý tìm kiếm, tăng/giảm số lượng, tính giá theo tùy chọn
    │   └── jquery*, bootstrap*, modernizr*
    ├── Global.asax(.cs)               Điểm khởi động: đăng ký Route/Bundle/Filter
    ├── Web.config                     Cấu hình ứng dụng (chưa có connectionStrings)
    └── packages.config                Danh sách gói NuGet
```

---

## 7. Kiến trúc & luồng xử lý

### 7.1. Phân tách MVC

* **Model** — chỉ chứa dữ liệu + quy tắc gắn liền với dữ liệu
  (ví dụ `SanPham.GiaBanThucTe`, `KhuyenMai.TinhTienGiam()`, `GioHang.Them()`).
* **View** — chỉ hiển thị. Mọi phép đếm, lọc, gom nhóm đều làm ở Controller rồi đẩy sang View
  (ví dụ `ViewBag.SoSanPhamTheoLoai` được tính sẵn trong `HomeController` / `SanPhamController`).
* **Controller** — nhận request, gọi `KhoDuLieu`, kiểm tra nghiệp vụ, chọn View.

### 7.2. `BaseController` — dữ liệu dùng chung cho `_Layout`

Mọi controller đều kế thừa `BaseController`. Trong `OnActionExecuting`, nó nạp sẵn:

| ViewBag | Nội dung | Dùng ở đâu |
|---|---|---|
| `ViewBag.NguoiDungHienTai` | Người đang đăng nhập (null nếu là khách vãng lai) | Header: hiện tên / nút đăng nhập |
| `ViewBag.MenuLoai` | Danh sách loại có `HienThi = true` | Menu 2 cấp "Thực đơn" |
| `ViewBag.SoLuongGioHang` | Tổng số món trong giỏ | Badge trên icon giỏ hàng |

`BaseController` còn cung cấp hàm `ThongBao(noiDung, loai)` để đặt thông báo flash một lần
qua `TempData["ThongBao"]` / `TempData["LoaiThongBao"]` (`success | danger | warning | info`).

### 7.3. Trạng thái phiên làm việc

Toàn bộ thao tác Session được gom vào `Helpers/PhienLamViec.cs` để không rải chuỗi khóa khắp code:

| Khóa Session | Kiểu | Ý nghĩa |
|---|---|---|
| `NguoiDungDangNhap` | `NguoiDung` | Người dùng đang đăng nhập |
| `GioHang` | `GioHang` | Giỏ hàng hiện tại |

Đăng xuất (`PhienLamViec.DangXuat`) xóa **cả hai** khóa.

### 7.4. Luồng một request tiêu biểu

```
Trình duyệt
   │  GET /SanPham/ChiTiet/5
   ▼
RouteConfig  →  SanPhamController.ChiTiet(5)
   │                 │
   │                 ├─ BaseController.OnActionExecuting → nạp menu, giỏ, người dùng
   │                 ├─ KhoDuLieu.LaySanPham(5)
   │                 ├─ KhoDuLieu.LaySanPhamLienQuan(5, maLoai, 4)
   │                 └─ trả ChiTietSanPhamViewModel
   ▼
Views/SanPham/ChiTiet.cshtml  (+ _Layout, _CardSanPham)  →  HTML
```

---

## 8. Mô hình dữ liệu

### 8.1. Sơ đồ quan hệ

```
   NguoiDung                     LoaiSanPham
   ─────────                     ───────────
   MaND (PK)                     MaLoai (PK)
   HoTen, Email, MatKhau              │ 1
   DienThoai, DiaChi                  │
   VaiTro (Customer|Seller)           │ n
   NgayDangKy                     SanPham
        │ 1                       ───────
        │                         MaSP (PK)
        │ n                       MaLoai (FK → LoaiSanPham)
    DonHang                       TenSP, Gia, GiaKhuyenMai
    ───────                       HinhAnh, MoTa, SoLuongTon
    MaDH (PK)                     DangBan, NoiBat, NgayTao, LuotBan
    MaND (FK, nullable)               │ 1
    TenNguoiNhan, DienThoai, DiaChi   │
    GhiChu, HinhThucThanhToan         │ n
    NgayDat, TrangThai            ChiTietDonHang
    TamTinh, TienGiam,            ──────────────
    PhiGiaoHang, TongTien         MaCT (PK)
    MaGiamGiaApDung ─ ─ ─ ─ ─ ─ ▶ MaDH (FK → DonHang)
        │ 1                       MaSP (FK → SanPham)
        └──────────── n ────────▶ TenSP, SoLuong, DonGia, TuyChon

   KhuyenMai (độc lập, đối chiếu qua chuỗi MaGiamGia)
   ─────────
   MaKM (PK), TenKM, MaGiamGia (duy nhất)
   PhanTramGiam, GiamToiDa, DonToiThieu
   NgayBatDau, NgayKetThuc, MoTa, KichHoat
```

**Quan hệ:**

* `LoaiSanPham 1 — n SanPham` (`SanPham.MaLoai`)
* `NguoiDung 1 — n DonHang` (`DonHang.MaND`, cho phép null)
* `DonHang 1 — n ChiTietDonHang`
* `SanPham 1 — n ChiTietDonHang`
* `KhuyenMai` không có khóa ngoại: đơn hàng chỉ **lưu lại chuỗi mã** đã dùng (`MaGiamGiaApDung`)
  để giữ nguyên lịch sử kể cả khi chương trình khuyến mãi bị xóa/sửa sau này.

### 8.2. Trường tính toán (`[NotMapped]`)

Các thuộc tính này **không lưu xuống database**, chỉ tính trong bộ nhớ:

| Lớp | Thuộc tính | Công thức |
|---|---|---|
| `SanPham` | `GiaBanThucTe` | Có `GiaKhuyenMai > 0` thì lấy giá khuyến mãi, ngược lại lấy `Gia` |
| `SanPham` | `PhanTramGiam` | `(1 − GiaKhuyenMai / Gia) × 100`, làm tròn — dùng cho nhãn "−20%" |
| `ChiTietDonHang` | `ThanhTien` | `DonGia × SoLuong` |
| `DonHang` | `CssTrangThai` | Trả lớp badge Bootstrap theo trạng thái |
| `KhuyenMai` | `ConHieuLuc` | `KichHoat` **và** hôm nay nằm trong `[NgayBatDau, NgayKetThuc]` |

> ⚠️ **Khi chuyển sang EF6:** các thuộc tính `[NotMapped]` **không được dùng trực tiếp trong câu LINQ
> gửi xuống SQL**. Phải `ToList()` trước rồi mới lọc, hoặc viết điều kiện trên cột thật (`Gia`, `GiaKhuyenMai`).

### 8.3. Trạng thái đơn hàng

Khai báo dưới dạng hằng trong `DonHang.cs`, dropdown lấy từ mảng `DonHang.CacTrangThai`:

```
Chờ xác nhận → Đã xác nhận → Đang giao → Hoàn thành
      │
      └──────────────────────────────► Đã hủy
```

Quy tắc: đơn đã ở **Hoàn thành** hoặc **Đã hủy** thì không đổi trạng thái được nữa.

---

## 9. Bảng chức năng & định tuyến (URL)

Route mặc định: `{controller}/{action}/{id}` → mặc định `Home/Index`.

### 9.1. Khu vực công khai (Guest xem được)

| URL | Method | Action | Chức năng |
|---|---|---|---|
| `/` hoặc `/Home/Index` | GET | `HomeController.Index` | Trang chủ: loại sản phẩm, SP nổi bật (8), SP mới (4), SP giảm giá (4), 3 khuyến mãi đang chạy |
| `/Home/GioiThieu` | GET | `HomeController.GioiThieu` | Trang giới thiệu |
| `/Home/LienHe` | GET | `HomeController.LienHe` | Trang liên hệ |
| `/SanPham` | GET | `SanPhamController.Index` | Thực đơn + **tìm kiếm** + **lọc theo loại** + lọc theo giá + sắp xếp + phân trang (9 SP/trang) |
| `/SanPham/ChiTiet/{id}` | GET | `SanPhamController.ChiTiet` | Chi tiết sản phẩm + **sản phẩm liên quan** (cùng loại, 4 món) |
| `/SanPham/GoiY?tuKhoa=` | GET (AJAX) | `SanPhamController.GoiY` | Trả JSON tối đa 6 gợi ý cho ô tìm kiếm ở header (yêu cầu từ khóa ≥ 2 ký tự) |

**Tham số lọc của `/SanPham`:**

```
/SanPham?tuKhoa=tra&maLoai=1&giaTu=30000&giaDen=50000&sapXep=gia-tang&trang=2
```

| Tham số | Ý nghĩa |
|---|---|
| `tuKhoa` | Tìm trong **tên** và **mô tả** sản phẩm (không phân biệt hoa/thường) |
| `maLoai` | Lọc theo loại sản phẩm |
| `giaTu`, `giaDen` | Lọc theo khoảng giá (tính trên `GiaBanThucTe`) |
| `sapXep` | `gia-tang` \| `gia-giam` \| `ten` \| `ban-chay` \| *(mặc định: nổi bật trước, rồi lượt bán)* |
| `trang` | Trang hiện tại, tự kẹp về `[1, TongSoTrang]` |

### 9.2. Tài khoản

| URL | Method | Quyền | Chức năng |
|---|---|---|---|
| `/TaiKhoan/DangKy` | GET/POST | Public | Đăng ký; đăng ký xong **tự đăng nhập** luôn |
| `/TaiKhoan/DangNhap` | GET/POST | Public | Đăng nhập; hỗ trợ `returnUrl`; Seller được đưa thẳng vào `/ThongKe` |
| `/TaiKhoan/DangXuat` | POST | Đã đăng nhập | Xóa Session người dùng **và** giỏ hàng |
| `/TaiKhoan/ThongTin` | GET | `[KiemTraDangNhap]` | Thông tin cá nhân + danh sách đơn đã đặt |
| `/TaiKhoan/KhongCoQuyen` | GET | Public | Trang báo "không có quyền truy cập" |

### 9.3. Giỏ hàng & đặt hàng

| URL | Method | Quyền | Chức năng |
|---|---|---|---|
| `/GioHang` | GET | Public | Xem giỏ, tạm tính, tiền giảm, phí giao hàng, tổng tiền |
| `/GioHang/Them` | POST | Public | Thêm từ trang chi tiết **kèm tùy chọn** size/đường/đá/topping |
| `/GioHang/ThemNhanh` | POST (AJAX) | Public | Nút "Thêm vào giỏ" trên card sản phẩm — mặc định size M, 100% đường/đá |
| `/GioHang/CapNhat` | POST (AJAX) | Public | Tăng/giảm/nhập lại số lượng một dòng; nhập 0 thì xóa dòng |
| `/GioHang/Xoa` | POST | Public | Xóa một dòng giỏ hàng |
| `/GioHang/XoaTatCa` | POST | Public | Xóa toàn bộ giỏ |
| `/GioHang/ApDungMa` | POST | Public | Nhập mã giảm giá |
| `/GioHang/BoMa` | POST | Public | Gỡ mã đã áp dụng |
| `/GioHang/ThanhToan` | GET/POST | `[KiemTraDangNhap]` | Form đặt hàng (điền sẵn tên/điện thoại/địa chỉ từ tài khoản) → tạo đơn |
| `/GioHang/DatHangThanhCong/{id}` | GET | `[KiemTraDangNhap]` | Trang xác nhận đặt hàng thành công |

### 9.4. Đơn hàng của khách

| URL | Method | Quyền | Chức năng |
|---|---|---|---|
| `/DonHang` | GET | `[KiemTraDangNhap]` | Danh sách đơn của **chính mình**, lọc theo trạng thái |
| `/DonHang/ChiTiet/{id}` | GET | `[KiemTraDangNhap]` | Chi tiết đơn — chặn xem đơn của người khác (`HttpUnauthorizedResult`) |
| `/DonHang/HuyDon` | POST | `[KiemTraDangNhap]` | Tự hủy đơn khi còn **"Chờ xác nhận"** |

### 9.5. Khu quản trị — tất cả đều `[KiemTraNguoiBan]`

| URL | Method | Chức năng |
|---|---|---|
| `/ThongKe` | GET | **Trang chủ khu quản trị**: doanh thu theo khoảng ngày, biểu đồ theo ngày, theo loại, top 5 bán chạy, 5 đơn mới nhất |
| `/QuanLySanPham` | GET | Danh sách SP + tìm theo tên + lọc theo loại + lọc `dang-ban`/`ngung-ban` |
| `/QuanLySanPham/ChiTiet/{id}` | GET | Xem chi tiết |
| `/QuanLySanPham/Them` | GET/POST | Thêm sản phẩm |
| `/QuanLySanPham/Sua/{id}` | GET/POST | Sửa sản phẩm |
| `/QuanLySanPham/Xoa/{id}` | GET | **Trang xác nhận** trước khi xóa |
| `/QuanLySanPham/XacNhanXoa/{id}` | POST | Thực hiện xóa (có kiểm tra ràng buộc) |
| `/QuanLyLoai` | GET | Danh sách loại + số sản phẩm mỗi loại |
| `/QuanLyLoai/Them`, `/Sua/{id}` | GET/POST | Thêm / sửa loại |
| `/QuanLyLoai/Xoa/{id}` → `/XacNhanXoa/{id}` | GET → POST | Xóa loại (chặn nếu còn sản phẩm) |
| `/QuanLyDonHang` | GET | Danh sách đơn + lọc trạng thái + tìm theo mã/tên/SĐT + lọc khoảng ngày + 4 ô số liệu nhanh |
| `/QuanLyDonHang/ChiTiet/{id}` | GET | Chi tiết đơn |
| `/QuanLyDonHang/CapNhatTrangThai` | POST | Đổi trạng thái đơn theo luồng xử lý |
| `/QuanLyKhuyenMai` | GET | Danh sách chương trình khuyến mãi |
| `/QuanLyKhuyenMai/Them`, `/Sua/{id}` | GET/POST | Thêm / sửa khuyến mãi |
| `/QuanLyKhuyenMai/Xoa/{id}` → `/XacNhanXoa/{id}` | GET → POST | Xóa khuyến mãi |
| `/QuanLyKhuyenMai/DoiTrangThai` | POST | Bật/tắt nhanh một chương trình ngay tại bảng |

---

## 10. Nghiệp vụ chi tiết

### 10.1. Tùy chọn sản phẩm và cách tính đơn giá

Bảng phụ thu nằm ở lớp tĩnh `TuyChonSanPham` (`Models/GioHang.cs`) — Controller và View dùng chung
để tránh hard-code nhiều nơi:

| Nhóm tùy chọn | Giá trị | Kiểu nhập | Phụ thu |
|---|---|---|---|
| **Size** | S / M / L | radio | S: 0đ · M: **+6.000đ** · L: **+12.000đ** |
| **Mức đường** | 0% / 30% / 50% / 70% / 100% | radio | 0đ |
| **Mức đá** | 0% / 30% / 50% / 70% / 100% | radio | 0đ |
| **Topping** | Trân châu đen, Trân châu trắng, Thạch phô mai, Kem cheese, Pudding trứng, Thạch dừa | checkbox | 8.000 / 8.000 / 10.000 / 10.000 / 10.000 / 7.000 |

```
DonGia = SanPham.GiaBanThucTe + PhuThuSize(size) + PhuThuTopping(toppings)
ThanhTien (mỗi dòng) = DonGia × SoLuong
```

**Quy tắc gộp dòng giỏ hàng:** mỗi dòng có một `Khoa` = `MaSP_Size_Duong_Da_Toppings(đã sắp xếp)`.
Cùng sản phẩm nhưng khác tùy chọn sẽ là **2 dòng riêng**; trùng khóa thì cộng dồn số lượng —
giống cách các web trà sữa thật hoạt động.

Số lượng mỗi dòng bị kẹp trong khoảng **1–100**; cập nhật về ≤ 0 thì dòng đó bị xóa.

### 10.2. Mã giảm giá

```
Điều kiện áp dụng: KhuyenMai.ConHieuLuc == true  và  TamTinh >= DonToiThieu

TienGiam = TamTinh × PhanTramGiam / 100
           → nếu GiamToiDa > 0 và TienGiam > GiamToiDa thì TienGiam = GiamToiDa
           → không bao giờ vượt quá TamTinh
           → làm tròn (Math.Round)
```

Việc so khớp mã **không phân biệt hoa/thường** (`KhoDuLieu.TimTheoMaGiamGia`), và mã luôn được lưu
dạng **IN HOA** khi Seller tạo/sửa. Thông báo lỗi được tách rõ ba trường hợp: mã không tồn tại,
mã hết hạn/chưa kích hoạt, và chưa đạt đơn tối thiểu.

### 10.3. Phí giao hàng

```
PhiGiaoHang = 15.000đ
Miễn phí khi TamTinh >= 150.000đ  (hằng số trong GioHangController)

TongTien = TamTinh − TienGiam + PhiGiaoHang
```

### 10.4. Đặt hàng

Khi đặt hàng thành công (`GioHang/ThanhToan` POST), hệ thống:

1. Tạo `DonHang` với trạng thái **"Chờ xác nhận"**, `HinhThucThanhToan` mặc định `"COD"`.
2. **Chốt giá tại thời điểm mua**: mỗi `ChiTietDonHang` lưu lại `TenSP`, `DonGia` và chuỗi `TuyChon`
   (ví dụ `Size L | Đường 50% | Đá 30% | Topping: Trân châu đen`) — về sau Seller đổi giá sản phẩm
   thì đơn cũ **không bị thay đổi**.
3. **Trừ tồn kho** (`SoLuongTon`, không cho âm) và **cộng lượt bán** (`LuotBan`) cho từng sản phẩm.
4. Xóa sạch giỏ hàng trong Session.
5. Chuyển sang trang `DatHangThanhCong/{MaDH}`.

Mã đơn hàng bắt đầu từ **1001**.

### 10.5. Thống kê doanh thu

`ThongKeController` mặc định thống kê **30 ngày gần nhất** (`denNgay` = hôm nay, `tuNgay` = trước đó 29 ngày);
người bán có thể tự chọn khoảng ngày.

> **Quy ước quan trọng khi bảo vệ:** doanh thu **chỉ tính các đơn có trạng thái "Hoàn thành"**.
> Đơn đang xử lý hoặc đã hủy được đếm số lượng nhưng **không tính vào doanh thu**.

Số liệu hiển thị:

| Nhóm | Chỉ số |
|---|---|
| Tổng quan | Tổng doanh thu, tổng đơn, đơn hoàn thành, đơn chờ xác nhận, đơn đã hủy, giá trị đơn trung bình, tổng sản phẩm, tổng khách hàng |
| Biểu đồ cột | Doanh thu và số đơn **theo từng ngày** trong khoảng chọn |
| Cơ cấu | Doanh thu và số lượng bán **theo loại sản phẩm** |
| Xếp hạng | **Top 5 sản phẩm bán chạy** (theo số lượng bán) |
| Bảng phụ | 5 đơn hàng mới nhất |

---

## 11. Validation & ràng buộc dữ liệu

### 11.1. Ba lớp kiểm tra

1. **Client-side** — jQuery Validation + Unobtrusive (bật sẵn trong `Web.config`:
   `ClientValidationEnabled` và `UnobtrusiveJavaScriptEnabled` = `true`).
2. **Data Annotation + `ModelState.IsValid`** — kiểm tra định dạng cơ bản.
3. **Kiểm tra nghiệp vụ trong Controller** — những thứ Data Annotation không làm được.

### 11.2. Data Annotation tiêu biểu

| Model | Quy tắc |
|---|---|
| `SanPham` | `TenSP` bắt buộc, ≤ 150 ký tự · `Gia` từ **1.000đ – 10.000.000đ** · `SoLuongTon` ≥ 0 · `MoTa` ≤ 1000 ký tự |
| `LoaiSanPham` | `TenLoai` bắt buộc, ≤ 100 ký tự |
| `NguoiDung` | `Email` đúng định dạng · `MatKhau` ≥ **6 ký tự** · `DienThoai` khớp `^0\d{9}$` (10 số, bắt đầu bằng 0) |
| `DonHang` | Tên người nhận, SĐT, địa chỉ bắt buộc; SĐT cùng quy tắc như trên |
| `ChiTietDonHang` | `SoLuong` từ **1 – 100** |
| `KhuyenMai` | `MaGiamGia` chỉ gồm **chữ và số, không dấu, không khoảng trắng** (`^[A-Za-z0-9]+$`) · `PhanTramGiam` 0–100 |
| `DangKyViewModel` | Có ô **xác nhận mật khẩu** (`Compare`) |

### 11.3. Kiểm tra nghiệp vụ trong Controller

| Nơi kiểm tra | Quy tắc |
|---|---|
| `TaiKhoanController.DangKy` | **Email không được trùng** |
| `QuanLySanPhamController.KiemTraNghiepVu` | Loại sản phẩm phải tồn tại · **Giá khuyến mãi phải nhỏ hơn giá bán** · **Tên sản phẩm không trùng** |
| `QuanLyLoaiController.KiemTraTrungTen` | **Tên loại không trùng** |
| `QuanLyKhuyenMaiController.KiemTraNghiepVu` | **Mã giảm giá không trùng** · **Ngày kết thúc ≥ ngày bắt đầu** · **Phần trăm giảm > 0** |
| `QuanLyDonHangController.CapNhatTrangThai` | Trạng thái phải thuộc `DonHang.CacTrangThai` · đơn đã **Hoàn thành/Đã hủy** thì không đổi tiếp |
| `DonHangController.HuyDon` | Chỉ hủy được đơn đang **"Chờ xác nhận"** |

### 11.4. Ràng buộc khóa ngoại khi xóa

| Thao tác | Quy tắc |
|---|---|
| **Xóa loại sản phẩm** | ❌ Chặn nếu loại đó **còn sản phẩm**; thông báo rõ số lượng sản phẩm đang vướng |
| **Xóa sản phẩm** | ❌ Chặn nếu sản phẩm **đang nằm trong đơn chưa xử lý xong** (khác Hoàn thành và Đã hủy); gợi ý chuyển sang **"Ngừng bán"** thay vì xóa |

Mọi thao tác xóa đều đi qua **trang xác nhận riêng** (`Xoa` → GET hiển thị, `XacNhanXoa` → POST thực hiện),
kèm `[ValidateAntiForgeryToken]`.

---

## 12. Phân quyền & bảo mật

### 12.1. Hai filter tự viết (`Filters/PhanQuyenAttribute.cs`)

| Filter | Áp dụng cho | Hành vi khi không đạt |
|---|---|---|
| `[KiemTraDangNhap]` | Thanh toán, đơn hàng của tôi, thông tin tài khoản | Request thường → chuyển đến `/TaiKhoan/DangNhap?returnUrl=...`<br>Request **AJAX** → trả JSON `{ thanhCong: false, canDangNhap: true, thongBao: "..." }` để JavaScript xử lý, không redirect |
| `[KiemTraNguoiBan]` | **Toàn bộ** controller khu quản trị (đặt ở cấp class) | Chưa đăng nhập → về trang đăng nhập kèm `returnUrl`<br>Đã đăng nhập nhưng là Customer → `/TaiKhoan/KhongCoQuyen` |

> Khách hàng gõ tay URL `/QuanLySanPham` sẽ bị đưa sang trang "Không có quyền truy cập" —
> đây là câu hỏi hay gặp khi bảo vệ đồ án.

### 12.2. Các lớp bảo vệ khác

* **`[ValidateAntiForgeryToken]`** trên mọi form POST quan trọng (đăng ký, đăng nhập, đăng xuất,
  thêm/sửa/xóa, đặt hàng, hủy đơn) — chống CSRF.
* **Kiểm tra quyền sở hữu dữ liệu**: khách chỉ xem/hủy được **đơn của chính mình**
  (`DonHangController`, `GioHangController.DatHangThanhCong`), sai thì trả `HttpUnauthorizedResult`.
* **`Url.IsLocalUrl(returnUrl)`** trước khi redirect — chống **open redirect** sang site ngoài.
* Lọc topping đầu vào: chỉ chấp nhận topping có thật trong bảng `TuyChonSanPham.CacTopping`.

### 12.3. ⚠️ Hạn chế bảo mật đã biết

**Mật khẩu đang được lưu ở dạng plain text** (`TaiKhoanController.DangKy` có ghi chú rõ trong code)
để tiện demo. Trước khi vận hành thật **bắt buộc** phải băm mật khẩu (SHA256 + salt, hoặc BCrypt/PBKDF2).

---

## 13. Giao diện (View & tài nguyên tĩnh)

* **Hai layout tách biệt:**
  * `_Layout.cshtml` — giao diện khách hàng, tông pastel hồng/kem, **menu 2 cấp** ("Thực đơn" → các loại sản phẩm),
    ô tìm kiếm ở header, icon giỏ hàng có badge số lượng.
  * `_LayoutSeller.cshtml` — khu quản trị dạng **sidebar**.
* **Partial dùng lại:** `_CardSanPham.cshtml` (card sản phẩm), `_FormSanPham.cshtml`, `_FormLoai.cshtml`,
  `_FormKhuyenMai.cshtml` — form Thêm và Sửa dùng chung một partial, tránh lặp code.
* **CSS:** toàn bộ thiết kế riêng nằm ở `Content/sugarchills.css` (`Site.css` đã rút gọn).
* **JavaScript:** `Scripts/sugarchills.js` xử lý thêm giỏ bằng AJAX, gợi ý tìm kiếm, nút tăng/giảm số lượng,
  và tính lại giá theo tùy chọn ở trang chi tiết.
* **Bundle** (`BundleConfig.cs`): `~/bundles/jquery`, `~/bundles/jqueryval`, `~/bundles/bootstrap`
  (dùng `bootstrap.bundle.js` **đã kèm Popper** để dropdown 2 cấp chạy được), `~/bundles/sugarchills`,
  và style bundle `~/Content/css`.
* **Ảnh sản phẩm:** hiện dùng link Unsplash, có `onerror` tự thay bằng ảnh placeholder nên giao diện
  không vỡ khi mất mạng. Muốn dùng ảnh riêng: bỏ ảnh vào `Content/images/` rồi đặt trường **Hình ảnh**
  của sản phẩm là `/Content/images/ten-anh.jpg`.

---

## 14. Đối chiếu thang điểm (rubric)

| # | Tiêu chí | Điểm | Nơi hiện thực |
|---|---|---|---|
| 1 | Layout, menu 2 cấp, màu sắc | 0.75 | `_Layout.cshtml` (dropdown "Thực đơn" → các loại), `sugarchills.css` |
| 2 | Đăng ký / Đăng nhập / Tìm kiếm | 0.75 | `TaiKhoanController`, ô tìm kiếm ở header + `SanPhamController.Index` / `GoiY` |
| 3 | Danh sách SP: hiển thị, CSS hover, lọc theo loại | 1.0 | `SanPham/Index.cshtml`, `.sc-product:hover`, cột bộ lọc, `KhoDuLieu.LocSanPham` |
| 4 | Chi tiết SP: hiển thị, định dạng, tùy chọn bổ sung, SP liên quan | 1.0 | `SanPham/ChiTiet.cshtml`, `LaySanPhamLienQuan`, `TuyChonSanPham` |
| 5 | Giỏ hàng & đặt hàng (1 – n món, tùy chọn, hiệu chỉnh, lưu đơn) | 1.5 | `GioHangController` + `GioHang/Index`, `ThanhToan`, `DatHangThanhCong` |
| 6 | Bảng sản phẩm: thêm/xóa/sửa, hình + loại, radio + checkbox | 1.0 | `QuanLySanPhamController`, `_FormSanPham.cshtml` (radio `DangBan`, checkbox `NoiBat`) |
| 7 | Đơn hàng, khuyến mãi, thống kê doanh thu | 1.5 | `QuanLyDonHangController`, `QuanLyKhuyenMaiController`, `ThongKeController` |
| 8 | Bảng loại: thêm/xóa/sửa/hiển thị | 0.5 | `QuanLyLoaiController` |

**Những điểm dễ bị hỏi khi bảo vệ — đã chuẩn bị sẵn:**

* Tùy chọn khi mua: size (**radio**), mức đường, mức đá, topping (**checkbox**) — có phụ thu và tính lại giá bằng jQuery.
* Hiệu chỉnh giỏ: tăng/giảm/nhập số lượng, xóa từng dòng, xóa toàn bộ, áp dụng & gỡ mã giảm giá.
* Validation ba lớp (client → Data Annotation → nghiệp vụ) — xem [mục 11](#11-validation--ràng-buộc-dữ-liệu).
* Ràng buộc khóa ngoại khi xóa loại / xóa sản phẩm.
* Phân quyền bằng **filter tự viết**, không dùng Identity — dễ giải thích từng dòng.

---

## 15. Lộ trình chuyển sang Entity Framework 6 + SQL Server

Toàn bộ Controller/View chỉ gọi các hàm của `KhoDuLieu`, nên **chỉ cần thay phần thân các hàm đó**
là hệ thống chạy trên SQL Server thật mà không phải sửa Controller hay View.

**Bước 1 — Cài EF6** (Tools → NuGet Package Manager → Package Manager Console):

```powershell
Install-Package EntityFramework -Version 6.4.4
```

**Bước 2 — Tạo `Models/FoodHubContext.cs`:**

```csharp
public class FoodHubContext : DbContext
{
    public FoodHubContext() : base("name=FoodHubConnection") { }

    public DbSet<LoaiSanPham> LoaiSanPhams { get; set; }
    public DbSet<SanPham> SanPhams { get; set; }
    public DbSet<NguoiDung> NguoiDungs { get; set; }
    public DbSet<DonHang> DonHangs { get; set; }
    public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
    public DbSet<KhuyenMai> KhuyenMais { get; set; }
}
```

**Bước 3 — Thêm connection string vào `Web.config`** (hiện `Web.config` **chưa có** thẻ `connectionStrings`):

```xml
<connectionStrings>
  <add name="FoodHubConnection"
       connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=SugarChillsDB;Integrated Security=True;MultipleActiveResultSets=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**Bước 4 — Thay ruột `KhoDuLieu` bằng LINQ trên `FoodHubContext`.** Ví dụ:

```csharp
public static List<SanPham> LaySanPhams()
{
    using (var db = new FoodHubContext())
        return db.SanPhams.Include("LoaiSanPham").ToList();
}
```

**Bước 5 — Những chỗ phải cẩn thận khi chuyển:**

| Vấn đề | Cách xử lý |
|---|---|
| `GiaBanThucTe`, `PhanTramGiam`, `ThanhTien`, `ConHieuLuc` là `[NotMapped]` | **Không** dùng trực tiếp trong LINQ-to-Entities. Gọi `.ToList()` trước rồi lọc, hoặc viết điều kiện trên cột thật |
| Đối tượng `NguoiDung` đang lưu thẳng vào Session | Với EF nên lưu `MaND` vào Session rồi nạp lại từ DB, tránh giữ entity đã detach |
| Mật khẩu plain text | Băm SHA256 + salt trong `TaiKhoanController` trước khi lưu và khi so khớp đăng nhập |
| Sinh khóa chính | Bỏ các biến đếm `_idSanPham`, `_idDonHang`… chuyển sang `IDENTITY` của SQL Server |

---

## 16. Hạn chế đã biết & hướng phát triển

**Đang là dữ liệu tạm (in-memory)**

* Mọi thay đổi mất khi khởi động lại project — sẽ hết khi nối EF6 + SQL Server.

**Chưa có (đề xuất làm tiếp)**

* Quên mật khẩu / đổi mật khẩu / sửa thông tin cá nhân (trang `ThongTin.cshtml` hiện chỉ hiển thị).
* **Upload ảnh từ máy** — form sản phẩm hiện chỉ có ô dán link ảnh.
* Lưu tin nhắn của form Liên hệ vào CSDL và cho Seller phản hồi.
* Thanh toán QR / cổng thanh toán thật; đăng nhập Google.
* Chức năng đánh giá sản phẩm (phần sao ở trang chi tiết hiện đang là số cố định — nên bỏ hoặc làm thật).
* Trang riêng liệt kê tất cả khuyến mãi; chọn nhanh mã gợi ý ngay ở giỏ hàng.
* In hóa đơn, email xác nhận đơn hàng.

**Bảo mật**

* Băm mật khẩu (bắt buộc trước khi vận hành thật).

---

## 17. Quy ước Git

**Nhánh:**

```
main              ← code ổn định, dùng để nộp/bảo vệ
develop           ← nhánh tích hợp
feature/product   feature/cart   feature/order   feature/auth   feature/promotion
```

**Commit message:**

```
feat: add product management
feat: implement shopping cart
feat: add order management
fix: fix product validation
style: improve customer layout
docs: update README
```

**Gợi ý `.gitignore`** (repo hiện chưa có) — không nên commit các thư mục sinh tự động:

```gitignore
bin/
obj/
.vs/
packages/
*.user
*.suo
```

---

## 18. Xử lý sự cố thường gặp

| Hiện tượng | Nguyên nhân | Cách khắc phục |
|---|---|---|
| Solution Explorer không thấy file mới thêm | `.csproj` chưa nạp lại | Chuột phải project → **Unload Project** → **Reload Project** |
| Lỗi thiếu `System.Web.Mvc`, `Newtonsoft.Json`… | Chưa restore NuGet | Chuột phải Solution → **Restore NuGet Packages**, rồi **Rebuild** |
| Menu "Thực đơn" bấm không xổ xuống | Thiếu Popper | Bundle `~/bundles/bootstrap` phải dùng **`bootstrap.bundle.js`** (đã cấu hình sẵn) |
| Ảnh sản phẩm không hiện | Mất mạng / link Unsplash hỏng | Có `onerror` tự thay ảnh placeholder; muốn ổn định thì để ảnh trong `Content/images/` |
| Sản phẩm/đơn hàng vừa thêm biến mất | Dữ liệu nằm trong RAM | Bình thường ở giai đoạn này — nối SQL Server để lưu vĩnh viễn |
| Vào `/QuanLySanPham` bị đá sang "Không có quyền" | Đang đăng nhập bằng tài khoản Customer | Đăng nhập bằng `seller@sugarchills.vn` |
| Nhập mã giảm giá báo không áp dụng được | Chưa đạt **đơn tối thiểu**, mã hết hạn, hoặc chưa `KichHoat` | Xem lại điều kiện ở `/QuanLyKhuyenMai`; thử mã `SUGAR10` (không yêu cầu đơn tối thiểu) |
| Không đổi được trạng thái đơn | Đơn đã **Hoàn thành** hoặc **Đã hủy** | Đây là quy tắc nghiệp vụ, không phải lỗi |

---

## 19. Nhóm thực hiện

| Họ và tên | MSSV | Vai trò / Phần phụ trách |
|---|---|---|
| *(điền tên)* | | |
| *(điền tên)* | | |
| *(điền tên)* | | |

* **Môn học:** Lập trình Web
* **Bảng phân công chi tiết:** `BangPhanCong_QL_MatHangAnUong.docx`
* **Tài liệu liên quan:** `BaoCao_Lan1_KeHoachDoAn.docx`, `ThietKeKyThuat_PhanCongChiTiet.docx`, `HUONG_DAN_SUGARCHILLS.md`
