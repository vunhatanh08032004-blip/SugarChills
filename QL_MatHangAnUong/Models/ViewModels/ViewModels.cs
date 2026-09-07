using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QL_MatHangAnUong.Models.ViewModels
{
    /// <summary>Dữ liệu cho trang chủ.</summary>
    public class TrangChuViewModel
    {
        public List<LoaiSanPham> DanhSachLoai { get; set; }
        public List<SanPham> SanPhamNoiBat { get; set; }
        public List<SanPham> SanPhamMoi { get; set; }
        public List<SanPham> SanPhamGiamGia { get; set; }
        public List<KhuyenMai> KhuyenMais { get; set; }

        public TrangChuViewModel()
        {
            DanhSachLoai = new List<LoaiSanPham>();
            SanPhamNoiBat = new List<SanPham>();
            SanPhamMoi = new List<SanPham>();
            SanPhamGiamGia = new List<SanPham>();
            KhuyenMais = new List<KhuyenMai>();
        }
    }

    /// <summary>Dữ liệu cho trang danh sách sản phẩm (kèm tìm kiếm, lọc, sắp xếp, phân trang).</summary>
    public class DanhSachSanPhamViewModel
    {
        public List<SanPham> SanPhams { get; set; }
        public List<LoaiSanPham> DanhSachLoai { get; set; }

        public string TuKhoa { get; set; }
        public int? MaLoai { get; set; }
        public string TenLoaiDangChon { get; set; }
        public decimal? GiaTu { get; set; }
        public decimal? GiaDen { get; set; }
        public string SapXep { get; set; }

        /// <summary>
        /// True nếu cột bộ lọc (loại sản phẩm, khoảng giá, lọc nhanh) được hiển thị.
        /// Chỉ tắt khi khách vào từ các lối tắt "Xem thêm" ở trang chủ (Món nổi bật,
        /// Đang giảm giá, Món mới) mà chưa chọn danh mục nào. Một khi khách đã ở trong
        /// trang danh mục (đã hiện sidebar) thì việc đổi Sắp xếp hay bấm Lọc nhanh
        /// không được làm sidebar biến mất.
        /// </summary>
        public bool HienBoLoc { get; set; }

        /// <summary>
        /// Tên trang khi khách vào từ lối tắt trang chủ (Món nổi bật, Đang giảm giá,
        /// Món mới lên kệ) — ví dụ "Món mới lên kệ". Null khi duyệt bình thường theo
        /// Thực đơn / danh mục, lúc đó breadcrumb và tiêu đề dùng TenLoaiDangChon như cũ.
        /// </summary>
        public string TieuDeTrang { get; set; }

        public int TrangHienTai { get; set; }
        public int TongSoTrang { get; set; }
        public int TongSoSanPham { get; set; }

        public DanhSachSanPhamViewModel()
        {
            SanPhams = new List<SanPham>();
            DanhSachLoai = new List<LoaiSanPham>();
            TrangHienTai = 1;
            TongSoTrang = 1;
        }
    }

    /// <summary>Dữ liệu cho trang chi tiết sản phẩm.</summary>
    public class ChiTietSanPhamViewModel
    {
        public SanPham SanPham { get; set; }
        public List<SanPham> SanPhamLienQuan { get; set; }

        public ChiTietSanPhamViewModel()
        {
            SanPhamLienQuan = new List<SanPham>();
        }
    }

    /// <summary>Form đăng nhập.</summary>
    public class DangNhapViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool GhiNho { get; set; }
    }

    /// <summary>Form đăng ký tài khoản khách hàng.</summary>
    public class DangKyViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 số và bắt đầu bằng 0")]
        [Display(Name = "Số điện thoại")]
        public string DienThoai { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu")]
        public string XacNhanMatKhau { get; set; }
    }

    /// <summary>Form đặt hàng (thanh toán).</summary>
    public class DatHangViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100)]
        [Display(Name = "Người nhận")]
        public string TenNguoiNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 số và bắt đầu bằng 0")]
        [Display(Name = "Số điện thoại")]
        public string DienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(250)]
        [Display(Name = "Địa chỉ giao hàng")]
        public string DiaChi { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú cho quán")]
        public string GhiChu { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình thức thanh toán")]
        [Display(Name = "Hình thức thanh toán")]
        public string HinhThucThanhToan { get; set; }

        /// <summary>true khi khách chọn QR và JS xác nhận đã "quét mã" xong (mô phỏng cổng thanh toán).</summary>
        [Display(Name = "Đã thanh toán qua QR")]
        public bool DaThanhToanQR { get; set; }

        // Thông tin hiển thị lại ở cột tóm tắt đơn hàng
        public GioHang GioHang { get; set; }
        public decimal TienGiam { get; set; }
        public decimal PhiGiaoHang { get; set; }
        public decimal TongTien { get; set; }

        public DatHangViewModel()
        {
            HinhThucThanhToan = "COD";
        }
    }

    /// <summary>Dữ liệu cho trang thống kê doanh thu của người bán.</summary>
    public class ThongKeViewModel
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }

        public decimal TongDoanhThu { get; set; }
        public int TongDonHang { get; set; }
        public int DonHoanThanh { get; set; }
        public int DonChoXacNhan { get; set; }
        public int DonDaHuy { get; set; }
        public int TongSanPham { get; set; }
        public int TongKhachHang { get; set; }
        public decimal GiaTriTrungBinh { get; set; }

        /// <summary>Doanh thu theo từng ngày (dùng vẽ biểu đồ).</summary>
        public List<DoanhThuTheoNgay> DoanhThuNgay { get; set; }

        /// <summary>Doanh thu theo loại sản phẩm.</summary>
        public List<DoanhThuTheoLoai> DoanhThuLoai { get; set; }

        /// <summary>Top sản phẩm bán chạy.</summary>
        public List<SanPhamBanChay> TopBanChay { get; set; }

        public ThongKeViewModel()
        {
            DoanhThuNgay = new List<DoanhThuTheoNgay>();
            DoanhThuLoai = new List<DoanhThuTheoLoai>();
            TopBanChay = new List<SanPhamBanChay>();
        }
    }

    public class DoanhThuTheoNgay
    {
        public DateTime Ngay { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoDon { get; set; }
    }

    public class DoanhThuTheoLoai
    {
        public string TenLoai { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoLuong { get; set; }
    }

    public class SanPhamBanChay
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
    }
}