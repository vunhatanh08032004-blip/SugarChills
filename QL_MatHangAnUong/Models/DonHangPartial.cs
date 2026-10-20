namespace QL_MatHangAnUong.Models
{
    public partial class DonHang
    {
        public const string ChoXacNhan = "Chờ xác nhận";
        public const string DangGiao   = "Đang giao";
        public const string HoanThanh  = "Hoàn thành";
        public const string DaHuy      = "Đã hủy";

        public static readonly string[] CacTrangThai =
        {
            ChoXacNhan, DangGiao, HoanThanh, DaHuy
        };
    }
}
