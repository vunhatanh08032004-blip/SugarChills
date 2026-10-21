using System.Data.Entity;
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
    /// Dùng db kế thừa từ BaseController.
    /// </summary>
    [KiemTraDangNhap]
    public class DonHangController : BaseController
    {
        // GET: /DonHang
        public ActionResult Index(string trangThai)
        {
            var maNd = NguoiDungHienTai.MaND;

            // Lấy tất cả đơn của người dùng hiện tại, mới nhất trước
            var ds = db.DonHangs
                .Where(d => d.MaND == maNd)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            // Lọc theo trạng thái nếu người dùng chọn
            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "Tất cả")
                ds = ds.Where(d => d.TrangThai == trangThai).ToList();

            ViewBag.TrangThaiDangChon = trangThai ?? "Tất cả";
            ViewBag.Title = Ngu.S("Header_DonHangCuaToi");
            return View(ds);
        }

        // GET: /DonHang/ChiTiet/1001
        public ActionResult ChiTiet(int id)
        {
            // Include ChiTietDonHangs để load chi tiết đơn hàng trong 1 query
            var dh = db.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefault(d => d.MaDH == id);

            if (dh == null)
                return HttpNotFound(Ngu.S("Order_KhongTimThayDonHang"));

            // Khách chỉ được xem đơn của chính mình
            if (dh.MaND != NguoiDungHienTai.MaND)
                return new HttpUnauthorizedResult(Ngu.S("Order_KhongCoQuyenXem"));

            ViewBag.Title = string.Format(Ngu.S("Order_DonHangTitleFormat"), dh.MaDH);
            return View(dh);
        }

        ///Khách tự hủy đơn khi đơn còn ở trạng thái "Chờ xác nhận".>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HuyDon(int id)
        {
            var dh = db.DonHangs.FirstOrDefault(d => d.MaDH == id);
            if (dh == null)
                return HttpNotFound(Ngu.S("Order_KhongTimThayDonHang"));

            // Kiểm tra quyền: chỉ được hủy đơn của chính mình
            if (dh.MaND != NguoiDungHienTai.MaND)
                return new HttpUnauthorizedResult(Ngu.S("Order_KhongCoQuyenHuy"));

            // Chỉ hủy được khi đơn đang ở trạng thái "Chờ xác nhận"
            if (dh.TrangThai != DonHang.ChoXacNhan)
            {
                ThongBao(Ngu.S("Order_KhongTheTuHuy"), "warning");
                return RedirectToAction("ChiTiet", new { id = id });
            }

            // Cập nhật trạng thái sang "Đã hủy" và lưu xuống DB
            dh.TrangThai = DonHang.DaHuy;
            db.Entry(dh).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            ThongBao(string.Format(Ngu.S("Order_DaHuyDonFormat"), id), "info");
            return RedirectToAction("Index");
        }
    }
}
