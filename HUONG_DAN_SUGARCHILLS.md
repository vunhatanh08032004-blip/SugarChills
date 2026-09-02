# SugarChills — Hướng dẫn chạy & cấu trúc khung website

Dự án: **QL_MatHangAnUong** (ASP.NET Web Application .NET Framework 4.7.2, MVC 5, Razor, Bootstrap 5.2.3, jQuery 3.7).
Giai đoạn hiện tại: **khung giao diện đầy đủ + logic nghiệp vụ, chạy bằng dữ liệu mẫu trong bộ nhớ (chưa cần SQL Server)**.

---

## 1. Cách chạy

1. Giải nén thư mục ra ổ đĩa bất kỳ (ví dụ `D:\DoAn\SugarChills`).
2. Mở `QL_MatHangAnUong.sln` bằng Visual Studio 2022.
3. Chuột phải Solution → **Restore NuGet Packages** (hoặc cứ **Build**, VS sẽ tự tải). Lần đầu cần có mạng, chỉ mất vài giây vì máy bạn đã có sẵn các gói này trong cache NuGet.
4. Bấm **F5**. Trang chủ SugarChills hiện ra, không cần cấu hình database.

### Tài khoản dùng thử

| Vai trò | Email | Mật khẩu | Vào được |
|---|---|---|---|
| Người bán (Seller) | `seller@sugarchills.vn` | `123456` | Toàn bộ khu quản trị |
| Khách hàng (Customer) | `khach@gmail.com` | `123456` | Giỏ hàng, đặt hàng, đơn của tôi |

Khách vãng lai (chưa đăng nhập) vẫn xem được trang chủ, thực đơn, chi tiết sản phẩm và thêm vào giỏ; chỉ khi bấm **Đặt hàng** mới bị yêu cầu đăng nhập.

> Dữ liệu nằm trong RAM nên khi **restart project** thì sản phẩm/đơn hàng thêm mới sẽ trở lại như ban đầu. Đây là hành vi mong muốn ở giai đoạn dựng giao diện.

---

## 2. Cấu trúc theo mô hình MVC

```
Models/
  LoaiSanPham.cs, SanPham.cs, NguoiDung.cs,
  DonHang.cs, ChiTietDonHang.cs, KhuyenMai.cs   → Entity (dùng lại được cho EF6)
  GioHang.cs                                    → Giỏ hàng lưu trong Session + bảng phụ thu tùy chọn
  KhoDuLieu.cs                                  → Kho dữ liệu tạm, sau này thay bằng DbContext
  ViewModels/ViewModels.cs                      → ViewModel cho từng màn hình

Controllers/
  BaseController.cs        → Controller cha: nạp menu, số lượng giỏ, người dùng cho _Layout
  HomeController.cs        → Trang chủ, giới thiệu, liên hệ
  SanPhamController.cs     → Danh sách, tìm kiếm, lọc, chi tiết, sản phẩm liên quan
  GioHangController.cs     → Giỏ hàng, mã giảm giá, đặt hàng
  DonHangController.cs     → Đơn hàng phía khách
  TaiKhoanController.cs    → Đăng ký, đăng nhập, đăng xuất
  QuanLySanPhamController.cs / QuanLyLoaiController.cs
  QuanLyDonHangController.cs / QuanLyKhuyenMaiController.cs
  ThongKeController.cs     → Thống kê doanh thu (trang chủ khu quản trị)

Filters/PhanQuyenAttribute.cs  → [KiemTraDangNhap] và [KiemTraNguoiBan]
Helpers/PhienLamViec.cs        → Đọc/ghi Session (người dùng, giỏ hàng)
Helpers/DinhDang.cs            → Định dạng tiền tệ, ngày giờ kiểu Việt Nam

Views/Shared/_Layout.cshtml        → Layout khách hàng, menu 2 cấp
Views/Shared/_LayoutSeller.cshtml  → Layout khu quản trị (sidebar)
Views/Shared/_CardSanPham.cshtml   → Partial card sản phẩm dùng lại nhiều nơi

Content/sugarchills.css   → Toàn bộ giao diện pastel hồng/kem
Scripts/sugarchills.js    → Thêm giỏ AJAX, gợi ý tìm kiếm, tăng/giảm số lượng, tính giá theo tùy chọn
```

---

## 3. Đối chiếu với thang điểm

| Mục rubric | Điểm | Nằm ở đâu |
|---|---|---|
| 1. Layout, menu 2 cấp, màu sắc | 0.75 | `_Layout.cshtml` (dropdown "Thực đơn" → các loại), `sugarchills.css` |
| 2. Đăng ký / Đăng nhập / Tìm kiếm | 0.75 | `TaiKhoanController`, ô tìm kiếm ở header + `SanPham/Index` |
| 3. DSSP: hiển thị, CSS hover, lọc theo loại | 1.0 | `SanPham/Index.cshtml`, `.sc-product:hover`, cột bộ lọc |
| 4. Chi tiết SP: hiển thị, định dạng, bổ sung, SP liên quan | 1.0 | `SanPham/ChiTiet.cshtml` (tabs, tùy chọn, liên quan) |
| 5. Giỏ hàng & đặt hàng (1, n, tùy chọn, hiệu chỉnh, lưu) | 1.5 | `GioHangController` + `GioHang/Index`, `ThanhToan`, `DatHangThanhCong` |
| 6. Bảng sản phẩm: thêm/xóa/sửa, hình + loại, radio + checkbox | 1.0 | `QuanLySanPham/*`, `_FormSanPham.cshtml` |
| 7. Đơn hàng, khuyến mãi, thống kê doanh thu | 1.5 | `QuanLyDonHang/*`, `QuanLyKhuyenMai/*`, `ThongKe/Index` |
| 8. Bảng loại: thêm/xóa/sửa/hiển thị | 0.5 | `QuanLyLoai/*` |

Các điểm dễ bị hỏi khi bảo vệ, đã xử lý sẵn:

* **Tùy chọn khi chọn mua**: size (radio), mức đường, mức đá, topping (checkbox) — có cộng phụ thu và tính lại giá bằng jQuery.
* **Hiệu chỉnh giỏ**: tăng/giảm/nhập số lượng, xóa từng dòng, xóa toàn bộ, nhập mã giảm giá.
* **Validation**: Data Annotation + `ModelState.IsValid` + kiểm tra nghiệp vụ (trùng tên sản phẩm, trùng email, giá KM phải nhỏ hơn giá bán, ngày kết thúc ≥ ngày bắt đầu).
* **Ràng buộc khóa ngoại**: không cho xóa loại đang có sản phẩm; cảnh báo khi xóa sản phẩm đang nằm trong đơn chưa xử lý xong.
* **Phân quyền**: khách hàng gõ tay URL `/QuanLySanPham` sẽ bị đưa sang trang "Không có quyền truy cập".

---

## 4. Bước tiếp theo: nối Entity Framework 6 + SQL Server

1. **Cài EF6**: Tools → NuGet Package Manager → Package Manager Console:
   ```
   Install-Package EntityFramework -Version 6.4.4
   ```
   (Project hiện tại chưa có EF trong `packages.config`.)

2. **Tạo `Models/FoodHubContext.cs`**:
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

3. **Thêm connection string** vào `Web.config`:
   ```xml
   <connectionStrings>
     <add name="FoodHubConnection"
          connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=SugarChillsDB;Integrated Security=True;MultipleActiveResultSets=True"
          providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```

4. **Thay ruột của `KhoDuLieu`** bằng truy vấn LINQ trên `FoodHubContext`. Toàn bộ Controller/View giữ nguyên vì chúng chỉ gọi các hàm của `KhoDuLieu`.

   Lưu ý khi chuyển: `SanPham.GiaBanThucTe` và `PhanTramGiam` là `[NotMapped]` nên **không dùng trực tiếp trong câu LINQ gửi xuống SQL**. Hãy `ToList()` trước rồi mới lọc theo giá, hoặc lọc bằng biểu thức trên `Gia` / `GiaKhuyenMai`.

5. Mật khẩu hiện lưu dạng plain text cho dễ demo. Nếu muốn điểm cao hơn phần bảo mật, băm bằng SHA256 + salt trong `TaiKhoanController`.

---

## 5. Ảnh sản phẩm

Tất cả ảnh đang dùng link từ Unsplash và có `onerror` tự thay bằng ảnh placeholder màu hồng, nên giao diện không bao giờ bị vỡ dù link ảnh hỏng hoặc máy không có mạng.
Muốn dùng ảnh riêng: thả ảnh vào `Content/images/` rồi sửa trường **Hình ảnh** của sản phẩm thành `/Content/images/ten-anh.jpg`.

---

## 6. Ghi chú nhỏ

* Project **không dùng bundling** (`Scripts.Render` / `Styles.Render`). File CSS/JS được nhúng thẳng bằng thẻ `<link>` và `<script>` trong `_Layout.cshtml` và `_LayoutSeller.cshtml`. Lý do: `Scripts.Render` thêm một lớp trung gian dễ ném `NullReferenceException` khi bundle không phân giải được, lại khó debug; nhúng trực tiếp thì thấy ngay đường dẫn trong View Source. Lớp `BundleConfig` vẫn giữ (rỗng) để mở rộng sau.
* JS dùng `bootstrap.bundle.min.js` — bản đã kèm Popper, cần thiết để dropdown menu 2 cấp hoạt động.
* Hai view cũ `About.cshtml` / `Contact.cshtml` của template đã bỏ, thay bằng `GioiThieu.cshtml` và `LienHe.cshtml`.
* `Content/Site.css` chỉ còn vài dòng chỉnh sửa nhỏ, toàn bộ thiết kế nằm ở `Content/sugarchills.css`.
* Thư mục `packages` không đi kèm trong file nén (dung lượng lớn); Visual Studio sẽ tự khôi phục ở bước 3 phần "Cách chạy".
