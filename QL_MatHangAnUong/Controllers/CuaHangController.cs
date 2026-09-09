using System.Web.Mvc;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Helpers;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// Trang "Hệ thống cửa hàng": danh sách chi nhánh trên cả nước + bản đồ,
    /// lọc theo Tỉnh/Thành và Phường/Xã (2 dropdown phụ thuộc nhau, đúng mô hình 2 cấp hiện hành
    /// sau khi cả nước bãi bỏ cấp Quận/Huyện từ 1/7/2025). Ai cũng xem được.
    /// </summary>
    public class CuaHangController : BaseController
    {
        // GET: /CuaHang?tinhThanh=...&quanHuyen=...
        public ActionResult Index(string tinhThanh, string quanHuyen)
        {
            var ds = KhoDuLieu.LayCuaHangs(tinhThanh, quanHuyen);

            ViewBag.DanhSachTinhThanh = KhoDuLieu.LayDanhSachTinhThanh();
            ViewBag.DanhSachQuanHuyen = KhoDuLieu.LayQuanHuyenTheoTinh(tinhThanh);
            ViewBag.TinhThanhChon = tinhThanh;
            ViewBag.QuanHuyenChon = quanHuyen;
            ViewBag.Title = Ngu.S("Footer_HeThongCuaHang");

            return View(ds);
        }
    }
}
