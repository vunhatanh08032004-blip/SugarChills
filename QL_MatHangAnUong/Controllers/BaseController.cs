using System.Web.Mvc;
using QL_MatHangAnUong.Helpers;
using QL_MatHangAnUong.Models;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// Controller cha cho toàn bộ hệ thống.
    /// Nhiệm vụ: nạp sẵn các dữ liệu mà _Layout luôn cần (menu loại sản phẩm, số lượng giỏ hàng,
    /// người dùng đang đăng nhập) để View không phải tự truy vấn — giữ đúng nguyên tắc MVC.
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

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
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
