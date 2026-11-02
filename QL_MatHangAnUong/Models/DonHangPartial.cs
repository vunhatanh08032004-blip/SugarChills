namespace QL_MatHangAnUong.Models
{
    public partial class DonHang
    {
        // Phải trùng CHECK (TrangThai IN (...)) của bảng DonHang trong database
        public const string ChoXacNhan = "Chờ xác nhận";
        public const string DaXacNhan  = "Đã xác nhận";
        public const string DangGiao   = "Đang giao";
        public const string HoanThanh  = "Hoàn thành";
        public const string DaHuy      = "Đã hủy";

        public static readonly string[] CacTrangThai =
        {
            ChoXacNhan, DaXacNhan, DangGiao, HoanThanh, DaHuy
        };

        /// <summary>
        /// Trả về CSS class badge tương ứng với trạng thái đơn hàng.
        /// Dùng trong View: &lt;span class="sc-badge @dh.CssTrangThai"&gt;@dh.TrangThai&lt;/span&gt;
        /// </summary>
        public string CssTrangThai
        {
            get
            {
                switch (TrangThai)
                {
                    case ChoXacNhan: return "badge-warning";
                    case DaXacNhan:  return "badge-primary";
                    case DangGiao:   return "badge-info";
                    case HoanThanh:  return "badge-success";
                    case DaHuy:      return "badge-danger";
                    default:         return "badge-muted";
                }
            }
        }
    }
}
