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
    /// [NGUOI BAN] Quan ly va cap nhat don hang - rubric muc 7.1.
    /// </summary>
    [KiemTraNguoiBan]
    public class QuanLyDonHangController : BaseController
    {
        // GET: /QuanLyDonHang
        public ActionResult Index(string trangThai, string tuKhoa, DateTime? tuNgay, DateTime? denNgay, int trang = 1)
        {
            // Filter trong DB thay vi load het roi loc trong memory
            var query = db.DonHangs.Include(d => d.NguoiDung).AsQueryable();

            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "Tat ca")
                query = query.Where(d => d.TrangThai == trangThai);

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk = tuKhoa.Trim();
                int maDH;
                bool isMaDH = int.TryParse(tk, out maDH);
                if (isMaDH)
                    query = query.Where(d => d.MaDH == maDH
                                          || (d.TenNguoiNhan != null && d.TenNguoiNhan.Contains(tk))
                                          || (d.DienThoai != null && d.DienThoai.Contains(tk)));
                else
                    query = query.Where(d => (d.TenNguoiNhan != null && d.TenNguoiNhan.Contains(tk))
                                          || (d.DienThoai != null && d.DienThoai.Contains(tk)));
            }

            if (tuNgay.HasValue)
            {
                DateTime start = tuNgay.Value.Date;
                query = query.Where(d => d.NgayDat >= start);
            }

            if (denNgay.HasValue)
            {
                DateTime end = denNgay.Value.Date.AddDays(1);
                query = query.Where(d => d.NgayDat < end);
            }

            // 10 đơn / trang, phân trang trong SQL
            var ds = PhanTrang(query.OrderByDescending(d => d.NgayDat), trang, 10);

            // So lieu nhanh hien thi o dau trang
            ViewBag.SoChoXacNhan = db.DonHangs.Count(d => d.TrangThai == DonHang.ChoXacNhan);
            ViewBag.SoDangGiao   = db.DonHangs.Count(d => d.TrangThai == DonHang.DangGiao);
            ViewBag.SoHoanThanh  = db.DonHangs.Count(d => d.TrangThai == DonHang.HoanThanh);
            ViewBag.SoDaHuy      = db.DonHangs.Count(d => d.TrangThai == DonHang.DaHuy);

            ViewBag.TrangThaiDangChon = trangThai ?? "Tat ca";
            ViewBag.TuKhoa  = tuKhoa;
            ViewBag.TuNgay  = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.Title   = Ngu.S("Seller_QuanLyDonHang");

            return View(ds);
        }

        // GET: /QuanLyDonHang/ChiTiet/1001
        public ActionResult ChiTiet(int id)
        {
            var dh = db.DonHangs
                .Include(d => d.NguoiDung)
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefault(d => d.MaDH == id);
            if (dh == null) return HttpNotFound(Ngu.S("Order_KhongTimThayDonHang"));

            ViewBag.Title = string.Format(Ngu.S("Order_DonFormat"), dh.MaDH);
            return View(dh);
        }

        /// <summary>Cap nhat trang thai don theo luong xu ly cua quan.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatTrangThai(int id, string trangThai, string quayVe)
        {
            // Include ChiTietDonHangs de co the hoan tra ton kho khi huy don
            var dh = db.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefault(d => d.MaDH == id);
            if (dh == null) return HttpNotFound(Ngu.S("Order_KhongTimThayDonHang"));

            if (!DonHang.CacTrangThai.Contains(trangThai))
            {
                ThongBao(Ngu.S("SellerOrder_TrangThaiKhongHopLe"), "danger");
                return RedirectToAction("Index");
            }

            // Quy tac nghiep vu: don da hoan thanh hoac da huy thi khong doi trang thai nua
            if (dh.TrangThai == DonHang.HoanThanh || dh.TrangThai == DonHang.DaHuy)
            {
                ThongBao(string.Format(Ngu.S("SellerOrder_KhongTheDoiTiepFormat"),
                    dh.MaDH, dh.TrangThai), "warning");
                return RedirectToAction("ChiTiet", new { id = id });
            }

            // [NGHIEP VU] Neu doi sang Da huy -> hoan tra ton kho cho tung san pham
            if (trangThai == DonHang.DaHuy)
            {
                foreach (var ct in dh.ChiTietDonHangs)
                {
                    var sp = db.SanPhams.Find(ct.MaSP);
                    if (sp != null)
                        sp.SoLuongTon += ct.SoLuong;
                }
            }

            dh.TrangThai = trangThai;
            db.SaveChanges();
            ThongBao(string.Format(Ngu.S("SellerOrder_DaChuyenSangFormat"), id, trangThai));

            if (quayVe == "chitiet") return RedirectToAction("ChiTiet", new { id = id });
            return RedirectToAction("Index");
        }
    }
}
