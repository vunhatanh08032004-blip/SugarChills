using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QL_MatHangAnUong.Models
{
    /// <summary>
    /// Loại sản phẩm (Trà sữa, Cà phê, Kem, Bánh ngọt...).
    /// Đây là POCO class dùng chung cho cả giai đoạn dữ liệu mẫu và giai đoạn Entity Framework Code First.
    /// </summary>
    public class LoaiSanPham
    {
        [Key]
        [Display(Name = "Mã loại")]
        public int MaLoai { get; set; }

        [Required(ErrorMessage = "Tên loại không được để trống")]
        [StringLength(100, ErrorMessage = "Tên loại tối đa 100 ký tự")]
        [Display(Name = "Tên loại")]
        public string TenLoai { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Hình ảnh")]
        public string HinhAnh { get; set; }

        [Display(Name = "Hiển thị trên menu")]
        public bool HienThi { get; set; }

        [Display(Name = "Thứ tự")]
        public int ThuTu { get; set; }

        // Quan hệ 1 - n : một loại có nhiều sản phẩm
        public virtual ICollection<SanPham> SanPhams { get; set; }

        public LoaiSanPham()
        {
            SanPhams = new List<SanPham>();
            HienThi = true;
        }
    }
}
