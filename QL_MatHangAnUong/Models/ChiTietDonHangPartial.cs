namespace QL_MatHangAnUong.Models
{
    /// <summary>
    /// Mở rộng entity ChiTietDonHang (EDMX tự sinh) — thêm computed property ThanhTien.
    /// File này an toàn khi EDMX regenerate.
    /// </summary>
    public partial class ChiTietDonHang
    {
        /// <summary>Thành tiền = Đơn giá × Số lượng (không lưu xuống DB, tính khi cần).</summary>
        public decimal ThanhTien => DonGia * SoLuong;
    }
}
