using System;
using System.Collections.Generic;
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

        // ───── PHÂN TRANG DÙNG CHUNG ─────
        // Đặt ViewBag.TrangHienTai / TongSoTrang / TongSoDong / SoDongMoiTrang
        // để partial Views/Shared/_PhanTrang.cshtml vẽ thanh chuyển trang.

        /// <summary>Phân trang ngay trong SQL (COUNT + OFFSET/FETCH). Truy vấn PHẢI có OrderBy.</summary>
        protected List<T> PhanTrang<T>(IQueryable<T> truyVan, int trang, int soDongMoiTrang)
        {
            int tong = truyVan.Count();
            trang = GhiThongTinTrang(tong, trang, soDongMoiTrang);
            return truyVan.Skip((trang - 1) * soDongMoiTrang).Take(soDongMoiTrang).ToList();
        }

        /// <summary>Phân trang danh sách đã nằm trong bộ nhớ.</summary>
        protected List<T> PhanTrang<T>(IEnumerable<T> danhSach, int trang, int soDongMoiTrang)
        {
            var ds = danhSach as IList<T> ?? danhSach.ToList();
            trang = GhiThongTinTrang(ds.Count, trang, soDongMoiTrang);
            return ds.Skip((trang - 1) * soDongMoiTrang).Take(soDongMoiTrang).ToList();
        }

        /// <summary>Chuẩn hóa số trang (1..tổng) và đưa thông tin sang View. Trả về trang hợp lệ.</summary>
        private int GhiThongTinTrang(int tong, int trang, int soDongMoiTrang)
        {
            int tongSoTrang = Math.Max(1, (int)Math.Ceiling(tong / (double)soDongMoiTrang));
            if (trang < 1) trang = 1;
            if (trang > tongSoTrang) trang = tongSoTrang;

            ViewBag.TrangHienTai = trang;
            ViewBag.TongSoTrang = tongSoTrang;
            ViewBag.TongSoDong = tong;
            ViewBag.SoDongMoiTrang = soDongMoiTrang;
            return trang;
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
