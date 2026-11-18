# SugarChills — Hướng dẫn cài đặt & chạy dự án

Tài liệu này mô tả phiên bản hiện tại của dự án **QL_MatHangAnUong** (SugarChills). Chi tiết quy tắc nghiệp vụ và bảo mật xem thêm trong `README.md`.

---

## 1. Giới thiệu project

**SugarChills** là website bán đồ ăn/uống (trà sữa, cà phê, kem, bánh ngọt), làm cho đồ án môn **Lập trình Web**. Dự án có hai phía:

* **Khách hàng:** xem thực đơn, tùy chọn món (size, đường, đá, topping), giỏ hàng, đặt hàng, theo dõi đơn, đánh giá sản phẩm.
* **Người bán (Seller):** khu quản trị gồm sản phẩm, loại, đơn hàng, khuyến mãi, đánh giá, tin nhắn liên hệ và thống kê doanh thu.

Dữ liệu lưu trong **SQL Server**, truy cập bằng **Entity Framework 6 (Database First)**.

---

## 2. Công nghệ sử dụng

| Thành phần | Chi tiết |
|---|---|
| Nền tảng | ASP.NET MVC **5.2.9**, **.NET Framework 4.7.2**, Razor |
| Truy cập dữ liệu | **Entity Framework 6.5.2**, Database First (`Models/QL_MatHangAnuong.edmx`) |
| Cơ sở dữ liệu | SQL Server / SQL Server Express |
| Giao diện | Bootstrap 5.2.3, jQuery 3.7.0, jQuery Validation, Bootstrap Icons, CSS riêng `Content/sugarchills.css` |
| Đa ngôn ngữ | Việt / Anh (`App_GlobalResources/Strings.resx`, `Strings.en.resx`) |
| Đăng nhập mạng xã hội | Google / Facebook (OAuth 2.0, gọi trực tiếp qua `Helpers/DangNhapMangXaHoi.cs`) |
| Gửi email | Gmail SMTP (`Helpers/GuiMail.cs`) — dùng cho mã OTP quên mật khẩu |
| IDE | Visual Studio 2022 (workload *ASP.NET and web development*) |

---

## 3. Tạo database từ script SQL

1. Mở **SQL Server Management Studio (SSMS)** và kết nối vào SQL Server của máy bạn.
2. Mở file **`QL_MatHangAnUong_ToanBo.sql`** (thư mục gốc repo) rồi bấm **Execute**.
3. Script tự làm toàn bộ:
   * tạo database `QL_MatHangAnUong` nếu chưa có;
   * tạo các bảng `LoaiSanPham`, `SanPham`, `NguoiDung`, `KhuyenMai`, `DonHang`, `ChiTietDonHang`, `CuaHang`, `DanhGia`, `LienLacs`;
   * nạp dữ liệu mẫu (loại, sản phẩm, tài khoản, khuyến mãi, cửa hàng, đơn hàng mẫu).

> ⚠️ **Cảnh báo:** script **xóa và tạo lại** các bảng nghiệp vụ (kể cả `DanhGia`, `NguoiDung`, `DonHang`...) nên mọi dữ liệu đang có, như tài khoản hay đơn hàng tự tạo, sẽ mất. Riêng bảng `LienLacs` được giữ nguyên dữ liệu nếu đã tồn tại. Có thể chạy lại bao nhiêu lần tùy ý khi muốn đưa database về dữ liệu mẫu.

`TaoBang_DanhGia.sql` và `TaoBang_LienLacs.sql` chỉ là các script rời đã được gộp sẵn vào `QL_MatHangAnUong_ToanBo.sql`, bình thường **không cần chạy**.

---

## 4. Cấu hình connection string và Secrets.config

### 4.1. Connection string

Nằm ở cuối file `QL_MatHangAnUong/Web.config`, tên **`QL_MatHangAnUongEntities`** (kiểu EF `System.Data.EntityClient`). Phần cần chú ý bên trong `provider connection string`:

```
data source=.\SQLEXPRESS;initial catalog=QL_MatHangAnUong;integrated security=True;trustservercertificate=True;MultipleActiveResultSets=True
```

* Mặc định dùng **SQL Express trên máy local** với đăng nhập Windows (`integrated security=True`).
* Nếu dùng instance khác, sửa `data source=` cho khớp, ví dụ `(localdb)\MSSQLLocalDB` hoặc `TENMAY\SQLEXPRESS`.
* **Giữ nguyên** phần `metadata=res://*/Models.QL_MatHangAnuong.csdl|...` (đường dẫn tới model EF) và `MultipleActiveResultSets=True`.

### 4.2. Secrets.config (không bắt buộc)

`Web.config` nạp thêm file `Secrets.config` qua `<appSettings file="Secrets.config">`. File này chứa khóa bí mật, đã bị git bỏ qua (**không commit**).

1. Copy `QL_MatHangAnUong/Secrets.config.example` thành `QL_MatHangAnUong/Secrets.config`.
2. Điền các khóa bạn cần:

| Khóa | Dùng cho | Lấy ở đâu |
|---|---|---|
| `GoogleClientId`, `GoogleClientSecret` | Đăng nhập Google | console.cloud.google.com → APIs & Services → Credentials → OAuth client ID (Web application) |
| `FacebookAppId`, `FacebookAppSecret` | Đăng nhập Facebook | developers.facebook.com → My Apps → Facebook Login |
| `SmtpUser`, `SmtpPass` | Gửi mã OTP quên mật khẩu | `SmtpUser` là địa chỉ Gmail, `SmtpPass` là **App Password 16 ký tự** (bật xác minh 2 bước rồi tạo tại myaccount.google.com/apppasswords, **không** dùng mật khẩu Gmail thường) |

3. Redirect URI cần đăng ký phía Google/Facebook (theo cổng IIS Express SSL mặc định của project là 44373):
   * `https://localhost:44373/TaiKhoan/GoogleCallback`
   * `https://localhost:44373/TaiKhoan/FacebookCallback`

Không có `Secrets.config` hoặc để trống thì nút Google/Facebook và trang quên mật khẩu chỉ báo "chưa cấu hình"; phần còn lại vẫn chạy bình thường. **Xóa `Secrets.config` trước khi nén nộp bài.**

---

## 5. Build và chạy

1. Mở `QL_MatHangAnUong.sln` bằng Visual Studio 2022.
2. Chuột phải Solution → **Restore NuGet Packages** (cần có mạng ở lần đầu; thư mục `packages` không đi kèm file nén).
3. **Build → Rebuild Solution**.
4. Bấm **F5** (IIS Express, mặc định `https://localhost:44373/`).

Build bằng dòng lệnh (tùy chọn):

```
nuget restore QL_MatHangAnUong.sln
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" QL_MatHangAnUong.sln /p:Configuration=Debug
```

Một số lỗi thường gặp:

| Hiện tượng | Cách khắc phục |
|---|---|
| Không kết nối được SQL Server | Sửa `data source=` trong `Web.config`; kiểm tra service SQL Server đang chạy |
| Thiếu `System.Web.Mvc`, `EntityFramework`... | Restore NuGet Packages rồi Rebuild |
| `Invalid object name 'DanhGia'` (hoặc bảng khác) | Chưa chạy `QL_MatHangAnUong_ToanBo.sql` |
| Vào `/QuanLy...` bị chuyển sang "Không có quyền" | Đăng nhập bằng tài khoản Seller |
| Mở `.edmx` báo `TYPE_E_LIBNOTREGISTERED` | Repair/Update Visual Studio 2022 |

---

## 6. Tài khoản đăng nhập mẫu

| Vai trò | Email | Mật khẩu | Vào được |
|---|---|---|---|
| Người bán (Seller) | `seller@sugarchills.vn` | `123456` | Toàn bộ khu quản trị (`/QuanLy...`, `/ThongKe`) |
| Khách hàng | `khach@gmail.com` | `123456` | Giỏ hàng, đặt hàng, đơn của tôi, đánh giá |
| Khách hàng | `khoa.tran@gmail.com` | `123456` | như trên |

Khách vãng lai (chưa đăng nhập) vẫn xem được thực đơn và thêm vào giỏ; chỉ khi **đặt hàng** mới bị yêu cầu đăng nhập. Mã giảm giá mẫu: `SUGAR10`, `CHILL15` (freeship), `HAPPY20`.

> Mật khẩu hiện lưu **plain text** để đơn giản hóa đồ án — hệ thống thật cần băm (BCrypt/PBKDF2).

---

## 7. Cấu trúc thư mục chính

```
QL_MatHangAnUong.sln
QL_MatHangAnUong_ToanBo.sql        ← tạo database + dữ liệu mẫu
QL_MatHangAnUong/
├─ Controllers/
│    BaseController (db, menu, giỏ hàng, culture, phân trang, thông báo)
│    Phía khách : Home, SanPham, GioHang, DonHang, TaiKhoan, HoaDon, DanhGia, CuaHang, NgonNgu
│    Khu quản trị: QuanLySanPham, QuanLyLoai, QuanLyDonHang, QuanLyKhuyenMai,
│                  QuanLyDanhGia, QuanLyLienLac, ThongKe
├─ Models/
│    QL_MatHangAnuong.edmx (+ Context, entity do T4 sinh — không sửa tay)
│    *Partial.cs (bổ sung thuộc tính/hàm cho entity), GioHang.cs (giỏ trong Session
│    + bảng phụ thu tùy chọn); controller truy vấn EF trực tiếp qua `db`
│    ViewModels/
├─ Filters/       PhanQuyenAttribute.cs  → [KiemTraDangNhap], [KiemTraNguoiBan]
├─ Helpers/       PhienLamViec (Session), DinhDang (tiền/ngày), Ngu (chuỗi đa ngôn ngữ trong .cs),
│                 DangNhapMangXaHoi (Google/Facebook), GuiMail (SMTP)
├─ Views/         theo từng controller + Shared/ (_Layout, _LayoutSeller, _CardSanPham, _PhanTrang)
├─ App_GlobalResources/   Strings.resx (vi), Strings.en.resx
├─ App_Start/     RouteConfig (một route mặc định {controller}/{action}/{id}), BundleConfig, FilterConfig
├─ Content/       sugarchills.css, Images/ (ảnh mẫu), Uploads/SanPham/ (ảnh người bán tải lên)
├─ Scripts/       sugarchills.js (giỏ AJAX, gợi ý tìm kiếm, tính giá theo tùy chọn), jQuery, Bootstrap
├─ Web.config     connection string, appSettings (nạp Secrets.config)
└─ Secrets.config.example
```

Luồng xử lý: **Browser → Controller (kế thừa `BaseController`) → EF6 `QL_MatHangAnUongEntities` → SQL Server → View Razor**.

Ghi chú:

* Đăng nhập dùng **Session** (không dùng ASP.NET Identity); Google/Facebook chỉ để xác minh danh tính rồi cũng ghi vào Session.
* CSS/JS nhúng trực tiếp bằng thẻ `<link>`/`<script>` trong layout, **không dùng bundling** (`BundleConfig` để trống).
* Thêm/sửa bảng: sửa database bằng SQL rồi **Update Model from Database** trong `.edmx`; thêm hành vi riêng ở file `*Partial.cs`.
* Controller/Model (.cs) lấy chuỗi đa ngôn ngữ bằng `Ngu.S("Key")`; View dùng `@Resources.Strings.Key`.

---

## 8. Các chức năng chính

### Khách hàng

* **Trang chủ, giới thiệu, khuyến mãi, hệ thống cửa hàng** (lọc theo tỉnh/quận), **liên hệ** (lưu vào bảng `LienLacs`), **đổi ngôn ngữ** Việt/Anh.
* **Thực đơn:** tìm kiếm (có gợi ý), lọc theo loại, lọc khoảng giá, sắp xếp, phân trang; **chi tiết sản phẩm** với sản phẩm liên quan và đánh giá.
* **Tài khoản:** đăng ký, đăng nhập (email hoặc Google/Facebook), xem/cập nhật thông tin, đổi mật khẩu, **quên mật khẩu bằng OTP gửi qua email**.
* **Giỏ hàng** (lưu Session): chọn size (S/M/L), mức đường, mức đá, topping; thêm nhanh bằng AJAX; tăng/giảm/xóa; áp mã giảm giá.
* **Đặt hàng:** thanh toán COD hoặc QR, tính phí giao hàng (freeship từ 150.000đ, ngược lại 15.000đ), trang đặt hàng thành công.
* **Đơn hàng của tôi:** lọc theo trạng thái, xem chi tiết, tự hủy khi còn *Chờ xác nhận* (tồn kho được hoàn lại), **in hóa đơn**.
* **Đánh giá sản phẩm:** chỉ khi đã có đơn *Hoàn thành* chứa món đó, mỗi món đánh giá một lần.

### Người bán (Seller)

* **Thống kê doanh thu** theo khoảng ngày (chỉ tính đơn *Hoàn thành*).
* **Sản phẩm:** xem/tìm/lọc, thêm, sửa, xóa, bật/tắt kinh doanh, đánh dấu nổi bật, tải ảnh từ máy hoặc chọn ảnh có sẵn.
* **Loại sản phẩm:** thêm, sửa, xóa (không xóa loại đang có sản phẩm).
* **Đơn hàng:** lọc theo trạng thái/từ khóa/ngày, xem chi tiết, đổi trạng thái (Chờ xác nhận → Đã xác nhận → Đang giao → Hoàn thành, hoặc Đã hủy), in hóa đơn.
* **Khuyến mãi:** thêm, sửa, xóa, bật/tắt mã giảm giá (giảm theo tổng tiền hoặc theo phí giao hàng).
* **Đánh giá:** xem, lọc theo sao/từ khóa, xóa.
* **Tin nhắn liên hệ:** xem, đánh dấu đã đọc/chưa đọc, xóa.

### Phân quyền & bảo mật

* Khu quản trị chỉ cho Seller; khách gõ tay URL `/QuanLy...` bị chuyển sang trang "Không có quyền".
* Mọi form POST có chống CSRF; khách chỉ xem/hủy/in **đơn của chính mình**; `returnUrl` được kiểm tra chống open redirect.
