using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Helpers;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// [NGƯỜI BÁN] Quản lý chương trình khuyến mãi — rubric mục 7.2.
    /// Mã giảm giá tạo ở đây chính là mã khách nhập tại giỏ hàng.
    /// </summary>
    [KiemTraNguoiBan]
    public class QuanLyKhuyenMaiController : BaseController
    {
        // GET: /QuanLyKhuyenMai
        public ActionResult Index()
        {
            ViewBag.Title = Ngu.S("Seller_QuanLyKhuyenMai");
            return View(KhoDuLieu.LayKhuyenMais());
        }

        // GET: /QuanLyKhuyenMai/Them
        public ActionResult Them()
        {
            ViewBag.Title = Ngu.S("SellerKM_ThemTitle");
            return View(new KhuyenMai());
        }

        // POST: /QuanLyKhuyenMai/Them
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Them(KhuyenMai km)
        {
            KiemTraNghiepVu(km, null);

            if (!ModelState.IsValid)
            {
                ViewBag.Title = Ngu.S("SellerKM_ThemTitle");
                return View(km);
            }

            km.MaGiamGia = km.MaGiamGia.Trim().ToUpper();
            KhoDuLieu.ThemKhuyenMai(km);
            ThongBao(string.Format(Ngu.S("SellerKM_DaThemFormat"), km.TenKM));
            return RedirectToAction("Index");
        }

        // GET: /QuanLyKhuyenMai/Sua/1
        public ActionResult Sua(int id)
        {
            var km = KhoDuLieu.LayKhuyenMai(id);
            if (km == null) return HttpNotFound(Ngu.S("SellerKM_KhongTimThay"));

            ViewBag.Title = Ngu.S("SellerKM_SuaTitle");
            return View(km);
        }

        // POST: /QuanLyKhuyenMai/Sua/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sua(KhuyenMai km)
        {
            KiemTraNghiepVu(km, km.MaKM);

            if (!ModelState.IsValid)
            {
                ViewBag.Title = Ngu.S("SellerKM_SuaTitle");
                return View(km);
            }

            km.MaGiamGia = km.MaGiamGia.Trim().ToUpper();
            if (!KhoDuLieu.CapNhatKhuyenMai(km))
                return HttpNotFound(Ngu.S("SellerKM_KhongTimThaySuaCanXoa"));

            ThongBao(string.Format(Ngu.S("SellerKM_DaCapNhatFormat"), km.TenKM));
            return RedirectToAction("Index");
        }

        // GET: /QuanLyKhuyenMai/Xoa/1
        public ActionResult Xoa(int id)
        {
            var km = KhoDuLieu.LayKhuyenMai(id);
            if (km == null) return HttpNotFound(Ngu.S("SellerKM_KhongTimThay"));

            ViewBag.Title = Ngu.S("SellerKM_XoaTitle");
            return View(km);
        }

        // POST: /QuanLyKhuyenMai/XacNhanXoa/1
        [HttpPost, ActionName("XacNhanXoa")]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanXoa(int id)
        {
            if (!KhoDuLieu.XoaKhuyenMai(id))
            {
                ThongBao(Ngu.S("SellerKM_KhongTimThay"), "danger");
                return RedirectToAction("Index");
            }

            ThongBao(Ngu.S("SellerKM_DaXoa"), "info");
            return RedirectToAction("Index");
        }

        /// <summary>Bật/tắt nhanh một chương trình ngay tại bảng danh sách.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiTrangThai(int id)
        {
            var km = KhoDuLieu.LayKhuyenMai(id);
            if (km == null) return HttpNotFound(Ngu.S("SellerKM_KhongTimThay"));

            km.KichHoat = !km.KichHoat;
            KhoDuLieu.CapNhatKhuyenMai(km);

            ThongBao(string.Format(Ngu.S("SellerKM_DaDuocFormat"),
                km.TenKM, km.KichHoat ? Ngu.S("SellerKM_Bat") : Ngu.S("SellerKM_Tat")), "info");
            return RedirectToAction("Index");
        }

        private void KiemTraNghiepVu(KhuyenMai km, int? boQuaMaKM)
        {
            if (!string.IsNullOrWhiteSpace(km.MaGiamGia) &&
                KhoDuLieu.MaGiamGiaDaTonTai(km.MaGiamGia, boQuaMaKM))
            {
                ModelState.AddModelError("MaGiamGia", Ngu.S("SellerKM_MaTrung"));
            }

            if (km.NgayKetThuc.Date < km.NgayBatDau.Date)
                ModelState.AddModelError("NgayKetThuc", Ngu.S("SellerKM_NgayKetThucSaiThuTu"));

            if (km.PhanTramGiam <= 0)
                ModelState.AddModelError("PhanTramGiam", Ngu.S("SellerKM_PhanTramPhaiLonHon0"));
        }
    }
}
