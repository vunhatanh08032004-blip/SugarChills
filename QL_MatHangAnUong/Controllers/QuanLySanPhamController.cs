using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Helpers;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// [NGƯỜI BÁN] Quản lý sản phẩm — rubric mục 6 (Thêm/Xóa/Sửa, hình + loại, radio + checkbox).
    /// </summary>
    [KiemTraNguoiBan]
    public class QuanLySanPhamController : BaseController
    {
        // GET: /QuanLySanPham
        public ActionResult Index(string tuKhoa, int? maLoai, string trangThai)
        {
            var ds = KhoDuLieu.LaySanPhams();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk = tuKhoa.Trim().ToLower();
                ds = ds.Where(s => s.TenSP.ToLower().Contains(tk)).ToList();
            }

            if (maLoai.HasValue && maLoai.Value > 0)
                ds = ds.Where(s => s.MaLoai == maLoai.Value).ToList();

            if (trangThai == "dang-ban")
                ds = ds.Where(s => s.DangBan).ToList();
            else if (trangThai == "ngung-ban")
                ds = ds.Where(s => !s.DangBan).ToList();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.MaLoai = maLoai;
            ViewBag.TrangThai = trangThai;
            ViewBag.DanhSachLoai = KhoDuLieu.LayLoais();
            ViewBag.Title = Ngu.S("Seller_QuanLySanPham");

            return View(ds);
        }

        // GET: /QuanLySanPham/ChiTiet/5
        public ActionResult ChiTiet(int id)
        {
            var sp = KhoDuLieu.LaySanPham(id);
            if (sp == null) return HttpNotFound(Ngu.S("SellerProd_KhongTimThaySP"));

            ViewBag.Title = Ngu.S("SellerProd_ChiTietTitle");
            return View(sp);
        }

        // GET: /QuanLySanPham/Them
        public ActionResult Them()
        {
            NapDanhSachLoai();
            ViewBag.Title = Ngu.S("SellerProd_ThemTitle");
            return View(new SanPham { DangBan = true, SoLuongTon = 100 });
        }

        // POST: /QuanLySanPham/Them
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Them(SanPham sp)
        {
            KiemTraNghiepVu(sp);

            if (!ModelState.IsValid)
            {
                NapDanhSachLoai(sp.MaLoai);
                ViewBag.Title = Ngu.S("SellerProd_ThemTitle");
                return View(sp);
            }

            KhoDuLieu.ThemSanPham(sp);
            ThongBao(string.Format(Ngu.S("SellerProd_DaThemFormat"), sp.TenSP));
            return RedirectToAction("Index");
        }

        // GET: /QuanLySanPham/Sua/5
        public ActionResult Sua(int id)
        {
            var sp = KhoDuLieu.LaySanPham(id);
            if (sp == null) return HttpNotFound(Ngu.S("SellerProd_KhongTimThaySP"));

            NapDanhSachLoai(sp.MaLoai);
            ViewBag.Title = Ngu.S("SellerProd_SuaTitle");
            return View(sp);
        }

        // POST: /QuanLySanPham/Sua/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sua(SanPham sp)
        {
            KiemTraNghiepVu(sp);

            if (!ModelState.IsValid)
            {
                NapDanhSachLoai(sp.MaLoai);
                ViewBag.Title = Ngu.S("SellerProd_SuaTitle");
                return View(sp);
            }

            if (!KhoDuLieu.CapNhatSanPham(sp))
                return HttpNotFound(Ngu.S("SellerProd_KhongTimThaySuaCanXoa"));

            ThongBao(string.Format(Ngu.S("SellerProd_DaCapNhatFormat"), sp.TenSP));
            return RedirectToAction("Index");
        }

        // GET: /QuanLySanPham/Xoa/5  — trang xác nhận trước khi xóa
        public ActionResult Xoa(int id)
        {
            var sp = KhoDuLieu.LaySanPham(id);
            if (sp == null) return HttpNotFound(Ngu.S("SellerProd_KhongTimThaySP"));

            ViewBag.Title = Ngu.S("SellerProd_XoaTitle");
            return View(sp);
        }

        // POST: /QuanLySanPham/XacNhanXoa/5
        [HttpPost, ActionName("XacNhanXoa")]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanXoa(int id)
        {
            string loi;
            if (!KhoDuLieu.XoaSanPham(id, out loi))
            {
                ThongBao(loi, "danger");
                return RedirectToAction("Index");
            }

            ThongBao(Ngu.S("SellerProd_DaXoa"), "info");
            return RedirectToAction("Index");
        }

        // POST: /QuanLySanPham/DoiNoiBat/5 — bật/tắt "nổi bật" ngay trên danh sách (nút sao)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiNoiBat(int id)
        {
            if (!KhoDuLieu.DoiTrangThaiNoiBat(id))
                return HttpNotFound(Ngu.S("SellerProd_KhongTimThaySP"));

            // Quay lại đúng trang danh sách (giữ nguyên tìm kiếm/lọc đang áp dụng)
            return Redirect(Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : Url.Action("Index"));
        }

        #region Hàm phụ

        private void NapDanhSachLoai(int? maLoaiDangChon = null)
        {
            ViewBag.DanhSachLoai = new SelectList(KhoDuLieu.LayLoais(), "MaLoai", "TenLoai", maLoaiDangChon);
        }

        /// <summary>Các quy tắc nghiệp vụ mà Data Annotation không kiểm tra được.</summary>
        private void KiemTraNghiepVu(SanPham sp)
        {
            if (sp.MaLoai <= 0 || KhoDuLieu.LayLoai(sp.MaLoai) == null)
                ModelState.AddModelError("MaLoai", Ngu.S("SellerProd_ChonLoaiHopLe"));

            if (sp.GiaKhuyenMai.HasValue && sp.GiaKhuyenMai.Value > 0 && sp.GiaKhuyenMai.Value >= sp.Gia)
                ModelState.AddModelError("GiaKhuyenMai", Ngu.S("SellerProd_GiaKMPhaiNhoHonGiaBan"));

            if (!string.IsNullOrWhiteSpace(sp.TenSP))
            {
                bool trungTen = KhoDuLieu.LaySanPhams()
                    .Any(s => s.MaSP != sp.MaSP &&
                              s.TenSP.Trim().ToLower() == sp.TenSP.Trim().ToLower());
                if (trungTen)
                    ModelState.AddModelError("TenSP", Ngu.S("SellerProd_TrungTen"));
            }
        }

        #endregion
    }
}
