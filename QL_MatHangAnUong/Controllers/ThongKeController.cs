using System;
using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Models.ViewModels;
using QL_MatHangAnUong.Helpers;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// [NGƯỜI BÁN] Thống kê doanh thu — rubric mục 7.3. Đây cũng là trang chủ của khu quản trị.
    ///
    /// Quy ước tính doanh thu: chỉ tính các đơn có trạng thái "Hoàn thành"
    /// (đơn đã hủy hoặc đang xử lý chưa được tính là doanh thu thực).
    /// </summary>
    [KiemTraNguoiBan]
    public class ThongKeController : BaseController
    {
        // GET: /ThongKe?tuNgay=2026-08-01&denNgay=2026-08-25
        public ActionResult Index(DateTime? tuNgay, DateTime? denNgay)
        {
            DateTime den = (denNgay ?? DateTime.Today).Date;
            DateTime tu = (tuNgay ?? den.AddDays(-29)).Date;
            if (tu > den) { var tmp = tu; tu = den; den = tmp; }

            var tatCaDon = KhoDuLieu.LayDonHangs();
            var donTrongKy = tatCaDon
                .Where(d => d.NgayDat.Date >= tu && d.NgayDat.Date <= den)
                .ToList();
            var donHoanThanh = donTrongKy
                .Where(d => d.TrangThai == DonHang.HoanThanh)
                .ToList();

            var model = new ThongKeViewModel
            {
                TuNgay = tu,
                DenNgay = den,
                TongDoanhThu = donHoanThanh.Sum(d => d.TongTien),
                TongDonHang = donTrongKy.Count,
                DonHoanThanh = donHoanThanh.Count,
                DonChoXacNhan = donTrongKy.Count(d => d.TrangThai == DonHang.ChoXacNhan),
                DonDaHuy = donTrongKy.Count(d => d.TrangThai == DonHang.DaHuy),
                TongSanPham = KhoDuLieu.LaySanPhams().Count,
                TongKhachHang = KhoDuLieu.LayNguoiDungs().Count(n => n.VaiTro == NguoiDung.RoleCustomer)
            };

            model.GiaTriTrungBinh = donHoanThanh.Count > 0
                ? Math.Round(model.TongDoanhThu / donHoanThanh.Count)
                : 0;

            // --- Doanh thu theo từng ngày (cho biểu đồ cột) ---
            for (var ngay = tu; ngay <= den; ngay = ngay.AddDays(1))
            {
                var donNgay = donHoanThanh.Where(d => d.NgayDat.Date == ngay).ToList();
                model.DoanhThuNgay.Add(new DoanhThuTheoNgay
                {
                    Ngay = ngay,
                    DoanhThu = donNgay.Sum(d => d.TongTien),
                    SoDon = donNgay.Count
                });
            }

            // --- Doanh thu theo loại sản phẩm ---
            var chiTiet = donHoanThanh.SelectMany(d => d.ChiTietDonHangs).ToList();
            var sanPhams = KhoDuLieu.LaySanPhams();

            model.DoanhThuLoai = chiTiet
                .Join(sanPhams, ct => ct.MaSP, sp => sp.MaSP, (ct, sp) => new { ct, sp })
                .GroupBy(x => x.sp.LoaiSanPham != null ? x.sp.LoaiSanPham.TenLoai : Ngu.S("Seller_Khac"))
                .Select(g => new DoanhThuTheoLoai
                {
                    TenLoai = g.Key,
                    DoanhThu = g.Sum(x => x.ct.ThanhTien),
                    SoLuong = g.Sum(x => x.ct.SoLuong)
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();

            // --- Top 5 sản phẩm bán chạy ---
            model.TopBanChay = chiTiet
                .GroupBy(c => new { c.MaSP, c.TenSP })
                .Select(g => new SanPhamBanChay
                {
                    MaSP = g.Key.MaSP,
                    TenSP = g.Key.TenSP,
                    HinhAnh = sanPhams.Where(s => s.MaSP == g.Key.MaSP)
                                      .Select(s => s.HinhAnh)
                                      .FirstOrDefault(),
                    SoLuongBan = g.Sum(x => x.SoLuong),
                    DoanhThu = g.Sum(x => x.ThanhTien)
                })
                .OrderByDescending(x => x.SoLuongBan)
                .Take(5)
                .ToList();

            ViewBag.DonMoiNhat = tatCaDon.Take(5).ToList();
            ViewBag.Title = Ngu.S("Seller_ThongKeDoanhThu");

            return View(model);
        }
    }
}
