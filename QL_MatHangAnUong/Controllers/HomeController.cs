using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Models.ViewModels;
using QL_MatHangAnUong.Helpers;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// Trang chủ và các trang tĩnh. Ai cũng xem được (kể cả khách vãng lai).
    /// </summary>
    public class HomeController : BaseController
    {
        // GET: /  hoặc  /Home/Index
        public ActionResult Index()
        {
            var model = new TrangChuViewModel
            {
                DanhSachLoai = KhoDuLieu.LayLoaiHienThi(),
                SanPhamNoiBat = KhoDuLieu.LaySanPhamDangBan()
                                         .Where(s => s.NoiBat)
                                         .OrderByDescending(s => s.LuotBan)
                                         .Take(8)
                                         .ToList(),
                SanPhamMoi = KhoDuLieu.LaySanPhamDangBan()
                                      .OrderByDescending(s => s.NgayTao)
                                      .Take(4)
                                      .ToList(),
                SanPhamGiamGia = KhoDuLieu.LaySanPhamDangBan()
                                          .Where(s => s.GiaKhuyenMai.HasValue && s.GiaKhuyenMai > 0)
                                          .OrderByDescending(s => s.PhanTramGiam)
                                          .Take(4)
                                          .ToList(),
                KhuyenMais = KhoDuLieu.LayKhuyenMaiConHieuLuc().Take(3).ToList()
            };

            // Đếm số món của từng loại ở Controller, View chỉ việc hiển thị
            ViewBag.SoSanPhamTheoLoai = model.DanhSachLoai
                .ToDictionary(l => l.MaLoai, l => KhoDuLieu.DemSanPhamTheoLoai(l.MaLoai));

            return View(model);
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
