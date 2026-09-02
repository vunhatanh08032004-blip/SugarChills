using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_MatHangAnUong.Models
{
    /// <summary>
    /// Chi tiết đơn hàng: mỗi dòng là 1 sản phẩm trong đơn, kèm tùy chọn size/đường/đá/topping.
    /// </summary>
    public class ChiTietDonHang
    {
        [Key]
        public int MaCT { get; set; }

        [Display(Name = "Mã đơn hàng")]
        public int MaDH { get; set; }

        [Display(Name = "Mã sản phẩm")]
        public int MaSP { get; set; }

        [StringLength(150)]
        [Display(Name = "Tên sản phẩm")]
        public string TenSP { get; set; }

        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; }

        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        [StringLength(200)]
        [Display(Name = "Tùy chọn")]
        public string TuyChon { get; set; }

        [ForeignKey("MaDH")]
        public virtual DonHang DonHang { get; set; }

        [ForeignKey("MaSP")]
        public virtual SanPham SanPham { get; set; }

        [NotMapped]
        public decimal ThanhTien
        {
            get { return DonGia * SoLuong; }
        }
    }
}
