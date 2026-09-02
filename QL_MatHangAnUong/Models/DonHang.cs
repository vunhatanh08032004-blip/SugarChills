using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_MatHangAnUong.Models
{
    /// <summary>
    /// Đơn hàng của khách. Trạng thái đi theo luồng:
    /// Chờ xác nhận -> Đã xác nhận -> Đang giao -> Hoàn thành (hoặc Đã hủy).
    /// </summary>
    public class DonHang
    {
        public const string ChoXacNhan = "Chờ xác nhận";
        public const string DaXacNhan = "Đã xác nhận";
        public const string DangGiao = "Đang giao";
        public const string HoanThanh = "Hoàn thành";
        public const string DaHuy = "Đã hủy";

        /// <summary>Danh sách trạng thái dùng cho dropdown ở trang quản lý đơn hàng.</summary>
        public static readonly string[] CacTrangThai =
        {
            ChoXacNhan, DaXacNhan, DangGiao, HoanThanh, DaHuy
        };

        [Key]
        [Display(Name = "Mã đơn hàng")]
        public int MaDH { get; set; }

        [Display(Name = "Khách hàng")]
        public int? MaND { get; set; }

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
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Hình thức thanh toán")]
        public string HinhThucThanhToan { get; set; }

        [Display(Name = "Ngày đặt")]
        public DateTime NgayDat { get; set; }

        [Display(Name = "Tạm tính")]
        public decimal TamTinh { get; set; }

        [Display(Name = "Giảm giá")]
        public decimal TienGiam { get; set; }

        [Display(Name = "Phí giao hàng")]
        public decimal PhiGiaoHang { get; set; }

        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; }

        [Display(Name = "Mã khuyến mãi")]
        [StringLength(50)]
        public string MaGiamGiaApDung { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; }

        [ForeignKey("MaND")]
        public virtual NguoiDung NguoiDung { get; set; }

        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }

        public DonHang()
        {
            ChiTietDonHangs = new List<ChiTietDonHang>();
            NgayDat = DateTime.Now;
            TrangThai = ChoXacNhan;
            HinhThucThanhToan = "COD";
        }

        /// <summary>Lớp CSS badge tương ứng trạng thái, dùng cho View.</summary>
        [NotMapped]
        public string CssTrangThai
        {
            get
            {
                switch (TrangThai)
                {
                    case DaXacNhan: return "badge-info";
                    case DangGiao: return "badge-primary";
                    case HoanThanh: return "badge-success";
                    case DaHuy: return "badge-danger";
                    default: return "badge-warning";
                }
            }
        }
    }
}
