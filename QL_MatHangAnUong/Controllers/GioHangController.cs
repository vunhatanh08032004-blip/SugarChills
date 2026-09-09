using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Helpers;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Models.ViewModels;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// Giỏ hàng và đặt hàng (rubric mục 5 - 1.5 điểm).
    /// Giỏ hàng lưu trong Session nên khách vãng lai vẫn chọn mua được;
    /// đến bước Thanh toán mới bắt buộc đăng nhập.
    /// </summary>
    public class GioHangController : BaseController
    {
        /// <summary>Phí giao hàng cố định, miễn phí cho đơn từ 150.000đ.</summary>
        public const decimal PhiGiaoHangMacDinh = 15000;
        public const decimal MucMienPhiGiaoHang = 150000;

        // GET: /GioHang
        public ActionResult Index()
        {
            var gio = GioHangHienTai;

            decimal tienGiam = TinhTienGiam(gio);
            decimal phiGiao = (gio.Items.Count == 0 || gio.TamTinh >= MucMienPhiGiaoHang)
                              ? 0 : PhiGiaoHangMacDinh;

            ViewBag.Title = Ngu.S("Common_GioHang");
            ViewBag.KhuyenMaiGoiY = KhoDuLieu.LayKhuyenMaiConHieuLuc();
            ViewBag.TienGiam = tienGiam;
            ViewBag.PhiGiaoHang = phiGiao;
            ViewBag.TongTien = gio.TamTinh - tienGiam + phiGiao;
            ViewBag.MucMienPhiGiaoHang = MucMienPhiGiaoHang;

            return View(gio);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ kèm tùy chọn size / đường / đá / topping (từ trang chi tiết).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Them(int maSP, int soLuong = 1, string size = "M",
                                 string duong = "100%", string da = "100%",
                                 string[] toppings = null)
        {
            var sp = KhoDuLieu.LaySanPham(maSP);
            if (sp == null || !sp.DangBan)
            {
                ThongBao(Ngu.S("Cart_SanPhamKhongTonTai"), "danger");
                return RedirectToAction("Index", "SanPham");
            }

            if (soLuong < 1) soLuong = 1;
            if (soLuong > 100) soLuong = 100;

            var dsTopping = (toppings ?? new string[0])
                            .Where(t => !string.IsNullOrWhiteSpace(t) && TuyChonSanPham.CacTopping.ContainsKey(t))
                            .Distinct()
                            .ToList();

            var item = new GioHangItem
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                HinhAnh = sp.HinhAnh,
                GiaGoc = sp.GiaBanThucTe,
                DonGia = sp.GiaBanThucTe
                         + TuyChonSanPham.PhuThuSize(size)
                         + TuyChonSanPham.PhuThuTopping(dsTopping),
                SoLuong = soLuong,
                Size = size,
                Duong = duong,
                Da = da,
                Toppings = dsTopping
            };

            var gio = GioHangHienTai;
            gio.Them(item);
            PhienLamViec.LuuGioHang(Session, gio);

            ThongBao(string.Format(Ngu.S("Cart_DaThemVaoGioFormat"), sp.TenSP));
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Thêm nhanh 1 ly với tùy chọn mặc định (nút "Thêm vào giỏ" ở card sản phẩm).
        /// Hỗ trợ AJAX để không phải tải lại trang.
        /// </summary>
        [HttpPost]
        public ActionResult ThemNhanh(int maSP)
        {
            var sp = KhoDuLieu.LaySanPham(maSP);
            if (sp == null || !sp.DangBan)
            {
                if (Request.IsAjaxRequest())
                    return Json(new { thanhCong = false, thongBao = Ngu.S("Cart_SanPhamKhongConBan") });

                ThongBao(Ngu.S("Cart_SanPhamKhongConBan"), "danger");
                return RedirectToAction("Index", "SanPham");
            }

            var item = new GioHangItem
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                HinhAnh = sp.HinhAnh,
                GiaGoc = sp.GiaBanThucTe,
                DonGia = sp.GiaBanThucTe + TuyChonSanPham.PhuThuSize("M"),
                SoLuong = 1,
                Size = "M",
                Duong = "100%",
                Da = "100%",
                Toppings = new List<string>()
            };

            var gio = GioHangHienTai;
            gio.Them(item);
            PhienLamViec.LuuGioHang(Session, gio);

            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    thanhCong = true,
                    thongBao = string.Format(Ngu.S("Cart_DaThemVaoGioFormat"), sp.TenSP),
                    soLuongGio = gio.TongSoLuong,
                    tamTinh = DinhDang.Tien(gio.TamTinh)
                });
            }

            ThongBao(string.Format(Ngu.S("Cart_DaThemVaoGioFormat"), sp.TenSP));
            return RedirectToAction("Index");
        }

        /// <summary>Tăng/giảm/nhập lại số lượng một dòng trong giỏ.</summary>
        [HttpPost]
        public ActionResult CapNhat(string khoa, int soLuong)
        {
            var gio = GioHangHienTai;
            gio.CapNhatSoLuong(khoa, soLuong);
            PhienLamViec.LuuGioHang(Session, gio);

            if (Request.IsAjaxRequest())
            {
                var item = gio.Items.FirstOrDefault(i => i.Khoa == khoa);
                return Json(new
                {
                    thanhCong = true,
                    daXoa = item == null,
                    thanhTien = item != null ? DinhDang.Tien(item.ThanhTien) : "0đ",
                    tamTinh = DinhDang.Tien(gio.TamTinh),
                    soLuongGio = gio.TongSoLuong
                });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Xoa(string khoa)
        {
            var gio = GioHangHienTai;
            gio.Xoa(khoa);
            PhienLamViec.LuuGioHang(Session, gio);

            ThongBao(Ngu.S("Cart_DaXoaSanPham"), "info");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult XoaTatCa()
        {
            var gio = GioHangHienTai;
            gio.XoaTatCa();
            PhienLamViec.LuuGioHang(Session, gio);

            ThongBao(Ngu.S("Cart_DaXoaToanBo"), "info");
            return RedirectToAction("Index");
        }

        /// <summary>Khách nhập mã giảm giá ở giỏ hàng (liên kết với chức năng Quản lý khuyến mãi).</summary>
        [HttpPost]
        public ActionResult ApDungMa(string maGiamGia)
        {
            var gio = GioHangHienTai;
            var km = KhoDuLieu.TimTheoMaGiamGia(maGiamGia);

            if (km == null)
            {
                ThongBao(Ngu.S("Cart_MaKhongTonTai"), "danger");
            }
            else if (!km.ConHieuLuc)
            {
                ThongBao(Ngu.S("Cart_MaHetHan"), "warning");
            }
            else if (gio.TamTinh < km.DonToiThieu)
            {
                ThongBao(string.Format(Ngu.S("Cart_MaChiApDungFormat"),
                    km.MaGiamGia.ToUpper(), DinhDang.Tien(km.DonToiThieu)), "warning");
            }
            else
            {
                gio.MaGiamGia = km.MaGiamGia;
                PhienLamViec.LuuGioHang(Session, gio);
                ThongBao(string.Format(Ngu.S("Cart_ApDungThanhCongFormat"),
                    km.MaGiamGia.ToUpper(), DinhDang.Tien(km.TinhTienGiam(gio.TamTinh))));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult BoMa()
        {
            var gio = GioHangHienTai;
            gio.MaGiamGia = null;
            PhienLamViec.LuuGioHang(Session, gio);
            return RedirectToAction("Index");
        }

        // GET: /GioHang/ThanhToan  — bắt buộc đăng nhập
        [KiemTraDangNhap]
        public ActionResult ThanhToan()
        {
            var gio = GioHangHienTai;
            if (gio.Items.Count == 0)
            {
                ThongBao(Ngu.S("Cart_GioHangDangTrongCanhBao"), "warning");
                return RedirectToAction("Index", "SanPham");
            }

            var nd = NguoiDungHienTai;
            var model = TaoViewModelThanhToan(gio);
            model.TenNguoiNhan = nd.HoTen;
            model.DienThoai = nd.DienThoai;
            model.DiaChi = nd.DiaChi;

            ViewBag.Title = Ngu.S("Checkout_DatHang");
            return View(model);
        }

        // POST: /GioHang/ThanhToan
        [HttpPost]
        [KiemTraDangNhap]
        [ValidateAntiForgeryToken]
        public ActionResult ThanhToan(DatHangViewModel model)
        {
            var gio = GioHangHienTai;
            if (gio.Items.Count == 0)
            {
                ThongBao(Ngu.S("Cart_GioHangDangTrong"), "warning");
                return RedirectToAction("Index", "SanPham");
            }

            if (!ModelState.IsValid)
            {
                var vmLoi = TaoViewModelThanhToan(gio);
                vmLoi.TenNguoiNhan = model.TenNguoiNhan;
                vmLoi.DienThoai = model.DienThoai;
                vmLoi.DiaChi = model.DiaChi;
                vmLoi.GhiChu = model.GhiChu;
                vmLoi.HinhThucThanhToan = model.HinhThucThanhToan;
                return View(vmLoi);
            }

            var nd = NguoiDungHienTai;
            decimal tamTinh = gio.TamTinh;
            decimal tienGiam = TinhTienGiam(gio);
            decimal phiGiao = tamTinh >= MucMienPhiGiaoHang ? 0 : PhiGiaoHangMacDinh;

            var dh = new DonHang
            {
                MaND = nd.MaND,
                TenNguoiNhan = model.TenNguoiNhan,
                DienThoai = model.DienThoai,
                DiaChi = model.DiaChi,
                GhiChu = model.GhiChu,
                HinhThucThanhToan = model.HinhThucThanhToan,
                NgayDat = DateTime.Now,
                TamTinh = tamTinh,
                TienGiam = tienGiam,
                PhiGiaoHang = phiGiao,
                TongTien = tamTinh - tienGiam + phiGiao,
                MaGiamGiaApDung = gio.MaGiamGia,
                TrangThai = DonHang.ChoXacNhan
            };

            foreach (var item in gio.Items)
            {
                dh.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaSP = item.MaSP,
                    TenSP = item.TenSP,
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia,
                    TuyChon = item.MoTaTuyChon
                });
            }

            KhoDuLieu.ThemDonHang(dh);

            gio.XoaTatCa();
            PhienLamViec.LuuGioHang(Session, gio);

            return RedirectToAction("DatHangThanhCong", new { id = dh.MaDH });
        }

        // GET: /GioHang/DatHangThanhCong/1001
        [KiemTraDangNhap]
        public ActionResult DatHangThanhCong(int id)
        {
            var dh = KhoDuLieu.LayDonHang(id);
            if (dh == null) return HttpNotFound(Ngu.S("Order_KhongTimThayDonHang"));

            var nd = NguoiDungHienTai;
            if (dh.MaND != nd.MaND && nd.VaiTro != NguoiDung.RoleSeller)
                return new HttpUnauthorizedResult(Ngu.S("Order_KhongCoQuyenXem"));

            ViewBag.Title = Ngu.S("Success_DatHangThanhCong");
            return View(dh);
        }

        #region Hàm dùng chung

        private DatHangViewModel TaoViewModelThanhToan(GioHang gio)
        {
            decimal tienGiam = TinhTienGiam(gio);
            decimal phiGiao = gio.TamTinh >= MucMienPhiGiaoHang ? 0 : PhiGiaoHangMacDinh;

            return new DatHangViewModel
            {
                GioHang = gio,
                TienGiam = tienGiam,
                PhiGiaoHang = phiGiao,
                TongTien = gio.TamTinh - tienGiam + phiGiao,
                HinhThucThanhToan = "COD"
            };
        }

        private decimal TinhTienGiam(GioHang gio)
        {
            if (string.IsNullOrWhiteSpace(gio.MaGiamGia)) return 0;

            var km = KhoDuLieu.TimTheoMaGiamGia(gio.MaGiamGia);
            if (km == null) return 0;

            return km.TinhTienGiam(gio.TamTinh);
        }

        #endregion
    }
}
