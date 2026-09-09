using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using QL_MatHangAnUong.Helpers;
using QL_MatHangAnUong.Models;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// Controller cha cho toàn bộ hệ thống.
    /// Nhiệm vụ: nạp sẵn các dữ liệu mà _Layout luôn cần (menu loại sản phẩm, số lượng giỏ hàng,
    /// người dùng đang đăng nhập, ngôn ngữ hiển thị) để View không phải tự truy vấn — giữ đúng
    /// nguyên tắc MVC.
    /// </summary>
    public class BaseController : Controller
    {
        /// <summary>Người dùng đang đăng nhập, null nếu là khách vãng lai.</summary>
        protected NguoiDung NguoiDungHienTai
        {
            get { return PhienLamViec.LayNguoiDung(Session); }
        }

        /// <summary>Giỏ hàng trong Session.</summary>
        protected GioHang GioHangHienTai
        {
            get { return PhienLamViec.LayGioHang(Session); }
        }

        /// <summary>Mã ngôn ngữ đang hiển thị: "vi" (mặc định) hoặc "en" — đọc từ cookie "scNgonNgu".</summary>
        protected string NgonNguHienTai { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Đổi ngôn ngữ toàn site: đọc lựa chọn đã lưu (cookie do NgonNguController ghi) và
            // đặt CurrentUICulture cho request này. Mọi @Resources.Strings.Xxx trong View (kể cả
            // trong _Layout) sẽ tự động trả về đúng bản dịch mà không cần sửa từng Controller.
            var cookieNgonNgu = Request.Cookies["scNgonNgu"];
            NgonNguHienTai = (cookieNgonNgu != null && cookieNgonNgu.Value == "en") ? "en" : "vi";

            var vanHoa = new CultureInfo(NgonNguHienTai == "en" ? "en-US" : "vi-VN");
            Thread.CurrentThread.CurrentCulture = vanHoa;
            Thread.CurrentThread.CurrentUICulture = vanHoa;
            ViewBag.NgonNguHienTai = NgonNguHienTai;

            ViewBag.NguoiDungHienTai = NguoiDungHienTai;
            ViewBag.MenuLoai = KhoDuLieu.LayLoaiHienThi();
            ViewBag.SoLuongGioHang = GioHangHienTai.TongSoLuong;

            base.OnActionExecuting(filterContext);
        }

        /// <summary>Đặt thông báo hiển thị 1 lần ở đầu trang kế tiếp.</summary>
        protected void ThongBao(string noiDung, string loai = "success")
        {
            TempData["ThongBao"] = noiDung;
            TempData["LoaiThongBao"] = loai;   // success | danger | warning | info
        }
    }
}
