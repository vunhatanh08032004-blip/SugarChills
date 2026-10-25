using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Helpers;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// [NGƯỜI BÁN] Quản lý khuyến mãi — làm việc trực tiếp với bảng KhuyenMai qua DbContext (biến db của BaseController).
    /// Mã giảm giá tạo ở đây chính là mã khách nhập tại giỏ hàng.
    /// </summary>
    [KiemTraNguoiBan]
    public class QuanLyKhuyenMaiController : BaseController
    {
        private new QL_MatHangAnUongEntities db = new QL_MatHangAnUongEntities();

        // Giá trị cột LoaiGiam, khớp CHECK (LoaiGiam IN (N'TongTien', N'PhiGiaoHang')) của bảng KhuyenMai
        private const string LoaiTongTien = "TongTien";
        private const string LoaiPhiGiaoHang = "PhiGiaoHang";

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        // GET: /QuanLyKhuyenMai
        public ActionResult Index()
        {
            ViewBag.Title = Ngu.S("Seller_QuanLyKhuyenMai");
            var ds = db.KhuyenMais.OrderByDescending(k => k.NgayBatDau).ToList();
            return View(ds);
        }

        // GET: /QuanLyKhuyenMai/Them
        public ActionResult Them()
        {
            ViewBag.Title = Ngu.S("SellerKM_ThemTitle");
            return View(new KhuyenMai
            {
                NgayBatDau = DateTime.Today,
                NgayKetThuc = DateTime.Today.AddDays(30),
                KichHoat = true,
                LoaiGiam = LoaiTongTien
            });
        }

        // POST: /QuanLyKhuyenMai/Them
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Them(KhuyenMai km)
        {
            ChuanHoa(km);
            KiemTraNghiepVu(km, null);

            if (!ModelState.IsValid)
            {
                ViewBag.Title = Ngu.S("SellerKM_ThemTitle");
                return View(km);
            }

            db.KhuyenMais.Add(km);

            if (!LuuThayDoi(out string loi))
            {
                ModelState.AddModelError("", loi);
                ViewBag.Title = Ngu.S("SellerKM_ThemTitle");
                return View(km);
            }

            ThongBao(string.Format(Ngu.S("SellerKM_DaThemFormat"), km.TenKM));
            return RedirectToAction("Index");
        }

        // GET: /QuanLyKhuyenMai/Sua/1
        public ActionResult Sua(int id)
        {
            var km = db.KhuyenMais.Find(id);
            if (km == null) return HttpNotFound(Ngu.S("SellerKM_KhongTimThay"));

            ViewBag.Title = Ngu.S("SellerKM_SuaTitle");
            return View(km);
        }

        // POST: /QuanLyKhuyenMai/Sua/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sua(KhuyenMai km)
        {
            ChuanHoa(km);
            KiemTraNghiepVu(km, km.MaKM);

            if (!ModelState.IsValid)
            {
                ViewBag.Title = Ngu.S("SellerKM_SuaTitle");
                return View(km);
            }

            var khuyenMai = db.KhuyenMais.Find(km.MaKM);
            if (khuyenMai == null)
                return HttpNotFound(Ngu.S("SellerKM_KhongTimThaySuaCanXoa"));

            khuyenMai.TenKM = km.TenKM;
            khuyenMai.MaGiamGia = km.MaGiamGia;
            khuyenMai.PhanTramGiam = km.PhanTramGiam;
            khuyenMai.GiamToiDa = km.GiamToiDa;
            khuyenMai.DonToiThieu = km.DonToiThieu;
            khuyenMai.NgayBatDau = km.NgayBatDau;
            khuyenMai.NgayKetThuc = km.NgayKetThuc;
            khuyenMai.MoTa = km.MoTa;
            khuyenMai.KichHoat = km.KichHoat;
            khuyenMai.LoaiGiam = km.LoaiGiam;

            if (!LuuThayDoi(out string loi))
            {
                ModelState.AddModelError("", loi);
                ViewBag.Title = Ngu.S("SellerKM_SuaTitle");
                return View(km);
            }

            ThongBao(string.Format(Ngu.S("SellerKM_DaCapNhatFormat"), km.TenKM));
            return RedirectToAction("Index");
        }

        // GET: /QuanLyKhuyenMai/Xoa/1
        public ActionResult Xoa(int id)
        {
            var km = db.KhuyenMais.Find(id);
            if (km == null) return HttpNotFound(Ngu.S("SellerKM_KhongTimThay"));

            ViewBag.Title = Ngu.S("SellerKM_XoaTitle");
            return View(km);
        }

        // POST: /QuanLyKhuyenMai/XacNhanXoa/1
        [HttpPost, ActionName("XacNhanXoa")]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanXoa(int id)
        {
            var km = db.KhuyenMais.Find(id);
            if (km == null)
            {
                ThongBao(Ngu.S("SellerKM_KhongTimThay"), "danger");
                return RedirectToAction("Index");
            }

            db.KhuyenMais.Remove(km);

            if (!LuuThayDoi(out string loi))
            {
                ThongBao(loi, "danger");
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
            var km = db.KhuyenMais.Find(id);
            if (km == null) return HttpNotFound(Ngu.S("SellerKM_KhongTimThay"));

            km.KichHoat = !km.KichHoat;

            if (!LuuThayDoi(out string loi))
            {
                ThongBao(loi, "danger");
                return RedirectToAction("Index");
            }

            ThongBao(string.Format(Ngu.S("SellerKM_DaDuocFormat"),
                km.TenKM, km.KichHoat ? Ngu.S("SellerKM_Bat") : Ngu.S("SellerKM_Tat")), "info");
            return RedirectToAction("Index");
        }

        #region Hàm phụ

        /// <summary>Chuẩn hóa mã giảm giá (bỏ khoảng trắng, viết hoa) TRƯỚC khi kiểm tra trùng và lưu.</summary>
        private static void ChuanHoa(KhuyenMai km)
        {
            if (km.MaGiamGia != null) km.MaGiamGia = km.MaGiamGia.Trim().ToUpper();
            if (km.TenKM != null) km.TenKM = km.TenKM.Trim();
        }

        private bool LuuThayDoi(out string loi)
        {
            try
            {
                db.SaveChanges();
                loi = null;
                return true;
            }
            catch (DbUpdateException)
            {
                loi = "Không thể lưu vào cơ sở dữ liệu. Mã giảm giá có thể bị trùng hoặc dữ liệu vi phạm ràng buộc.";
                return false;
            }
        }

        /// <summary>Kiểm tra mã trùng, thời gian hiệu lực và các điều kiện (khớp CHECK/UNIQUE trong bảng KhuyenMai).</summary>
        private void KiemTraNghiepVu(KhuyenMai km, int? boQuaMaKM)
        {
            if (string.IsNullOrWhiteSpace(km.TenKM))
                ModelState.AddModelError("TenKM", "Vui lòng nhập tên chương trình.");

            // Mã giảm giá: bắt buộc + không trùng (cột MaGiamGia có UNIQUE)
            if (string.IsNullOrWhiteSpace(km.MaGiamGia))
                ModelState.AddModelError("MaGiamGia", "Vui lòng nhập mã giảm giá.");
            else
            {
                string ma = km.MaGiamGia;
                bool trung = db.KhuyenMais.Any(k =>
                    k.MaGiamGia == ma &&
                    (!boQuaMaKM.HasValue || k.MaKM != boQuaMaKM.Value));
                if (trung)
                    ModelState.AddModelError("MaGiamGia", Ngu.S("SellerKM_MaTrung"));
            }

            // Thời gian hiệu lực: CHECK (NgayKetThuc >= NgayBatDau)
            if (km.NgayKetThuc.Date < km.NgayBatDau.Date)
                ModelState.AddModelError("NgayKetThuc", Ngu.S("SellerKM_NgayKetThucSaiThuTu"));

            // Phần trăm giảm: 1..100 (DB cho 0..100 nhưng 0% thì vô nghĩa)
            if (km.PhanTramGiam <= 0)
                ModelState.AddModelError("PhanTramGiam", Ngu.S("SellerKM_PhanTramPhaiLonHon0"));
            else if (km.PhanTramGiam > 100)
                ModelState.AddModelError("PhanTramGiam", "Phần trăm giảm tối đa là 100.");

            if (km.GiamToiDa < 0)
                ModelState.AddModelError("GiamToiDa", "Mức giảm tối đa không được âm.");

            if (km.DonToiThieu < 0)
                ModelState.AddModelError("DonToiThieu", "Đơn tối thiểu không được âm.");

            // CHECK (LoaiGiam IN ('TongTien','PhiGiaoHang'))
            if (km.LoaiGiam != LoaiTongTien && km.LoaiGiam != LoaiPhiGiaoHang)
                ModelState.AddModelError("LoaiGiam", "Loại giảm không hợp lệ.");
        }

        #endregion
    }
}
