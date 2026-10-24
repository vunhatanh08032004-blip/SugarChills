using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Helpers;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// [NGƯỜI BÁN] Quản lý và cập nhật đơn hàng — rubric mục 7.1.
    /// </summary>
    [KiemTraNguoiBan]
    public class QuanLyDonHangController : BaseController
    {
        // GET: /QuanLyDonHang
        public ActionResult Index(string trangThai, string tuKhoa, DateTime? tuNgay, DateTime? denNgay)
        {
            var ds = db.DonHangs.Include(d => d.NguoiDung).OrderByDescending(d => d.NgayDat).ToList();

            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "Tất cả")
                ds = ds.Where(d => d.TrangThai == trangThai).ToList();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk = tuKhoa.Trim().ToLower();
                ds = ds.Where(d => d.MaDH.ToString().Contains(tk)
                                   || (d.TenNguoiNhan != null && d.TenNguoiNhan.ToLower().Contains(tk))
                                   || (d.DienThoai != null && d.DienThoai.Contains(tk))).ToList();
            }

            if (tuNgay.HasValue)
                ds = ds.Where(d => d.NgayDat.Date >= tuNgay.Value.Date).ToList();

            if (denNgay.HasValue)
                ds = ds.Where(d => d.NgayDat.Date <= denNgay.Value.Date).ToList();

            // Số liệu nhanh hiển thị ở đầu trang
            ViewBag.SoChoXacNhan = db.DonHangs.Count(d => d.TrangThai == DonHang.ChoXacNhan);
            ViewBag.SoDangGiao = db.DonHangs.Count(d => d.TrangThai == DonHang.DangGiao);
            ViewBag.SoHoanThanh = db.DonHangs.Count(d => d.TrangThai == DonHang.HoanThanh);
            ViewBag.SoDaHuy = db.DonHangs.Count(d => d.TrangThai == DonHang.DaHuy);

            ViewBag.TrangThaiDangChon = trangThai ?? "Tất cả";
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.Title = Ngu.S("Seller_QuanLyDonHang");

            return View(ds);
        }

        // GET: /QuanLyDonHang/ChiTiet/1001
        public ActionResult ChiTiet(int id)
        {
            var dh = db.DonHangs.Include(d => d.NguoiDung).Include(d => d.ChiTietDonHangs).FirstOrDefault(d => d.MaDH == id);
            if (dh == null) return HttpNotFound(Ngu.S("Order_KhongTimThayDonHang"));

            ViewBag.Title = string.Format(Ngu.S("Order_DonFormat"), dh.MaDH);
            return View(dh);
        }

        /// <summary>Cập nhật trạng thái đơn theo luồng xử lý của quán.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatTrangThai(int id, string trangThai, string quayVe)
        {
            var dh = db.DonHangs.Find(id);
            if (dh == null) return HttpNotFound(Ngu.S("Order_KhongTimThayDonHang"));

            if (!DonHang.CacTrangThai.Contains(trangThai))
            {
                ThongBao(Ngu.S("SellerOrder_TrangThaiKhongHopLe"), "danger");
                return RedirectToAction("Index");
            }

            // Quy tắc nghiệp vụ: đơn đã hoàn thành hoặc đã hủy thì không đổi trạng thái nữa
            if (dh.TrangThai == DonHang.HoanThanh || dh.TrangThai == DonHang.DaHuy)
            {
                ThongBao(string.Format(Ngu.S("SellerOrder_KhongTheDoiTiepFormat"),
                    dh.MaDH, dh.TrangThai), "warning");
                return RedirectToAction("ChiTiet", new { id = id });
            }

            dh.TrangThai = trangThai;
            db.SaveChanges();
            ThongBao(string.Format(Ngu.S("SellerOrder_DaChuyenSangFormat"), id, trangThai));

            if (quayVe == "chitiet") return RedirectToAction("ChiTiet", new { id = id });
            return RedirectToAction("Index");
        }
    }
}
