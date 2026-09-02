using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Models;

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
            ViewBag.Title = "Quản lý khuyến mãi";
            return View(KhoDuLieu.LayKhuyenMais());
        }

        // GET: /QuanLyKhuyenMai/Them
        public ActionResult Them()
        {
            ViewBag.Title = "Thêm khuyến mãi";
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
                ViewBag.Title = "Thêm khuyến mãi";
                return View(km);
            }

            km.MaGiamGia = km.MaGiamGia.Trim().ToUpper();
            KhoDuLieu.ThemKhuyenMai(km);
            ThongBao(string.Format("Đã thêm chương trình \"{0}\".", km.TenKM));
            return RedirectToAction("Index");
        }

        // GET: /QuanLyKhuyenMai/Sua/1
        public ActionResult Sua(int id)
        {
            var km = KhoDuLieu.LayKhuyenMai(id);
            if (km == null) return HttpNotFound("Không tìm thấy chương trình khuyến mãi.");

            ViewBag.Title = "Sửa khuyến mãi";
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
                ViewBag.Title = "Sửa khuyến mãi";
                return View(km);
            }

            km.MaGiamGia = km.MaGiamGia.Trim().ToUpper();
            if (!KhoDuLieu.CapNhatKhuyenMai(km))
                return HttpNotFound("Không tìm thấy chương trình cần sửa.");

            ThongBao(string.Format("Đã cập nhật chương trình \"{0}\".", km.TenKM));
            return RedirectToAction("Index");
        }

        // GET: /QuanLyKhuyenMai/Xoa/1
        public ActionResult Xoa(int id)
        {
            var km = KhoDuLieu.LayKhuyenMai(id);
            if (km == null) return HttpNotFound("Không tìm thấy chương trình khuyến mãi.");

            ViewBag.Title = "Xóa khuyến mãi";
            return View(km);
        }

        // POST: /QuanLyKhuyenMai/XacNhanXoa/1
        [HttpPost, ActionName("XacNhanXoa")]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanXoa(int id)
        {
            if (!KhoDuLieu.XoaKhuyenMai(id))
            {
                ThongBao("Không tìm thấy chương trình khuyến mãi.", "danger");
                return RedirectToAction("Index");
            }

            ThongBao("Đã xóa chương trình khuyến mãi.", "info");
            return RedirectToAction("Index");
        }

        /// <summary>Bật/tắt nhanh một chương trình ngay tại bảng danh sách.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiTrangThai(int id)
        {
            var km = KhoDuLieu.LayKhuyenMai(id);
            if (km == null) return HttpNotFound("Không tìm thấy chương trình khuyến mãi.");

            km.KichHoat = !km.KichHoat;
            KhoDuLieu.CapNhatKhuyenMai(km);

            ThongBao(string.Format("Chương trình \"{0}\" đã được {1}.",
                km.TenKM, km.KichHoat ? "bật" : "tắt"), "info");
            return RedirectToAction("Index");
        }

        private void KiemTraNghiepVu(KhuyenMai km, int? boQuaMaKM)
        {
            if (!string.IsNullOrWhiteSpace(km.MaGiamGia) &&
                KhoDuLieu.MaGiamGiaDaTonTai(km.MaGiamGia, boQuaMaKM))
            {
                ModelState.AddModelError("MaGiamGia", "Mã giảm giá này đã được dùng cho chương trình khác.");
            }

            if (km.NgayKetThuc.Date < km.NgayBatDau.Date)
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");

            if (km.PhanTramGiam <= 0)
                ModelState.AddModelError("PhanTramGiam", "Phần trăm giảm phải lớn hơn 0.");
        }
    }
}
