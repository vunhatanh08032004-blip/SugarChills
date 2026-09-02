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

        public KhuyenMai()
        {
            KichHoat = true;
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

        /// <summary>Tính số tiền được giảm cho một giá trị đơn hàng.</summary>
        public decimal TinhTienGiam(decimal tamTinh)
        {
            if (!ConHieuLuc || tamTinh < DonToiThieu) return 0;

            decimal giam = tamTinh * PhanTramGiam / 100m;
            if (GiamToiDa > 0 && giam > GiamToiDa) giam = GiamToiDa;
            if (giam > tamTinh) giam = tamTinh;
            return Math.Round(giam);
        }
    }
}
