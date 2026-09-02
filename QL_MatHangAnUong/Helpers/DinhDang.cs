using System;
using System.Globalization;

namespace QL_MatHangAnUong.Helpers
{
    /// <summary>
    /// Các hàm định dạng dùng chung cho View (tiền tệ, ngày giờ).
    /// Đặt ở đây để không phải lặp lại string.Format trong từng .cshtml.
    /// </summary>
    public static class DinhDang
    {
        private static readonly CultureInfo VN = new CultureInfo("vi-VN");

        /// <summary>39000 -> "39.000đ"</summary>
        public static string Tien(decimal soTien)
        {
            return soTien.ToString("#,##0", VN) + "đ";
        }

        public static string Tien(decimal? soTien)
        {
            return soTien.HasValue ? Tien(soTien.Value) : "-";
        }

        /// <summary>39000 -> "39.000" (không có ký hiệu đ)</summary>
        public static string So(decimal soTien)
        {
            return soTien.ToString("#,##0", VN);
        }

        public static string NgayGio(DateTime d)
        {
            return d.ToString("dd/MM/yyyy HH:mm", VN);
        }

        public static string Ngay(DateTime d)
        {
            return d.ToString("dd/MM/yyyy", VN);
        }
    }
}
