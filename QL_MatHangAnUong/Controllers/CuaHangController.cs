using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Helpers;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// Trang "Hệ thống cửa hàng": danh sách chi nhánh trên cả nước + bản đồ,
    /// lọc theo Tỉnh/Thành và Phường/Xã (2 dropdown phụ thuộc nhau).
    /// Ai cũng xem được. Dùng db kế thừa từ BaseController.
    /// </summary>
    public class CuaHangController : BaseController
    {
        // GET: /CuaHang?tinhThanh=...&quanHuyen=...
        public ActionResult Index(string tinhThanh, string quanHuyen)
        {
            // Lấy danh sách cửa hàng, lọc theo tỉnh/thành và quận/huyện nếu được chọn
            var query = db.CuaHangs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tinhThanh))
                query = query.Where(c => c.TinhThanh == tinhThanh);

            if (!string.IsNullOrWhiteSpace(quanHuyen))
                query = query.Where(c => c.QuanHuyen == quanHuyen);

            var ds = query.OrderBy(c => c.TenCH).ToList();

            // Lấy danh sách tỉnh/thành không trùng để đổ vào dropdown
            var danhSachTinhThanh = db.CuaHangs
                .Select(c => c.TinhThanh)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            // Lấy danh sách quận/huyện của tỉnh/thành đang chọn (cho dropdown cấp 2)
            var danhSachQuanHuyen = string.IsNullOrWhiteSpace(tinhThanh)
                ? new System.Collections.Generic.List<string>()
                : db.CuaHangs
                    .Where(c => c.TinhThanh == tinhThanh)
                    .Select(c => c.QuanHuyen)
                    .Distinct()
                    .OrderBy(q => q)
                    .ToList();

            ViewBag.DanhSachTinhThanh = danhSachTinhThanh;
            ViewBag.DanhSachQuanHuyen = danhSachQuanHuyen;
            ViewBag.TinhThanhChon     = tinhThanh;
            ViewBag.QuanHuyenChon     = quanHuyen;
            ViewBag.Title             = Ngu.S("Footer_HeThongCuaHang");

            return View(ds);
        }
    }
}
