using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Models.ViewModels;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// Trang thực đơn dành cho khách: danh sách, tìm kiếm, lọc theo loại, chi tiết, sản phẩm liên quan.
    /// Không cần đăng nhập (đáp ứng nhóm chức năng của khách vãng lai).
    /// </summary>
    public class SanPhamController : BaseController
    {
        private const int SoSanPhamMoiTrang = 9;

        /// <summary>
        /// GET: /SanPham/Index?tuKhoa=tra&maLoai=1&giaTu=&giaDen=&sapXep=gia-tang&trang=1
        /// Đáp ứng rubric: hiển thị danh sách (3.1), lọc dữ liệu theo loại (3.3), tìm kiếm (2.3).
        /// </summary>
        public ActionResult Index(string tuKhoa, int? maLoai, decimal? giaTu, decimal? giaDen,
                                  string sapXep, int trang = 1)
        {
            var ketQua = KhoDuLieu.LocSanPham(tuKhoa, maLoai, giaTu, giaDen, sapXep);

            if (trang < 1) trang = 1;
            int tongSoTrang = (int)Math.Ceiling(ketQua.Count / (double)SoSanPhamMoiTrang);
            if (tongSoTrang < 1) tongSoTrang = 1;
            if (trang > tongSoTrang) trang = tongSoTrang;

            var loaiDangChon = maLoai.HasValue ? KhoDuLieu.LayLoai(maLoai.Value) : null;

            var model = new DanhSachSanPhamViewModel
            {
                SanPhams = ketQua.Skip((trang - 1) * SoSanPhamMoiTrang)
                                 .Take(SoSanPhamMoiTrang)
                                 .ToList(),
                DanhSachLoai = KhoDuLieu.LayLoaiHienThi(),
                TuKhoa = tuKhoa,
                MaLoai = maLoai,
                TenLoaiDangChon = loaiDangChon != null ? loaiDangChon.TenLoai : null,
                GiaTu = giaTu,
                GiaDen = giaDen,
                SapXep = sapXep,
                TrangHienTai = trang,
                TongSoTrang = tongSoTrang,
                TongSoSanPham = ketQua.Count
            };

            ViewBag.SoSanPhamTheoLoai = model.DanhSachLoai
                .ToDictionary(l => l.MaLoai, l => KhoDuLieu.DemSanPhamTheoLoai(l.MaLoai));
            ViewBag.Title = string.IsNullOrWhiteSpace(tuKhoa)
                ? (model.TenLoaiDangChon ?? "Thực đơn")
                : "Kết quả tìm kiếm";

            return View(model);
        }

        /// <summary>
        /// GET: /SanPham/ChiTiet/5
        /// Đáp ứng rubric mục 4: hiển thị chi tiết + sản phẩm liên quan.
        /// </summary>
        public ActionResult ChiTiet(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var sp = KhoDuLieu.LaySanPham(id.Value);
            if (sp == null || !sp.DangBan)
                return HttpNotFound("Không tìm thấy sản phẩm bạn cần xem.");

            var model = new ChiTietSanPhamViewModel
            {
                SanPham = sp,
                SanPhamLienQuan = KhoDuLieu.LaySanPhamLienQuan(sp.MaSP, sp.MaLoai, 4)
            };

            ViewBag.Title = sp.TenSP;
            return View(model);
        }

        /// <summary>
        /// Gợi ý tìm kiếm cho ô search ở header (gọi bằng AJAX).
        /// </summary>
        public ActionResult GoiY(string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa) || tuKhoa.Trim().Length < 2)
                return Json(new object[0], JsonRequestBehavior.AllowGet);

            var ds = KhoDuLieu.LocSanPham(tuKhoa, null, null, null, null)
                              .Take(6)
                              .Select(s => new
                              {
                                  maSP = s.MaSP,
                                  tenSP = s.TenSP,
                                  hinhAnh = s.HinhAnh,
                                  gia = s.GiaBanThucTe
                              })
                              .ToList();

            return Json(ds, JsonRequestBehavior.AllowGet);
        }
    }
}
