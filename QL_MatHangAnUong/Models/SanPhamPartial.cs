namespace QL_MatHangAnUong.Models
{
    public partial class SanPham
    {
        /// <summary>
        /// Giá hiển thị thực tế: dùng GiaKhuyenMai nếu có, ngược lại dùng Gia gốc.
        /// </summary>
        public decimal GiaBanThucTe =>
            GiaKhuyenMai.HasValue && GiaKhuyenMai.Value > 0
                ? GiaKhuyenMai.Value
                : Gia;
    }
}
