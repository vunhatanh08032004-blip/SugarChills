using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Helpers;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// [NGƯỜI BÁN] Quản lý sản phẩm — làm việc trực tiếp với Database qua DbContext (biến db của BaseController).
    /// </summary>
    [KiemTraNguoiBan]
    public class QuanLySanPhamController : BaseController
    {
        private new QL_MatHangAnUongEntities db = new QL_MatHangAnUongEntities();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        // GET: /QuanLySanPham
        public ActionResult Index(string tuKhoa, int? maLoai, string trangThai)
        {
            // Lọc ngay trên Database: các điều kiện được dịch thành câu SELECT ... WHERE
            var q = db.SanPhams.Include(s => s.LoaiSanPham).AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk = tuKhoa.Trim();
                q = q.Where(s => s.TenSP.Contains(tk));
            }

            if (maLoai.HasValue && maLoai.Value > 0)
                q = q.Where(s => s.MaLoai == maLoai.Value);

            if (trangThai == "dang-ban")
                q = q.Where(s => s.DangBan);
            else if (trangThai == "ngung-ban")
                q = q.Where(s => !s.DangBan);

            var ds = q.OrderByDescending(s => s.NgayTao).ToList();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.MaLoai = maLoai;
            ViewBag.TrangThai = trangThai;
            ViewBag.DanhSachLoai = db.LoaiSanPhams.OrderBy(l => l.ThuTu).ToList();
            ViewBag.Title = Ngu.S("Seller_QuanLySanPham");

            return View(ds);
        }

        // GET: /QuanLySanPham/ChiTiet/5
        public ActionResult ChiTiet(int id)
        {
            var sp = db.SanPhams.Include(s => s.LoaiSanPham).FirstOrDefault(s => s.MaSP == id);
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
        public ActionResult Them(SanPham sp, HttpPostedFileBase anhTaiLen, string chonAnh)
        {
            XuLyHinhAnh(sp, anhTaiLen, chonAnh);
            KiemTraNghiepVu(sp);

            if (!ModelState.IsValid)
            {
                NapDanhSachLoai(sp.MaLoai);
                ViewBag.Title = Ngu.S("SellerProd_ThemTitle");
                return View(sp);
            }

            // INSERT vào bảng SanPham
            sp.TenSP = sp.TenSP.Trim();
            sp.NgayTao = DateTime.Now;
            sp.LuotBan = 0;
            db.SanPhams.Add(sp);

            if (!LuuThayDoi(out string loi))
            {
                ModelState.AddModelError("", loi);
                NapDanhSachLoai(sp.MaLoai);
                ViewBag.Title = Ngu.S("SellerProd_ThemTitle");
                return View(sp);
            }

            ThongBao(string.Format(Ngu.S("SellerProd_DaThemFormat"), sp.TenSP));
            return RedirectToAction("Index");
        }

        // GET: /QuanLySanPham/Sua/5
        public ActionResult Sua(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound(Ngu.S("SellerProd_KhongTimThaySP"));

            NapDanhSachLoai(sp.MaLoai);
            ViewBag.Title = Ngu.S("SellerProd_SuaTitle");
            return View(sp);
        }

        // POST: /QuanLySanPham/Sua/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sua(SanPham sp, HttpPostedFileBase anhTaiLen, string chonAnh)
        {
            XuLyHinhAnh(sp, anhTaiLen, chonAnh);
            KiemTraNghiepVu(sp);

            if (!ModelState.IsValid)
            {
                NapDanhSachLoai(sp.MaLoai);
                ViewBag.Title = Ngu.S("SellerProd_SuaTitle");
                return View(sp);
            }

            // Lấy bản ghi hiện tại trong Database rồi chỉ cập nhật các cột được phép sửa
            // (không đụng tới NgayTao, LuotBan)
            var sanPham = db.SanPhams.Find(sp.MaSP);
            if (sanPham == null)
                return HttpNotFound(Ngu.S("SellerProd_KhongTimThaySuaCanXoa"));

            sanPham.TenSP = sp.TenSP.Trim();
            sanPham.MaLoai = sp.MaLoai;
            sanPham.Gia = sp.Gia;
            sanPham.GiaKhuyenMai = sp.GiaKhuyenMai;
            sanPham.HinhAnh = sp.HinhAnh;
            sanPham.MoTa = sp.MoTa;
            sanPham.SoLuongTon = sp.SoLuongTon;
            sanPham.DangBan = sp.DangBan;
            sanPham.NoiBat = sp.NoiBat;

            if (!LuuThayDoi(out string loi))
            {
                ModelState.AddModelError("", loi);
                NapDanhSachLoai(sp.MaLoai);
                ViewBag.Title = Ngu.S("SellerProd_SuaTitle");
                return View(sp);
            }

            ThongBao(string.Format(Ngu.S("SellerProd_DaCapNhatFormat"), sanPham.TenSP));
            return RedirectToAction("Index");
        }

        // GET: /QuanLySanPham/Xoa/5 — trang xác nhận trước khi xóa
        public ActionResult Xoa(int id)
        {
            var sp = db.SanPhams.Include(s => s.LoaiSanPham).FirstOrDefault(s => s.MaSP == id);
            if (sp == null) return HttpNotFound(Ngu.S("SellerProd_KhongTimThaySP"));

            // Số dòng chi tiết đơn hàng đang tham chiếu tới sản phẩm (FK_CT_SanPham) — view dùng để cảnh báo/khóa nút xóa
            ViewBag.SoDonLienQuan = db.ChiTietDonHangs.Count(ct => ct.MaSP == id);

            ViewBag.Title = Ngu.S("SellerProd_XoaTitle");
            return View(sp);
        }

        // POST: /QuanLySanPham/XacNhanXoa/5
        [HttpPost, ActionName("XacNhanXoa")]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanXoa(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null)
            {
                ThongBao("Không tìm thấy sản phẩm.", "danger");
                return RedirectToAction("Index");
            }

            // Sản phẩm đã nằm trong đơn hàng (FK_CT_SanPham) thì không được xóa
            if (db.ChiTietDonHangs.Any(ct => ct.MaSP == id))
            {
                ThongBao(string.Format(
                    "Không thể xóa \"{0}\" vì đã có trong đơn hàng. Hãy tắt trạng thái bán thay vì xóa.",
                    sp.TenSP), "danger");
                return RedirectToAction("Index");
            }

            db.SanPhams.Remove(sp);

            if (!LuuThayDoi(out string loi))
            {
                ThongBao(loi, "danger");
                return RedirectToAction("Index");
            }

            ThongBao(Ngu.S("SellerProd_DaXoa"), "info");
            return RedirectToAction("Index");
        }

        // POST: /QuanLySanPham/DoiTrangThaiKinhDoanh/5 — bật/tắt "đang bán"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiTrangThaiKinhDoanh(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound(Ngu.S("SellerProd_KhongTimThaySP"));

            sp.DangBan = !sp.DangBan;

            if (!LuuThayDoi(out string loi))
                ThongBao(loi, "danger");

            return QuayLaiDanhSach();
        }

        // POST: /QuanLySanPham/DoiNoiBat/5 — bật/tắt "nổi bật" (nút sao)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiNoiBat(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound(Ngu.S("SellerProd_KhongTimThaySP"));

            sp.NoiBat = !sp.NoiBat;

            if (!LuuThayDoi(out string loi))
                ThongBao(loi, "danger");

            return QuayLaiDanhSach();
        }

        #region Hàm phụ

        private static readonly string[] DuoiAnhChoPhep = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long DungLuongAnhToiDa = 5 * 1024 * 1024; // 5MB

        /// <summary>Quay lại đúng trang danh sách (giữ nguyên tìm kiếm/lọc đang áp dụng).</summary>
        private ActionResult QuayLaiDanhSach()
        {
            return Redirect(Request.UrlReferrer != null
                ? Request.UrlReferrer.ToString()
                : Url.Action("Index"));
        }

        /// <summary>SaveChanges có bắt lỗi khóa ngoại / ràng buộc của Database.</summary>
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
                loi = "Không thể lưu vào cơ sở dữ liệu. Dữ liệu vi phạm ràng buộc hoặc đang được sử dụng ở nơi khác.";
                return false;
            }
        }

        private void XuLyHinhAnh(SanPham sp, HttpPostedFileBase anhTaiLen, string chonAnh)
        {
            if (chonAnh != "upload") return; // chế độ dán link: giữ nguyên sp.HinhAnh

            if (anhTaiLen == null || anhTaiLen.ContentLength <= 0)
            {
                ModelState.AddModelError("HinhAnh", "Vui lòng chọn một tệp ảnh để tải lên.");
                return;
            }

            string duoiFile = Path.GetExtension(anhTaiLen.FileName).ToLowerInvariant();
            if (!DuoiAnhChoPhep.Contains(duoiFile))
            {
                ModelState.AddModelError("HinhAnh", "Chỉ chấp nhận ảnh định dạng JPG, PNG, GIF hoặc WEBP.");
                return;
            }

            if (anhTaiLen.ContentLength > DungLuongAnhToiDa)
            {
                ModelState.AddModelError("HinhAnh", "Dung lượng ảnh tối đa 5MB.");
                return;
            }

            string thuMuc = Server.MapPath("~/Content/Uploads/SanPham");
            if (!Directory.Exists(thuMuc)) Directory.CreateDirectory(thuMuc);

            string tenFile = Guid.NewGuid().ToString("N") + duoiFile;
            anhTaiLen.SaveAs(Path.Combine(thuMuc, tenFile));

            sp.HinhAnh = "/Content/Uploads/SanPham/" + tenFile;
        }

        /// <summary>Nạp danh sách loại từ bảng LoaiSanPham cho DropDownList ở form thêm/sửa.</summary>
        private void NapDanhSachLoai(int? maLoaiDangChon = null)
        {
            var dsLoai = db.LoaiSanPhams.OrderBy(l => l.ThuTu).ToList();
            ViewBag.DanhSachLoai = new SelectList(dsLoai, "MaLoai", "TenLoai", maLoaiDangChon);
        }

        /// <summary>Kiểm tra dữ liệu và khóa ngoại trước khi INSERT/UPDATE (khớp các CHECK/UNIQUE/FK trong SQL).</summary>
        private void KiemTraNghiepVu(SanPham sp)
        {
            // Khóa ngoại: loại sản phẩm phải tồn tại trong Database
            if (sp.MaLoai <= 0 || !db.LoaiSanPhams.Any(l => l.MaLoai == sp.MaLoai))
                ModelState.AddModelError("MaLoai", Ngu.S("SellerProd_ChonLoaiHopLe"));

            if (string.IsNullOrWhiteSpace(sp.TenSP))
                ModelState.AddModelError("TenSP", "Vui lòng nhập tên sản phẩm.");
            else
            {
                string ten = sp.TenSP.Trim();
                if (ten.Length > 150)
                    ModelState.AddModelError("TenSP", "Tên sản phẩm tối đa 150 ký tự.");

                // Cột TenSP có UNIQUE
                if (db.SanPhams.Any(s => s.MaSP != sp.MaSP && s.TenSP == ten))
                    ModelState.AddModelError("TenSP", Ngu.S("SellerProd_TrungTen"));
            }

            // CHECK (Gia BETWEEN 1000 AND 10000000)
            if (sp.Gia < 1000 || sp.Gia > 10000000)
                ModelState.AddModelError("Gia", "Giá bán phải từ 1.000đ đến 10.000.000đ.");

            // CHECK (GiaKhuyenMai IS NULL OR = 0 OR < Gia)
            if (sp.GiaKhuyenMai.HasValue && sp.GiaKhuyenMai.Value > 0 && sp.GiaKhuyenMai.Value >= sp.Gia)
                ModelState.AddModelError("GiaKhuyenMai", Ngu.S("SellerProd_GiaKMPhaiNhoHonGiaBan"));

            // CHECK (SoLuongTon >= 0)
            if (sp.SoLuongTon < 0)
                ModelState.AddModelError("SoLuongTon", "Số lượng tồn không được âm.");
        }

        #endregion
    }
}
