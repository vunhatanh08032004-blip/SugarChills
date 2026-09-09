using System.ComponentModel.DataAnnotations;

namespace QL_MatHangAnUong.Models
{
    /// <summary>
    /// Một chi nhánh/cửa hàng SugarChills — dùng cho trang "Hệ thống cửa hàng"
    /// (danh sách chi nhánh + bản đồ, lọc theo Tỉnh/Thành và Quận/Huyện).
    /// </summary>
    public class CuaHang
    {
        [Key]
        public int MaCH { get; set; }

        [Required(ErrorMessage = "Tên cửa hàng không được để trống")]
        [StringLength(150)]
        [Display(Name = "Tên cửa hàng")]
        public string TenCH { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành")]
        [Display(Name = "Tỉnh/Thành phố")]
        public string TinhThanh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Phường/Xã")]
        [Display(Name = "Phường/Xã")]
        // Tên trường (QuanHuyen) giữ nguyên để không phải sửa Controller/View, nhưng dữ liệu thực chứa tên Phường/Xã —
        // vì Việt Nam đã bãi bỏ cấp Quận/Huyện từ 1/7/2025, chỉ còn 2 cấp: Tỉnh/Thành phố → Phường/Xã.
        public string QuanHuyen { get; set; }

        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [Display(Name = "Giờ mở cửa")]
        public string GioMoCua { get; set; }

        /// <summary>Địa chỉ đầy đủ dùng để nhúng Google Maps (không cần API key): "DiaChi, QuanHuyen, TinhThanh".</summary>
        public string DiaChiDayDu
        {
            get { return string.Format("{0}, {1}, {2}", DiaChi, QuanHuyen, TinhThanh); }
        }
    }
}
