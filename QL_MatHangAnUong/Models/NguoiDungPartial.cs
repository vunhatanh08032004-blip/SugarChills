namespace QL_MatHangAnUong.Models
{
    /// <summary>
    /// Mở rộng entity NguoiDung (EDMX tự sinh) — thêm hằng số vai trò và helper.
    /// File này an toàn khi EDMX regenerate.
    /// </summary>
    public partial class NguoiDung
    {
        // ── Hằng số vai trò (phải khớp với giá trị lưu trong cột VaiTro của DB) ──
        public const string RoleCustomer = "Customer";
        public const string RoleSeller   = "Seller";

        /// <summary>True nếu tài khoản là Người bán (Seller).</summary>
        public bool LaNguoiBan => VaiTro == RoleSeller;

        /// <summary>True nếu tài khoản là Khách hàng (Customer).</summary>
        public bool LaKhachHang => VaiTro == RoleCustomer;
    }
}
