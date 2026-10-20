using System;
using System.Linq;
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
    }
}
