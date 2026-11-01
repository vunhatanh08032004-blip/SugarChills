using System;
using System.Collections.Generic;
using System.Data;
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
            var canhBao = LamMoiGioHang(gio);
            if (canhBao.Count > 0)
                ThongBao(string.Join(" ", canhBao.Distinct()), "warning");

            decimal phiGiao = (gio.Items.Count == 0 || gio.TamTinh >= MucMienPhiGiaoHang)
                              ? 0 : PhiGiaoHangMacDinh;
            decimal tienGiam = TinhTienGiam(gio, phiGiao);

            ViewBag.Title = Ngu.S("Common_GioHang");
            var thoiGianHienTai = DateTime.Now;
            ViewBag.KhuyenMaiGoiY = db.KhuyenMais
                .Where(k => k.KichHoat
                         && k.NgayBatDau <= thoiGianHienTai
                         && k.NgayKetThuc >= thoiGianHienTai)
                .ToList();
            ViewBag.TienGiam = tienGiam;
            ViewBag.PhiGiaoHang = phiGiao;
            ViewBag.TongTien = Math.Max(0, gio.TamTinh + phiGiao - tienGiam);
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
            if (maSP <= 0)
            {
                ThongBao(Ngu.S("Cart_SanPhamKhongTonTai"), "danger");
                return RedirectToAction("Index", "SanPham");
            }

            if (soLuong < 1 || soLuong > 100)
            {
                ThongBao("Số lượng phải từ 1 đến 100.", "warning");
                return RedirectToAction("Index", "SanPham");
            }

            var dsTopping = (toppings ?? new string[0]).Distinct().ToList();
            if (!TuyChonHopLe(size, duong, da, dsTopping))
            {
                ThongBao("Tùy chọn sản phẩm không hợp lệ.", "warning");
                return RedirectToAction("Index", "SanPham");
            }

            var sp = db.SanPhams.Find(maSP);
            if (sp == null || !sp.DangBan)
            {
                ThongBao(Ngu.S("Cart_SanPhamKhongTonTai"), "danger");
                return RedirectToAction("Index", "SanPham");
            }

            var gio = GioHangHienTai;
            LamMoiGioHang(gio);
            var item = TaoGioHangItem(sp, soLuong, size, duong, da, dsTopping);
            var dongCungTuyChon = gio.Items.FirstOrDefault(i => i.Khoa == item.Khoa);
            if (dongCungTuyChon != null && dongCungTuyChon.SoLuong + soLuong > 100)
            {
                ThongBao("Số lượng phải từ 1 đến 100.", "warning");
                return RedirectToAction("Index", "SanPham");
            }

            string loiTonKho = KiemTraTonKho(gio, sp, soLuong, null);
            if (loiTonKho != null)
            {
                ThongBao(loiTonKho, "warning");
                return RedirectToAction("Index", "SanPham");
            }

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
            if (maSP <= 0)
            {
                if (Request.IsAjaxRequest())
                    return Json(new { thanhCong = false, thongBao = Ngu.S("Cart_SanPhamKhongConBan") });

                ThongBao(Ngu.S("Cart_SanPhamKhongConBan"), "danger");
                return RedirectToAction("Index", "SanPham");
            }

            var sp = db.SanPhams.Find(maSP);
            if (sp == null || !sp.DangBan)
            {
                if (Request.IsAjaxRequest())
                    return Json(new { thanhCong = false, thongBao = Ngu.S("Cart_SanPhamKhongConBan") });

                ThongBao(Ngu.S("Cart_SanPhamKhongConBan"), "danger");
                return RedirectToAction("Index", "SanPham");
            }

            var gio = GioHangHienTai;
            LamMoiGioHang(gio);
            var item = TaoGioHangItem(sp, 1, "M", "100%", "100%", new List<string>());
            var dongCungTuyChon = gio.Items.FirstOrDefault(i => i.Khoa == item.Khoa);
            if (dongCungTuyChon != null && dongCungTuyChon.SoLuong >= 100)
            {
                const string loiSoLuong = "Số lượng mỗi sản phẩm không được vượt quá 100.";
                if (Request.IsAjaxRequest())
                    return Json(new { thanhCong = false, thongBao = loiSoLuong });

                ThongBao(loiSoLuong, "warning");
                return RedirectToAction("Index", "SanPham");
            }

            string loiTonKho = KiemTraTonKho(gio, sp, 1, null);
            if (loiTonKho != null)
            {
                if (Request.IsAjaxRequest())
                    return Json(new { thanhCong = false, thongBao = loiTonKho });

                ThongBao(loiTonKho, "warning");
                return RedirectToAction("Index", "SanPham");
            }

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
            var item = gio.Items.FirstOrDefault(i => i.Khoa == khoa);
            if (item == null)
                return TraVeLoiCapNhat(Ngu.S("Cart_SanPhamKhongTonTai"));

            if (soLuong > 100)
                return TraVeLoiCapNhat("Số lượng phải từ 1 đến 100.");

            if (soLuong > 0)
            {
                var sp = item.MaSP > 0 ? db.SanPhams.Find(item.MaSP) : null;
                if (sp == null || !sp.DangBan)
                    return TraVeLoiCapNhat(Ngu.S("Cart_SanPhamKhongTonTai"));

                string loiTonKho = KiemTraTonKho(gio, sp, soLuong, item.Khoa);
                if (loiTonKho != null)
                    return TraVeLoiCapNhat(loiTonKho);

                if (!TuyChonHopLe(item.Size, item.Duong, item.Da, item.Toppings))
                    return TraVeLoiCapNhat("Tùy chọn sản phẩm không hợp lệ.");

                item.TenSP = sp.TenSP;
                item.HinhAnh = sp.HinhAnh;
                item.GiaGoc = sp.GiaBanThucTe;
                item.DonGia = TinhDonGia(sp.GiaBanThucTe, item.Size, item.Toppings);
            }

            gio.CapNhatSoLuong(khoa, soLuong);
            PhienLamViec.LuuGioHang(Session, gio);

            if (Request.IsAjaxRequest())
            {
                item = gio.Items.FirstOrDefault(i => i.Khoa == khoa);
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
            LamMoiGioHang(gio);
            var ma = string.IsNullOrWhiteSpace(maGiamGia) ? null : maGiamGia.Trim().ToUpper();
            var km = ma == null ? null : db.KhuyenMais.FirstOrDefault(k => k.MaGiamGia == ma);

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
                decimal phiGiaoXemTruoc = (gio.Items.Count == 0 || gio.TamTinh >= MucMienPhiGiaoHang)
                                          ? 0 : PhiGiaoHangMacDinh;
                ThongBao(string.Format(Ngu.S("Cart_ApDungThanhCongFormat"),
                    km.MaGiamGia.ToUpper(), DinhDang.Tien(km.TinhTienGiam(gio.TamTinh, phiGiaoXemTruoc))));
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
            var canhBao = LamMoiGioHang(gio);
            if (canhBao.Count > 0)
            {
                ThongBao(string.Join(" ", canhBao.Distinct()), "warning");
                return RedirectToAction("Index");
            }

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
            var canhBao = LamMoiGioHang(gio);
            if (gio.Items.Count == 0)
            {
                ThongBao(Ngu.S("Cart_GioHangDangTrong"), "warning");
                return RedirectToAction("Index", "SanPham");
            }

            if (model == null)
            {
                model = new DatHangViewModel();
                ModelState.AddModelError("", "Thông tin đặt hàng không hợp lệ.");
            }

            if (canhBao.Count > 0)
                ModelState.AddModelError("", string.Join(" ", canhBao.Distinct()));

            if (model.HinhThucThanhToan != "COD" && model.HinhThucThanhToan != "QR")
                ModelState.AddModelError("HinhThucThanhToan", "Hình thức thanh toán không hợp lệ.");

            if (!ModelState.IsValid)
            {
                var vmLoi = TaoViewModelThanhToan(gio);
                vmLoi.TenNguoiNhan = model.TenNguoiNhan;
                vmLoi.DienThoai = model.DienThoai;
                vmLoi.DiaChi = model.DiaChi;
                vmLoi.GhiChu = model.GhiChu;
                vmLoi.HinhThucThanhToan = model.HinhThucThanhToan;
                ViewBag.Title = Ngu.S("Checkout_DatHang");
                return View(vmLoi);
            }

            var nd = NguoiDungHienTai;
            // QR: JS đã mô phỏng chờ 10s và báo thanh toán thành công -> đơn được đánh dấu đã thu đủ tiền ngay.
            // COD: chưa thu tiền, số tiền còn lại = tổng tiền, chỉ hết khi giao hàng xong.
            bool daThanhToanQR = model.HinhThucThanhToan == "QR" && model.DaThanhToanQR;

            var dh = new DonHang
            {
                MaND = nd.MaND,
                TenNguoiNhan = model.TenNguoiNhan,
                DienThoai = model.DienThoai,
                DiaChi = model.DiaChi,
                GhiChu = model.GhiChu,
                HinhThucThanhToan = model.HinhThucThanhToan,
                NgayDat = DateTime.Now,
                MaGiamGiaApDung = gio.MaGiamGia,
                TrangThai = DonHang.ChoXacNhan,
                DaThanhToan = daThanhToanQR
            };

            string loiTaoDon;
            if (!TaoDonHangVaTruKho(dh, gio.Items, out loiTaoDon))
            {
                ModelState.AddModelError("", loiTaoDon);
                var vmLoi = TaoViewModelThanhToan(gio);
                vmLoi.TenNguoiNhan = model.TenNguoiNhan;
                vmLoi.DienThoai = model.DienThoai;
                vmLoi.DiaChi = model.DiaChi;
                vmLoi.GhiChu = model.GhiChu;
                vmLoi.HinhThucThanhToan = model.HinhThucThanhToan;
                vmLoi.DaThanhToanQR = model.DaThanhToanQR;
                ViewBag.Title = Ngu.S("Checkout_DatHang");
                return View(vmLoi);
            }

            gio.XoaTatCa();
            PhienLamViec.LuuGioHang(Session, gio);

            return RedirectToAction("DatHangThanhCong", new { id = dh.MaDH });
        }

        // GET: /GioHang/DatHangThanhCong/1001
        [KiemTraDangNhap]
        public ActionResult DatHangThanhCong(int id)
        {
            var dh = db.DonHangs.Find(id);
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
            decimal phiGiao = gio.TamTinh >= MucMienPhiGiaoHang ? 0 : PhiGiaoHangMacDinh;
            decimal tienGiam = TinhTienGiam(gio, phiGiao);

            return new DatHangViewModel
            {
                GioHang = gio,
                TienGiam = tienGiam,
                PhiGiaoHang = phiGiao,
                TongTien = Math.Max(0, gio.TamTinh + phiGiao - tienGiam),
                HinhThucThanhToan = "COD"
            };
        }

        private decimal TinhTienGiam(GioHang gio, decimal phiGiaoHang)
        {
            if (string.IsNullOrWhiteSpace(gio.MaGiamGia)) return 0;

            var ma = gio.MaGiamGia.Trim().ToUpper();
            var km = db.KhuyenMais.FirstOrDefault(k => k.MaGiamGia == ma);
            if (km == null || !km.ConHieuLuc || gio.TamTinh < km.DonToiThieu) return 0;

            return Math.Min(km.TinhTienGiam(gio.TamTinh, phiGiaoHang),
                gio.TamTinh + phiGiaoHang);
        }

        private GioHangItem TaoGioHangItem(SanPham sp, int soLuong, string size, string duong,
                                           string da, List<string> toppings)
        {
            return new GioHangItem
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                HinhAnh = sp.HinhAnh,
                GiaGoc = sp.GiaBanThucTe,
                DonGia = TinhDonGia(sp.GiaBanThucTe, size, toppings),
                SoLuong = soLuong,
                Size = size,
                Duong = duong,
                Da = da,
                Toppings = toppings
            };
        }

        private string KiemTraTonKho(GioHang gio, SanPham sp, int soLuongThem, string khoaBoQua)
        {
            long soLuongTrongGio = gio.Items
                .Where(i => i.MaSP == sp.MaSP && i.Khoa != khoaBoQua)
                .Sum(i => (long)i.SoLuong);
            long soLuongYeuCau = soLuongTrongGio + soLuongThem;
            if (soLuongYeuCau > sp.SoLuongTon)
                return string.Format("Sản phẩm \"{0}\" chỉ còn {1} sản phẩm trong kho.", sp.TenSP, sp.SoLuongTon);
            return null;
        }

        private List<string> LamMoiGioHang(GioHang gio)
        {
            var canhBao = new List<string>();
            if (gio.Items.Count == 0) return canhBao;

            var maSanPhams = gio.Items.Select(i => i.MaSP).Distinct().ToList();
            var sanPhams = db.SanPhams.Where(s => maSanPhams.Contains(s.MaSP))
                .ToDictionary(s => s.MaSP);
            var soLuongTheoSanPham = gio.Items.GroupBy(i => i.MaSP)
                .ToDictionary(g => g.Key, g => g.Sum(i => (long)i.SoLuong));

            foreach (var item in gio.Items)
            {
                SanPham sp;
                if (!sanPhams.TryGetValue(item.MaSP, out sp) || !sp.DangBan)
                {
                    item.GiaGoc = 0;
                    item.DonGia = 0;
                    canhBao.Add(Ngu.S("Cart_SanPhamKhongTonTai"));
                    continue;
                }

                if (!TuyChonHopLe(item.Size, item.Duong, item.Da, item.Toppings))
                {
                    item.GiaGoc = 0;
                    item.DonGia = 0;
                    canhBao.Add("Tùy chọn sản phẩm không hợp lệ.");
                    continue;
                }

                item.TenSP = sp.TenSP;
                item.HinhAnh = sp.HinhAnh;
                item.GiaGoc = sp.GiaBanThucTe;
                item.DonGia = TinhDonGia(sp.GiaBanThucTe, item.Size, item.Toppings);
            }

            foreach (var entry in soLuongTheoSanPham)
            {
                SanPham sp;
                if (sanPhams.TryGetValue(entry.Key, out sp) && sp.DangBan && entry.Value > sp.SoLuongTon)
                    canhBao.Add(string.Format("Sản phẩm \"{0}\" chỉ còn {1} sản phẩm trong kho.", sp.TenSP, sp.SoLuongTon));
            }

            PhienLamViec.LuuGioHang(Session, gio);
            return canhBao;
        }

        private ActionResult TraVeLoiCapNhat(string loi)
        {
            if (Request.IsAjaxRequest())
                return Json(new { thanhCong = false, thongBao = loi });

            ThongBao(loi, "warning");
            return RedirectToAction("Index");
        }

        private bool TuyChonHopLe(string size, string duong, string da, IEnumerable<string> toppings)
        {
            return TuyChonSanPham.CacSize.Contains(size)
                   && TuyChonSanPham.CacMucDuong.Contains(duong)
                   && TuyChonSanPham.CacMucDa.Contains(da)
                   && (toppings ?? Enumerable.Empty<string>())
                       .All(t => !string.IsNullOrWhiteSpace(t)
                                 && TuyChonSanPham.CacTopping.ContainsKey(t));
        }

        private decimal TinhDonGia(decimal giaGoc, string size, IEnumerable<string> toppings)
        {
            return giaGoc + TuyChonSanPham.PhuThuSize(size)
                   + TuyChonSanPham.PhuThuTopping(toppings);
        }

        private bool TaoDonHangVaTruKho(DonHang dh, IList<GioHangItem> items, out string loi)
        {
            using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                if (dh == null || items == null || items.Count == 0)
                {
                    loi = "Giỏ hàng đang trống hoặc dữ liệu đơn hàng không hợp lệ.";
                    return false;
                }

                if (!dh.MaND.HasValue || !db.NguoiDungs.Any(n => n.MaND == dh.MaND.Value))
                {
                    loi = "Không tìm thấy tài khoản đặt hàng.";
                    return false;
                }

                if (items.Any(i => i == null || i.MaSP <= 0 || i.SoLuong < 1 || i.SoLuong > 100
                                   || !TuyChonHopLe(i.Size, i.Duong, i.Da, i.Toppings)))
                {
                    loi = "Số lượng hoặc tùy chọn sản phẩm không hợp lệ.";
                    return false;
                }

                var soLuongTheoSanPham = items.GroupBy(i => i.MaSP)
                    .ToDictionary(g => g.Key, g => g.Sum(i => (long)i.SoLuong));
                var maSanPhams = soLuongTheoSanPham.Keys.ToList();
                var sanPhams = db.SanPhams.Where(s => maSanPhams.Contains(s.MaSP))
                    .ToDictionary(s => s.MaSP);

                foreach (var entry in soLuongTheoSanPham)
                {
                    SanPham sp;
                    if (!sanPhams.TryGetValue(entry.Key, out sp) || !sp.DangBan)
                    {
                        loi = "Sản phẩm không tồn tại hoặc đã ngừng bán.";
                        return false;
                    }

                    if (entry.Value > sp.SoLuongTon)
                    {
                        loi = string.Format("Sản phẩm \"{0}\" chỉ còn {1} sản phẩm trong kho.",
                            sp.TenSP, sp.SoLuongTon);
                        return false;
                    }
                }

                decimal tamTinh = 0;
                dh.ChiTietDonHangs.Clear();
                foreach (var item in items)
                {
                    var sp = sanPhams[item.MaSP];
                    decimal donGia = TinhDonGia(sp.GiaBanThucTe, item.Size, item.Toppings);
                    tamTinh += donGia * item.SoLuong;
                    dh.ChiTietDonHangs.Add(new ChiTietDonHang
                    {
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        SoLuong = item.SoLuong,
                        DonGia = donGia,
                        TuyChon = item.MoTaTuyChon
                    });
                }

                decimal phiGiaoHang = tamTinh >= MucMienPhiGiaoHang ? 0 : PhiGiaoHangMacDinh;
                decimal tienGiam = 0;
                string maGiamGia = string.IsNullOrWhiteSpace(dh.MaGiamGiaApDung)
                    ? null
                    : dh.MaGiamGiaApDung.Trim().ToUpper();

                if (maGiamGia != null)
                {
                    var km = db.KhuyenMais.FirstOrDefault(k => k.MaGiamGia == maGiamGia);
                    if (km == null)
                    {
                        loi = "Mã giảm giá không tồn tại.";
                        return false;
                    }
                    if (!km.ConHieuLuc)
                    {
                        loi = "Mã giảm giá đã hết hạn hoặc chưa được kích hoạt.";
                        return false;
                    }
                    if (tamTinh < km.DonToiThieu)
                    {
                        loi = string.Format("Mã {0} chỉ áp dụng cho đơn từ {1}.",
                            km.MaGiamGia.ToUpper(), DinhDang.Tien(km.DonToiThieu));
                        return false;
                    }

                    tienGiam = Math.Min(km.TinhTienGiam(tamTinh, phiGiaoHang),
                        tamTinh + phiGiaoHang);
                }

                dh.TamTinh = tamTinh;
                dh.PhiGiaoHang = phiGiaoHang;
                dh.TienGiam = tienGiam;
                dh.TongTien = Math.Max(0, tamTinh + phiGiaoHang - tienGiam);
                dh.MaGiamGiaApDung = maGiamGia;
                dh.SoTienConLai = dh.DaThanhToan ? 0 : dh.TongTien;

                foreach (var entry in soLuongTheoSanPham)
                {
                    var sp = sanPhams[entry.Key];
                    sp.SoLuongTon -= (int)entry.Value;
                    sp.LuotBan += (int)entry.Value;
                }

                db.DonHangs.Add(dh);
                db.SaveChanges();
                transaction.Commit();
                loi = null;
                return true;
            }
        }

        #endregion
    }
}
