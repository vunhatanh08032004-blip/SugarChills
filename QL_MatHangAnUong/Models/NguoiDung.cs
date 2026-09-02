using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QL_MatHangAnUong.Models
{
    /// <summary>
    /// Tài khoản người dùng. Hệ thống chỉ có 2 vai trò: "Customer" và "Seller".
    /// Khách vãng lai (Guest) là người CHƯA đăng nhập, không phải một role trong bảng này.
    /// </summary>
    public class NguoiDung
    {
        public const string RoleCustomer = "Customer";
        public const string RoleSeller = "Seller";

        [Key]
        [Display(Name = "Mã người dùng")]
        public int MaND { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 số và bắt đầu bằng 0")]
        [Display(Name = "Số điện thoại")]
        public string DienThoai { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; }

        [Display(Name = "Ngày đăng ký")]
        public DateTime NgayDangKy { get; set; }

        public virtual ICollection<DonHang> DonHangs { get; set; }

        public NguoiDung()
        {
            DonHangs = new List<DonHang>();
            VaiTro = RoleCustomer;
            NgayDangKy = DateTime.Now;
        }

        public bool LaNguoiBan
        {
            get { return VaiTro == RoleSeller; }
        }
    }
}
