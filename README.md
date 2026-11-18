# SugarChills — Website bán đồ ăn/uống (QL_MatHangAnUong)

Đồ án môn **Lập trình Web**: website bán trà sữa, cà phê, kem, bánh ngọt.
**ASP.NET MVC 5.2 (.NET Framework 4.7.2)** · Razor · Bootstrap 5 · jQuery · **Entity Framework 6 (Database First, file `.edmx`)** · **SQL Server**.

---

## 1. Cách chạy

1. Cài **Visual Studio 2022** (workload *ASP.NET and web development*) và **SQL Server / SQL Server Express**.
2. Mở **SSMS**, chạy file **`QL_MatHangAnUong_ToanBo.sql`** (ở thư mục gốc repo). Script tự tạo database `QL_MatHangAnUong`, toàn bộ bảng (gồm `LienLacs`, `DanhGia`) và dữ liệu mẫu. Có thể chạy lại nhiều lần (sẽ xóa và tạo lại).
3. Mở `QL_MatHangAnUong.sln` → chuột phải Solution → **Restore NuGet Packages** → **Rebuild**.
4. Kiểm tra connection string trong `QL_MatHangAnUong/Web.config` (tên `QL_MatHangAnUongEntities`). Mặc định là `data source=.\SQLEXPRESS` (SQL Express trên máy local). Nếu dùng instance khác (ví dụ `(localdb)\MSSQLLocalDB` hay `TENMAY\SQLEXPRESS`) thì sửa `data source=` cho khớp.
5. Nhấn **F5**.

### Đăng nhập Google / Facebook (không bắt buộc)

Copy `Secrets.config.example` → `Secrets.config` rồi điền Client ID/Secret. Không có file này thì các nút đăng nhập mạng xã hội báo "chưa cấu hình", phần còn lại vẫn chạy bình thường.

### Gửi OTP quên mật khẩu (Gmail SMTP — miễn phí)

Trong `Secrets.config` điền `SmtpUser` (địa chỉ Gmail) và `SmtpPass` (**App Password** 16 ký tự: bật xác minh 2 bước rồi tạo tại myaccount.google.com/apppasswords — không dùng mật khẩu Gmail thường). Chưa điền thì trang quên mật khẩu báo "chưa cấu hình gửi mã".
`Secrets.config` đã bị git bỏ qua — **không commit, và xóa file này trước khi nén nộp bài.**

### Tài khoản dùng thử

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Người bán (Seller) | `seller@sugarchills.vn` | `123456` |
| Khách hàng | `khach@gmail.com` | `123456` |
| Khách hàng | `khoa.tran@gmail.com` | `123456` |

---

## 2. Chức năng

**Khách (chưa đăng nhập):** xem trang chủ, thực đơn (tìm kiếm, lọc loại, lọc khoảng giá, sắp xếp, phân trang), chi tiết sản phẩm + sản phẩm liên quan, xem đánh giá, thêm giỏ hàng (Session), xem hệ thống cửa hàng, gửi liên hệ, đổi ngôn ngữ Việt/Anh.

**Khách hàng (Customer):**
* Đăng ký / đăng nhập (email hoặc Google/Facebook) / quên mật khẩu bằng **mã OTP gửi qua email**.
* Chọn size, đường, đá, topping; mã giảm giá; phí giao hàng; đặt hàng (COD / QR).
* Xem đơn của mình, tự hủy khi còn **Chờ xác nhận** (tồn kho được hoàn lại), **in hóa đơn**.
* **Đánh giá sản phẩm**: chỉ khi đã mua và có đơn **Hoàn thành** chứa món đó; mỗi món đánh giá **một lần**.

**Người bán (Seller):** thống kê doanh thu, quản lý sản phẩm, loại, đơn hàng (đổi trạng thái, in hóa đơn), khuyến mãi, **đánh giá** (xem/xóa), tin nhắn liên hệ.

### Quy tắc nghiệp vụ chính

* Trạng thái đơn: Chờ xác nhận → Đã xác nhận → Đang giao → Hoàn thành; hoặc Đã hủy. Đơn Hoàn thành/Đã hủy không đổi nữa.
* Đơn chuyển **Hoàn thành** thì tự đánh dấu **đã thanh toán** (`SoTienConLai = 0`).
* Doanh thu chỉ tính đơn **Hoàn thành**.
* Đặt hàng trừ tồn kho + cộng lượt bán; hủy đơn (khách hoặc người bán) hoàn lại.
* Đơn giá chốt tại thời điểm mua (lưu `TenSP`, `DonGia`, `TuyChon` trong chi tiết đơn).
* Giá = giá bán (hoặc khuyến mãi) + phụ thu size (S 0 / M +6.000 / L +12.000) + topping. Freeship từ 150.000đ, ngược lại 15.000đ.

---

## 3. Cấu trúc

```
QL_MatHangAnUong/
├─ Controllers/   BaseController, Home, SanPham, GioHang, DonHang, TaiKhoan, HoaDon, DanhGia, CuaHang, NgonNgu,
│                 QuanLySanPham, QuanLyLoai, QuanLyDonHang, QuanLyKhuyenMai, QuanLyDanhGia, QuanLyLienLac, ThongKe
├─ Filters/       PhanQuyenAttribute  ([KiemTraDangNhap], [KiemTraNguoiBan])
├─ Helpers/       DinhDang (tiền/ngày), Ngu (chuỗi đa ngôn ngữ), PhienLamViec, DangNhapMangXaHoi (OAuth)
├─ Models/        QL_MatHangAnUong.edmx (EF6 Database First), ViewModels/, GioHang.cs (giỏ trong Session)
├─ App_GlobalResources/  Strings.resx (vi), Strings.en.resx
├─ Views/ , Scripts/ , Content/
QL_MatHangAnUong_ToanBo.sql   ← tạo database + dữ liệu mẫu (chạy file này)
```

Luồng: **Browser → Controller (kế thừa `BaseController`, có `db`, menu, giỏ hàng) → EF6 `QL_MatHangAnUongEntities` → SQL Server → View Razor**.
Bảng `LienLacs` và `DanhGia` được tạo bằng SQL và đã Update Model trong `.edmx`.

---

## 4. Bảo mật

* **Phân quyền:** filter tự viết; khu quản trị chặn người không phải Seller (về trang "Không có quyền").
* **CSRF:** mọi form POST (kể cả giỏ hàng, AJAX thêm nhanh) có `@Html.AntiForgeryToken()` + `[ValidateAntiForgeryToken]`.
* **Quyền sở hữu:** khách chỉ xem/hủy/in **đơn của mình**.
* **Open redirect:** kiểm tra `Url.IsLocalUrl(returnUrl)`.
* **Quên mật khẩu (OTP email):** 3 bước (email → nhập OTP → mật khẩu mới; chỉ khi OTP đúng mới hiện ô mật khẩu mới); mã 6 số sinh bằng bộ sinh ngẫu nhiên mật mã, lưu trong Session, hiệu lực 5 phút, sai quá 5 lần phải xin mã mới, chờ 60 giây giữa 2 lần gửi (tối đa 5 lần/phiên); báo lỗi chung không lộ email có tồn tại hay không; tài khoản Seller không đặt lại qua form công khai.
* **Hạn chế đã biết:** mật khẩu đang lưu **plain text** để đơn giản hóa đồ án; hệ thống thật bắt buộc phải băm (BCrypt/PBKDF2).

---

## 5. Xử lý sự cố

| Hiện tượng | Cách khắc phục |
|---|---|
| Không kết nối được SQL Server | Sửa `data source=` trong `Web.config` cho đúng tên instance; kiểm tra service SQL Server đang chạy |
| Thiếu `System.Web.Mvc`, `EntityFramework`… | Restore NuGet Packages rồi Rebuild |
| Lỗi `Invalid object name 'DanhGia'` | Chưa chạy `QL_MatHangAnUong_ToanBo.sql` |
| Đơn Hoàn thành cũ hiện "Chưa thanh toán" | Chạy: `UPDATE DonHang SET DaThanhToan = 1, SoTienConLai = 0 WHERE TrangThai = N'Hoàn thành'` |
| Vào `/QuanLy...` bị đá sang "Không có quyền" | Đăng nhập bằng `seller@sugarchills.vn` |
| Trang quên mật khẩu báo "chưa cấu hình" / không nhận được mail | Điền `SmtpUser`, `SmtpPass` (App Password) trong `Secrets.config`; kiểm tra hòm thư Spam |
| Mở `.edmx` báo `TYPE_E_LIBNOTREGISTERED` | Repair/Update Visual Studio 2022 |

---

## 6. Hướng phát triển

Băm mật khẩu · quên mật khẩu bằng liên kết token · email xác nhận đơn · upload ảnh sản phẩm từ máy · cổng thanh toán thật · unit test.

## 7. Nhóm thực hiện

| Họ và tên | MSSV | Phần phụ trách |
|---|---|---|
| *(điền tên)* | | |
