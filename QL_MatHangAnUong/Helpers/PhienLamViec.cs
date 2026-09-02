using System.Web;
using QL_MatHangAnUong.Models;

namespace QL_MatHangAnUong.Helpers
{
    /// <summary>
    /// Gom toàn bộ thao tác với Session vào một chỗ để tránh gõ chuỗi "GioHang", "NguoiDung" rải rác trong code.
    /// </summary>
    public static class PhienLamViec
    {
        public const string KeyNguoiDung = "NguoiDungDangNhap";
        public const string KeyGioHang = "GioHang";

        public static NguoiDung LayNguoiDung(HttpSessionStateBase session)
        {
            if (session == null) return null;
            return session[KeyNguoiDung] as NguoiDung;
        }

        public static void DangNhap(HttpSessionStateBase session, NguoiDung nd)
        {
            session[KeyNguoiDung] = nd;
        }

        public static void DangXuat(HttpSessionStateBase session)
        {
            session.Remove(KeyNguoiDung);
            session.Remove(KeyGioHang);
        }

        public static bool DaDangNhap(HttpSessionStateBase session)
        {
            return LayNguoiDung(session) != null;
        }

        public static bool LaNguoiBan(HttpSessionStateBase session)
        {
            var nd = LayNguoiDung(session);
            return nd != null && nd.VaiTro == NguoiDung.RoleSeller;
        }

        /// <summary>Lấy giỏ hàng trong Session, chưa có thì tạo mới.</summary>
        public static GioHang LayGioHang(HttpSessionStateBase session)
        {
            var gio = session[KeyGioHang] as GioHang;
            if (gio == null)
            {
                gio = new GioHang();
                session[KeyGioHang] = gio;
            }
            return gio;
        }

        public static void LuuGioHang(HttpSessionStateBase session, GioHang gio)
        {
            session[KeyGioHang] = gio;
        }
    }
}
