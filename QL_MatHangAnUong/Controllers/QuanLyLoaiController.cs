using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Helpers;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// [NGƯỜI BÁN] Quản lý loại sản phẩm — rubric mục 8 (thêm, xóa, sửa, hiển thị).
    /// </summary>
    [KiemTraNguoiBan]
    public class QuanLyLoaiController : BaseController
    {
        // GET: /QuanLyLoai
        public ActionResult Index()
        {
            var ds = KhoDuLieu.LayLoais();

            // Số sản phẩm của mỗi loại để hiển thị ở bảng và cảnh báo khi xóa
            ViewBag.SoSanPham = ds.ToDictionary(l => l.MaLoai, l => KhoDuLieu.DemSanPhamTheoLoai(l.MaLoai));
            ViewBag.Title = Ngu.S("SellerLoai_Title");

            return View(ds);
        }

        // GET: /QuanLyLoai/Them
        public ActionResult Them()
        {
            ViewBag.Title = Ngu.S("SellerLoai_ThemTitle");
            return View(new LoaiSanPham { HienThi = true, ThuTu = KhoDuLieu.LayLoais().Count + 1 });
        }

        // POST: /QuanLyLoai/Them
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Them(LoaiSanPham loai)
        {
            if (loai == null)
            {
                loai = new LoaiSanPham();
                ModelState.AddModelError("", "Dữ liệu loại sản phẩm không hợp lệ.");
            }

            loai.MaLoai = 0;
            KiemTraTrungTen(loai);
            KiemTraDuLieu(loai);

            if (!ModelState.IsValid)
            {
                ViewBag.Title = Ngu.S("SellerLoai_ThemTitle");
                return View(loai);
            }

            loai.TenLoai = loai.TenLoai.Trim();
            KhoDuLieu.ThemLoai(loai);
            ThongBao(string.Format(Ngu.S("SellerLoai_DaThemFormat"), loai.TenLoai));
            return RedirectToAction("Index");
        }

        // GET: /QuanLyLoai/Sua/2
        public ActionResult Sua(int id)
        {
            var loai = KhoDuLieu.LayLoai(id);
            if (loai == null) return HttpNotFound(Ngu.S("SellerLoai_KhongTimThay"));

            ViewBag.Title = Ngu.S("SellerLoai_SuaTitle");
            return View(loai);
        }

        // POST: /QuanLyLoai/Sua/2
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sua(LoaiSanPham loai)
        {
            if (loai == null)
            {
                loai = new LoaiSanPham();
                ModelState.AddModelError("", "Dữ liệu loại sản phẩm không hợp lệ.");
            }

            KiemTraTrungTen(loai);
            KiemTraDuLieu(loai);

            if (!ModelState.IsValid)
            {
                ViewBag.Title = Ngu.S("SellerLoai_SuaTitle");
                return View(loai);
            }

            loai.TenLoai = loai.TenLoai.Trim();
            if (!KhoDuLieu.CapNhatLoai(loai))
                return HttpNotFound(Ngu.S("SellerLoai_KhongTimThaySuaCanXoa"));

            ThongBao(string.Format(Ngu.S("SellerLoai_DaCapNhatFormat"), loai.TenLoai));
            return RedirectToAction("Index");
        }

        // GET: /QuanLyLoai/Xoa/2
        public ActionResult Xoa(int id)
        {
            var loai = KhoDuLieu.LayLoai(id);
            if (loai == null) return HttpNotFound(Ngu.S("SellerLoai_KhongTimThay"));

            ViewBag.SoSanPham = KhoDuLieu.DemSanPhamTheoLoai(id);
            ViewBag.Title = Ngu.S("SellerLoai_XoaTitle");
            return View(loai);
        }

        // POST: /QuanLyLoai/XacNhanXoa/2
        [HttpPost, ActionName("XacNhanXoa")]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanXoa(int id)
        {
            string loi;
            if (!KhoDuLieu.XoaLoai(id, out loi))
            {
                ThongBao(loi, "danger");
                return RedirectToAction("Index");
            }

            ThongBao(Ngu.S("SellerLoai_DaXoa"), "info");
            return RedirectToAction("Index");
        }

        private void KiemTraTrungTen(LoaiSanPham loai)
        {
            if (loai == null) return;
            if (string.IsNullOrWhiteSpace(loai.TenLoai)) return;

            string tenLoai = loai.TenLoai.Trim().ToLower();
            bool trung = db.LoaiSanPhams.Any(l =>
                l.MaLoai != loai.MaLoai && l.TenLoai.Trim().ToLower() == tenLoai);

            if (trung) ModelState.AddModelError("TenLoai", Ngu.S("SellerLoai_TrungTen"));
        }

        private void KiemTraDuLieu(LoaiSanPham loai)
        {
            if (loai == null) return;

            if (string.IsNullOrWhiteSpace(loai.TenLoai))
                ModelState.AddModelError("TenLoai", "Vui lòng nhập tên loại sản phẩm.");
            else if (loai.TenLoai.Trim().Length > 100)
                ModelState.AddModelError("TenLoai", "Tên loại sản phẩm không được vượt quá 100 ký tự.");

            if (loai.MoTa != null && loai.MoTa.Length > 500)
                ModelState.AddModelError("MoTa", "Mô tả loại sản phẩm không được vượt quá 500 ký tự.");
            if (loai.HinhAnh != null && loai.HinhAnh.Length > 500)
                ModelState.AddModelError("HinhAnh", "Đường dẫn hình ảnh không được vượt quá 500 ký tự.");
            if (loai.ThuTu < 0)
                ModelState.AddModelError("ThuTu", "Thứ tự phải là số không âm.");
        }
    }
}
