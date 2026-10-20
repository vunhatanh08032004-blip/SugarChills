using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using QL_MatHangAnUong.Helpers;
using QL_MatHangAnUong.Models;

namespace QL_MatHangAnUong.Controllers
{
 
    public class BaseController : Controller
    {
        protected QL_MatHangAnUongEntities db = new QL_MatHangAnUongEntities();

        /// Người dùng đang đăng nhập, null nếu là khách vãng lai.
        protected NguoiDung NguoiDungHienTai
        {
            get { return PhienLamViec.LayNguoiDung(Session); }
        }

        /// Giỏ hàng trong Session.
        protected GioHang GioHangHienTai
        {
            get { return PhienLamViec.LayGioHang(Session); }
        }

        /// Mã ngôn ngữ đang hiển thị: "vi" (mặc định) hoặc "en" — đọc từ cookie "scNgonNgu".
        protected string NgonNguHienTai { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Đổi ngôn ngữ toàn site
            var cookieNgonNgu = Request.Cookies["scNgonNgu"];
            NgonNguHienTai = (cookieNgonNgu != null && cookieNgonNgu.Value == "en") ? "en" : "vi";

            var vanHoa = new CultureInfo(NgonNguHienTai == "en" ? "en-US" : "vi-VN");
            Thread.CurrentThread.CurrentCulture = vanHoa;
            Thread.CurrentThread.CurrentUICulture = vanHoa;
            ViewBag.NgonNguHienTai = NgonNguHienTai;

            ViewBag.NguoiDungHienTai = NguoiDungHienTai;

            // Menu loại sản phẩm: lấy từ DB thay vì KhoDuLieu
            ViewBag.MenuLoai = db.LoaiSanPhams
                .Where(l => l.HienThi)
                .OrderBy(l => l.ThuTu)
                .ToList();

            ViewBag.SoLuongGioHang = GioHangHienTai.TongSoLuong;

            base.OnActionExecuting(filterContext);
        }

        ///Đặt thông báo hiển thị 1 lần ở đầu trang kế tiếp.
        protected void ThongBao(string noiDung, string loai = "success")
        {
            TempData["ThongBao"] = noiDung;
            TempData["LoaiThongBao"] = loai;   // success | danger | warning | info
        }

        /// Giải phóng DbContext khi request kết thúc.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
