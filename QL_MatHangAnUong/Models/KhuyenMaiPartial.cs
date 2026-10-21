using System;

namespace QL_MatHangAnUong.Models
{
    public partial class KhuyenMai
    {
        public const string GiamTongTien    = "tong_tien";
        public const string GiamPhiGiaoHang = "phi_giao_hang";

        /// <summary>Khuyến mãi còn trong thời hạn và được kích hoạt.</summary>
        public bool ConHieuLuc =>
            KichHoat &&
            NgayBatDau.Date <= DateTime.Now.Date &&
            NgayKetThuc.Date >= DateTime.Now.Date;

        /// <summary>Nhãn loại giảm dùng trong View.</summary>
        public string TenLoaiGiam =>
            LoaiGiam == GiamPhiGiaoHang ? "Phí giao hàng" : "Tổng tiền";

        /// <summary>
        /// Tính số tiền được giảm.
        /// </summary>
        /// <param name="tamTinh">Tạm tính trước khi giảm (không gồm phí vận chuyển).</param>
        /// <param name="phiGiaoHang">Phí giao hàng.</param>
        /// <returns>Số tiền được giảm (0 nếu không còn hiệu lực hoặc không đủ điều kiện).</returns>
        public decimal TinhTienGiam(decimal tamTinh, decimal phiGiaoHang)
        {
            if (!ConHieuLuc) return 0;
            if (DonToiThieu > 0 && tamTinh < DonToiThieu) return 0;

            decimal giamTren = LoaiGiam == GiamPhiGiaoHang ? phiGiaoHang : tamTinh;
            decimal tienGiam = giamTren * PhanTramGiam / 100m;

            if (GiamToiDa > 0 && tienGiam > GiamToiDa)
                tienGiam = GiamToiDa;

            return tienGiam;
        }
    }
}
