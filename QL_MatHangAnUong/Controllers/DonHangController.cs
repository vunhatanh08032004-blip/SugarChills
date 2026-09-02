using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Models;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// Đơn hàng nhìn từ phía KHÁCH HÀNG: xem danh sách đơn của mình và theo dõi trạng thái.
    /// (Phía người bán dùng QuanLyDonHangController.)
    /// </summary>
    [KiemTraDangNhap]
    public class DonHangController : BaseController
    {
        // GET: /DonHang
        public ActionResult Index(string trangThai)
        {
            var ds = KhoDuLieu.LayDonHangCuaKhach(NguoiDungHienTai.MaND);

            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "Tất cả")
                ds = ds.Where(d => d.TrangThai == trangThai).ToList();

            ViewBag.TrangThaiDangChon = trangThai ?? "Tất cả";
            ViewBag.Title = "Đơn hàng của tôi";
            return View(ds);
        }

        // GET: /DonHang/ChiTiet/1001
        public ActionResult ChiTiet(int id)
        {
            var dh = KhoDuLieu.LayDonHang(id);
            if (dh == null) return HttpNotFound("Không tìm thấy đơn hàng.");

            // Khách chỉ được xem đơn của chính mình
            if (dh.MaND != NguoiDungHienTai.MaND)
                return new HttpUnauthorizedResult("Bạn không có quyền xem đơn hàng này.");

            ViewBag.Title = "Đơn hàng #" + dh.MaDH;
            return View(dh);
        }

        /// <summary>Khách tự hủy đơn khi đơn còn ở trạng thái "Chờ xác nhận".</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HuyDon(int id)
        {
            var dh = KhoDuLieu.LayDonHang(id);
            if (dh == null) return HttpNotFound("Không tìm thấy đơn hàng.");

            if (dh.MaND != NguoiDungHienTai.MaND)
                return new HttpUnauthorizedResult("Bạn không có quyền hủy đơn hàng này.");

            if (dh.TrangThai != DonHang.ChoXacNhan)
            {
                ThongBao("Đơn đã được xác nhận nên không thể tự hủy. Vui lòng liên hệ 1900 6789.", "warning");
                return RedirectToAction("ChiTiet", new { id = id });
            }

            KhoDuLieu.CapNhatTrangThaiDonHang(id, DonHang.DaHuy);
            ThongBao("Đã hủy đơn hàng #" + id + ".", "info");
            return RedirectToAction("Index");
        }
    }
}
