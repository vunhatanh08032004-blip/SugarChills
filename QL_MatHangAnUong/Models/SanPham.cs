using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_MatHangAnUong.Models
{
    /// <summary>
    /// Sản phẩm (món nước / món ăn) của SugarChills.
    /// </summary>
    public class SanPham
    {
        [Key]
        [Display(Name = "Mã SP")]
        public int MaSP { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(150, ErrorMessage = "Tên sản phẩm tối đa 150 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string TenSP { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại sản phẩm")]
        [Display(Name = "Loại sản phẩm")]
        public int MaLoai { get; set; }

        [Required(ErrorMessage = "Giá bán không được để trống")]
        [Range(1000, 10000000, ErrorMessage = "Giá bán phải từ 1.000đ đến 10.000.000đ")]
        [Display(Name = "Giá bán")]
        public decimal Gia { get; set; }

        /// <summary>Giá sau khi giảm. Null nghĩa là sản phẩm không giảm giá.</summary>
        [Range(0, 10000000, ErrorMessage = "Giá khuyến mãi không hợp lệ")]
        [Display(Name = "Giá khuyến mãi")]
        public decimal? GiaKhuyenMai { get; set; }

        [Display(Name = "Hình ảnh")]
        public string HinhAnh { get; set; }

        [StringLength(1000)]
        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Range(0, 100000, ErrorMessage = "Số lượng tồn không được âm")]
        [Display(Name = "Số lượng tồn")]
        public int SoLuongTon { get; set; }

        /// <summary>Trạng thái bán: true = Đang bán, false = Ngừng bán (dùng radio button ở form Seller).</summary>
        [Display(Name = "Trạng thái")]
        public bool DangBan { get; set; }

        /// <summary>Sản phẩm nổi bật, hiện ở trang chủ (dùng checkbox ở form Seller).</summary>
        [Display(Name = "Sản phẩm nổi bật")]
        public bool NoiBat { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; }

        [Display(Name = "Lượt bán")]
        public int LuotBan { get; set; }

        // Quan hệ n - 1 : nhiều sản phẩm thuộc một loại
        [ForeignKey("MaLoai")]
        public virtual LoaiSanPham LoaiSanPham { get; set; }

        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }

        public SanPham()
        {
            ChiTietDonHangs = new List<ChiTietDonHang>();
            NgayTao = DateTime.Now;
            DangBan = true;
            SoLuongTon = 100;
        }

        /// <summary>Giá thực tế khách phải trả (ưu tiên giá khuyến mãi nếu có).</summary>
        [NotMapped]
        public decimal GiaBanThucTe
        {
            get { return (GiaKhuyenMai.HasValue && GiaKhuyenMai.Value > 0) ? GiaKhuyenMai.Value : Gia; }
        }

        /// <summary>Phần trăm giảm giá để hiện nhãn "-20%" trên card sản phẩm.</summary>
        [NotMapped]
        public int PhanTramGiam
        {
            get
            {
                if (!GiaKhuyenMai.HasValue || GiaKhuyenMai.Value <= 0 || Gia <= 0) return 0;
                return (int)Math.Round((1 - GiaKhuyenMai.Value / Gia) * 100);
            }
        }
    }
}
