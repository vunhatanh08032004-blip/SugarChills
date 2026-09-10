using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Helpers;

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
            ViewBag.Title = Ngu.S("Header_DonHangCuaToi");
            return View(ds);
        }

        // GET: /DonHang/ChiTiet/1001
        public ActionResult ChiTiet(int id)
        {
            var dh = KhoDuLieu.LayDonHang(id);
            if (dh == null) return HttpNotFound(Ngu.S("Order_KhongTimThayDonHang"));

            // Khách chỉ được xem đơn của chính mình
            if (dh.MaND != NguoiDungHienTai.MaND)
                return new HttpUnauthorizedResult(Ngu.S("Order_KhongCoQuyenXem"));

            ViewBag.Title = string.Format(Ngu.S("Order_DonHangTitleFormat"), dh.MaDH);
            return View(dh);
        }

        /// <summary>Khách tự hủy đơn khi đơn còn ở trạng thái "Chờ xác nhận".</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HuyDon(int id)
        {
            var dh = KhoDuLieu.LayDonHang(id);
            if (dh == null) return HttpNotFound(Ngu.S("Order_KhongTimThayDonHang"));

            if (dh.MaND != NguoiDungHienTai.MaND)
                return new HttpUnauthorizedResult(Ngu.S("Order_KhongCoQuyenHuy"));

            if (dh.TrangThai != DonHang.ChoXacNhan)
            {
                ThongBao(Ngu.S("Order_KhongTheTuHuy"), "warning");
                return RedirectToAction("ChiTiet", new { id = id });
            }

            KhoDuLieu.CapNhatTrangThaiDonHang(id, DonHang.DaHuy);
            ThongBao(string.Format(Ngu.S("Order_DaHuyDonFormat"), id), "info");
            return RedirectToAction("Index");
        }
    }
}
