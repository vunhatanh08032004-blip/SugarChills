using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_MatHangAnUong.Models
{
    /// <summary>
    /// Chương trình khuyến mãi. Khách nhập MaGiamGia ở giỏ hàng để được giảm tiền.
    /// </summary>
    public class KhuyenMai
    {
        [Key]
        [Display(Name = "Mã KM")]
        public int MaKM { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên chương trình")]
        [StringLength(150)]
        [Display(Name = "Tên chương trình")]
        public string TenKM { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã giảm giá")]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Mã giảm giá chỉ gồm chữ và số, không dấu, không khoảng trắng")]
        [Display(Name = "Mã giảm giá")]
        public string MaGiamGia { get; set; }

        [Range(0, 100, ErrorMessage = "Phần trăm giảm từ 0 đến 100")]
        [Display(Name = "Giảm (%)")]
        public int PhanTramGiam { get; set; }

        [Range(0, 10000000, ErrorMessage = "Số tiền giảm không hợp lệ")]
        [Display(Name = "Giảm tối đa (đ)")]
        public decimal GiamToiDa { get; set; }

        [Range(0, 10000000, ErrorMessage = "Giá trị đơn tối thiểu không hợp lệ")]
        [Display(Name = "Đơn tối thiểu (đ)")]
        public decimal DonToiThieu { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime NgayBatDau { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày kết thúc")]
        public DateTime NgayKetThuc { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool KichHoat { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại giảm giá")]
        [Display(Name = "Áp dụng giảm cho")]
        public string LoaiGiam { get; set; }

        /// <summary>Giảm trên tổng tiền hàng (tạm tính).</summary>
        public const string GiamTongTien = "TongTien";

        /// <summary>Giảm trên phí giao hàng (phí thu khi nhận hàng / COD) — dùng cho các mã kiểu "Freeship".</summary>
        public const string GiamPhiGiaoHang = "PhiGiaoHang";

        public static readonly string[] CacLoaiGiam = { GiamTongTien, GiamPhiGiaoHang };

        /// <summary>Tên hiển thị của loại giảm, dùng ở danh sách quản lý và trang chủ.</summary>
        [NotMapped]
        public string TenLoaiGiam
        {
            get { return LoaiGiam == GiamPhiGiaoHang ? "Giảm phí giao hàng (COD)" : "Giảm tổng tiền"; }
        }

        public KhuyenMai()
        {
            KichHoat = true;
            LoaiGiam = GiamTongTien;
            NgayBatDau = DateTime.Today;
            NgayKetThuc = DateTime.Today.AddMonths(1);
        }

        /// <summary>Khuyến mãi có đang trong thời gian áp dụng hay không.</summary>
        [NotMapped]
        public bool ConHieuLuc
        {
            get
            {
                return KichHoat
                       && DateTime.Today >= NgayBatDau.Date
                       && DateTime.Today <= NgayKetThuc.Date;
            }
        }

        /// <summary>
        /// Tính số tiền được giảm. Với mã "Giảm tổng tiền" thì % áp dụng trên tạm tính (tamTinh);
        /// với mã "Giảm phí giao hàng" thì % áp dụng trên chính phí giao hàng (phiGiaoHang) —
        /// ví dụ mã Freeship 100% sẽ làm phí giao hàng về 0.
        /// Điều kiện "Đơn tối thiểu" luôn xét trên tạm tính, không phụ thuộc loại giảm.
        /// </summary>
        public decimal TinhTienGiam(decimal tamTinh, decimal phiGiaoHang = 0)
        {
            if (!ConHieuLuc || tamTinh < DonToiThieu) return 0;

            decimal coSo = LoaiGiam == GiamPhiGiaoHang ? phiGiaoHang : tamTinh;
            if (coSo <= 0) return 0;

            decimal giam = coSo * PhanTramGiam / 100m;
            if (GiamToiDa > 0 && giam > GiamToiDa) giam = GiamToiDa;
            if (giam > coSo) giam = coSo;
            return Math.Round(giam);
        }
    }
}
