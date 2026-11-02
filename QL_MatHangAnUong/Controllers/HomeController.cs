using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Models.ViewModels;
using QL_MatHangAnUong.Helpers;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// Trang chủ và các trang tĩnh. Ai cũng xem được (kể cả khách vãng lai).
    /// Dùng db kế thừa từ BaseController — không cần khai báo thêm.
    /// </summary>
    public class HomeController : BaseController
    {
        // GET: /  hoặc  /Home/Index
        public ActionResult Index()
        {
            var now = DateTime.Now.Date;

            // Lấy 1 lần, lọc bằng LINQ in-memory để tránh truy vấn thừa
            var sanPhamDangBan = db.SanPhams
                .Where(s => s.DangBan)
                .ToList();

            var model = new TrangChuViewModel
            {
                DanhSachLoai = db.LoaiSanPhams
                    .Where(l => l.HienThi)
                    .OrderBy(l => l.ThuTu)
                    .ToList(),

                // Sản phẩm nổi bật: NoiBat = true, ưu tiên bán chạy
                SanPhamNoiBat = sanPhamDangBan
                    .Where(s => s.NoiBat)
                    .OrderByDescending(s => s.LuotBan)
                    .Take(8)
                    .ToList(),

                // Sản phẩm mới: thêm vào gần đây nhất
                SanPhamMoi = sanPhamDangBan
                    .OrderByDescending(s => s.NgayTao)
                    .Take(4)
                    .ToList(),

                // Sản phẩm giảm giá: có GiaKhuyenMai, ưu tiên % giảm cao nhất
                SanPhamGiamGia = sanPhamDangBan
                    .Where(s => s.GiaKhuyenMai.HasValue && s.GiaKhuyenMai > 0)
                    .OrderByDescending(s => (s.Gia - s.GiaKhuyenMai.Value) / s.Gia * 100)
                    .Take(4)
                    .ToList(),

                // Khuyến mãi đang có hiệu lực
                KhuyenMais = db.KhuyenMais
                    .Where(k => k.KichHoat
                             && k.NgayBatDau <= DateTime.Now
                             && k.NgayKetThuc >= DateTime.Now)
                    .Take(3)
                    .ToList()
            };

            // Đếm số món của từng loại để View hiển thị con số bên cạnh tên loại
            ViewBag.SoSanPhamTheoLoai = model.DanhSachLoai
                .ToDictionary(
                    l => l.MaLoai,
                    l => db.SanPhams.Count(s => s.MaLoai == l.MaLoai && s.DangBan)
                );

            return View(model);
        }

        // GET: /Home/KhuyenMai
        public ActionResult KhuyenMai()
        {
            ViewBag.Title = "Khuyến mãi đang chạy";
            return View(db.KhuyenMais
                .Where(k => k.KichHoat
                         && k.NgayBatDau <= DateTime.Now
                         && k.NgayKetThuc >= DateTime.Now)
                .ToList());
        }

        // GET: /Home/GioiThieu
        public ActionResult GioiThieu()
        {
            ViewBag.Title = Ngu.S("Home_GioiThieuTitle");
            return View();
        }

        // GET: /Home/LienHe
        public ActionResult LienHe()
        {
            ViewBag.Title = Ngu.S("Common_LienHe");
            return View();
        }

        // POST: /Home/LienHe — lưu tin nhắn của khách vào bảng LienLacs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LienHe(string hoTen, string soDienThoai, string email, string noiDung)
        {
            hoTen = (hoTen ?? "").Trim();
            soDienThoai = (soDienThoai ?? "").Trim();
            email = (email ?? "").Trim();
            noiDung = (noiDung ?? "").Trim();

            // ----- Validation -----
            string loi = null;
            if (hoTen.Length == 0) loi = "Vui lòng nhập họ và tên.";
            else if (hoTen.Length > 100) loi = "Họ tên tối đa 100 ký tự.";
            else if (soDienThoai.Length == 0 && email.Length == 0) loi = "Vui lòng để lại số điện thoại hoặc email để chúng tôi liên hệ lại.";
            else if (soDienThoai.Length > 0 && !Regex.IsMatch(soDienThoai, @"^0\d{9,10}$")) loi = "Số điện thoại không hợp lệ (10–11 số, bắt đầu bằng 0).";
            else if (email.Length > 0 && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) loi = "Email không đúng định dạng.";
            else if (email.Length > 150) loi = "Email tối đa 150 ký tự.";
            else if (noiDung.Length < 10) loi = "Nội dung cần ít nhất 10 ký tự.";
            else if (noiDung.Length > 2000) loi = "Nội dung tối đa 2000 ký tự.";

            if (loi != null)
            {
                // Giữ lại dữ liệu đã nhập để khách không phải gõ lại
                ViewBag.Loi = loi;
                ViewBag.HoTen = hoTen; ViewBag.SoDienThoai = soDienThoai;
                ViewBag.Email = email; ViewBag.NoiDung = noiDung;
                ViewBag.Title = Ngu.S("Common_LienHe");
                return View();
            }

            try
            {
                // Câu SQL có tham số → an toàn trước SQL Injection
                db.Database.ExecuteSqlCommand(
                    "INSERT INTO LienLacs (HoTen, SoDienThoai, Email, NoiDung, NgayGui, DaDoc) " +
                    "VALUES (@HoTen, @SoDienThoai, @Email, @NoiDung, GETDATE(), 0)",
                    new SqlParameter("@HoTen", hoTen),
                    new SqlParameter("@SoDienThoai", soDienThoai.Length > 0 ? (object)soDienThoai : DBNull.Value),
                    new SqlParameter("@Email", email.Length > 0 ? (object)email : DBNull.Value),
                    new SqlParameter("@NoiDung", noiDung));

                ThongBao("Cảm ơn " + hoTen + "! Tin nhắn đã được gửi, SugarChills sẽ phản hồi sớm nhất có thể.");
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("[LienHe] " + ex.Message);
                ThongBao("Có lỗi khi gửi tin nhắn. Vui lòng thử lại sau hoặc gọi hotline 1900 6789.", "danger");
            }

            // Post/Redirect/Get: F5 không bị gửi trùng tin nhắn
            return RedirectToAction("LienHe");
        }
    }
}
