using System.Web.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Helpers;
using QL_MatHangAnUong.Models;
using QL_MatHangAnUong.Models.ViewModels;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// Đăng ký - Đăng nhập - Đăng xuất (rubric mục 2).
    /// Cơ chế: lưu đối tượng NguoiDung vào Session, phân quyền bằng filter tự viết
    /// (KiemTraDangNhapAttribute / KiemTraNguoiBanAttribute).
    /// </summary>
    public class TaiKhoanController : BaseController
    {
        // GET: /TaiKhoan/DangKy
        public ActionResult DangKy()
        {
            if (NguoiDungHienTai != null) return RedirectToAction("Index", "Home");

            ViewBag.Title = Ngu.S("Auth_DangKyTitle");
            return View(new DangKyViewModel());
        }

        // POST: /TaiKhoan/DangKy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(DangKyViewModel model)
        {
            // Kiểm tra trùng email — validation nghiệp vụ, Data Annotation không làm được
            if (!string.IsNullOrWhiteSpace(model.Email) && db.NguoiDungs.Any(n => n.Email == model.Email))
                ModelState.AddModelError("Email", Ngu.S("Auth_EmailDaTonTai"));

            if (!ModelState.IsValid)
            {
                ViewBag.Title = Ngu.S("Auth_DangKyTitle");
                return View(model);
            }

            var nd = new NguoiDung
            {
                HoTen = model.HoTen.Trim(),
                Email = model.Email.Trim(),
                MatKhau = model.MatKhau,          // Lưu ý: đồ án demo nên lưu thẳng.
                DienThoai = model.DienThoai,      // Thực tế cần băm mật khẩu (SHA256 + salt).
                DiaChi = model.DiaChi,
                VaiTro = NguoiDung.RoleCustomer,
                NgayDangKy = DateTime.Now
            };
            db.NguoiDungs.Add(nd);
            db.SaveChanges();

            // Đăng ký xong đăng nhập luôn cho tiện
            PhienLamViec.DangNhap(Session, nd);
            ThongBao(Ngu.S("Auth_DangKyThanhCong"));

            return RedirectToAction("Index", "Home");
        }

        // GET: /TaiKhoan/DangNhap
        public ActionResult DangNhap(string returnUrl)
        {
            if (NguoiDungHienTai != null) return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Title = Ngu.S("Common_DangNhap");
            return View(new DangNhapViewModel());
        }

        // POST: /TaiKhoan/DangNhap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(DangNhapViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Title = Ngu.S("Common_DangNhap");

            if (!ModelState.IsValid) return View(model);

            var nd = db.NguoiDungs.FirstOrDefault(n => n.Email == model.Email && n.MatKhau == model.MatKhau);
            if (nd == null)
            {
                ModelState.AddModelError("", Ngu.S("Auth_SaiTaiKhoanMatKhau"));
                return View(model);
            }

            PhienLamViec.DangNhap(Session, nd);
            ThongBao(string.Format(Ngu.S("Auth_XinChaoFormat"), nd.HoTen));

            // Người bán vào thẳng khu quản trị
            if (nd.VaiTro == NguoiDung.RoleSeller && string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", "ThongKe");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // POST: /TaiKhoan/DangXuat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangXuat()
        {
            PhienLamViec.DangXuat(Session);
            return RedirectToAction("Index", "Home");
        }

        // GET: /TaiKhoan/ThongTin
        [KiemTraDangNhap]
        public ActionResult ThongTin()
        {
            ViewBag.Title = Ngu.S("Header_TaiKhoanCuaToi");
            var maND = NguoiDungHienTai.MaND;
            ViewBag.DonHangs = db.DonHangs.Where(d => d.MaND == maND).OrderByDescending(d => d.NgayDat).ToList();
            return View(NguoiDungHienTai);
        }
        // GET: /TaiKhoan/CapNhatThongTin
        [KiemTraDangNhap]
        public ActionResult CapNhatThongTin()
        {
            ViewBag.Title = "Chỉnh sửa thông tin cá nhân";

            var nd = NguoiDungHienTai;
            var model = new CapNhatThongTinViewModel
            {
                MaND = nd.MaND,
                HoTen = nd.HoTen,
                // Tài khoản tạo từ Google/Facebook đang mang SĐT tạm -> để trống cho khách tự nhập
                DienThoai = nd.DienThoai == SoDienThoaiTam ? "" : nd.DienThoai,
                DiaChi = nd.DiaChi
            };
            return View(model);
        }

        // POST: /TaiKhoan/CapNhatThongTin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [KiemTraDangNhap]
        public ActionResult CapNhatThongTin(CapNhatThongTinViewModel model)
        {
            ViewBag.Title = "Chỉnh sửa thông tin cá nhân";

            // Chặn trường hợp cố tình sửa MaND trong request để đụng vào tài khoản khác
            model.MaND = NguoiDungHienTai.MaND;

            if (!ModelState.IsValid)
                return View(model);

            var nd = db.NguoiDungs.Find(model.MaND);
            if (nd == null) return RedirectToAction("DangNhap");
            nd.HoTen = model.HoTen;
            nd.DienThoai = model.DienThoai;
            nd.DiaChi = model.DiaChi;
            db.SaveChanges();

            // Đồng bộ lại Session để _Layout / trang Thông tin hiển thị dữ liệu mới ngay,
            // không cần đăng nhập lại.
            var ndMoi = db.NguoiDungs.Find(model.MaND);
            PhienLamViec.DangNhap(Session, ndMoi);

            ThongBao("Cập nhật thông tin cá nhân thành công.");
            return RedirectToAction("ThongTin");
        }

        // GET: /TaiKhoan/DoiMatKhau
        [KiemTraDangNhap]
        public ActionResult DoiMatKhau()
        {
            ViewBag.Title = "Đổi mật khẩu";
            return View(new DoiMatKhauViewModel());
        }

        // POST: /TaiKhoan/DoiMatKhau
        [HttpPost]
        [ValidateAntiForgeryToken]
        [KiemTraDangNhap]
        public ActionResult DoiMatKhau(DoiMatKhauViewModel model)
        {
            ViewBag.Title = "Đổi mật khẩu";

            if (!ModelState.IsValid)
                return View(model);

            var maND = NguoiDungHienTai.MaND;
            var matKhauCu = model.MatKhauHienTai;
            var nd = db.NguoiDungs.FirstOrDefault(n => n.MaND == maND && n.MatKhau == matKhauCu);
            bool thanhCong = (nd != null);
            if (thanhCong)
            {
                nd.MatKhau = model.MatKhauMoi;
                db.SaveChanges();
            }
            if (!thanhCong)
            {
                ModelState.AddModelError("MatKhauHienTai", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            // Đồng bộ lại Session với mật khẩu mới
            var ndMoi = db.NguoiDungs.Find(NguoiDungHienTai.MaND);
            PhienLamViec.DangNhap(Session, ndMoi);

            ThongBao("Đổi mật khẩu thành công.");
            return RedirectToAction("ThongTin");
        }

        // =====================================================================
        //  QUÊN MẬT KHẨU BẰNG MÃ OTP QUA EMAIL
        //  Bước 1: nhập email -> GuiMaOtp gửi mã 6 số (hiệu lực 5 phút)
        //  Bước 2: nhập mã + mật khẩu mới -> QuenMatKhau (POST) kiểm tra rồi đổi mật khẩu
        //  Mã OTP chỉ lưu trong Session (không cần thêm bảng/EDMX).
        // =====================================================================
        private const string SkEmail = "QMK_Email";
        private const string SkMa = "QMK_Ma";
        private const string SkHetHan = "QMK_HetHan";
        private const string SkDaXacMinh = "QMK_DaXacMinh";   // đã nhập đúng OTP -> được đặt mật khẩu mới
        private const string SkSai = "QMK_SoLanSai";
        private const string SkSoLanGui = "QMK_SoLanGui";
        private const string SkLanGuiCuoi = "QMK_LanGuiCuoi";
        private const int OtpHieuLucPhut = 5;
        private const int OtpToiDaSai = 5;
        private const int OtpToiDaGui = 5;
        private const int OtpCho = 60;   // giây giữa 2 lần gửi

        private void XoaPhienOtp()
        {
            Session.Remove(SkEmail); Session.Remove(SkMa); Session.Remove(SkHetHan);
            Session.Remove(SkSai); Session.Remove(SkSoLanGui); Session.Remove(SkLanGuiCuoi); Session.Remove(SkDaXacMinh);
        }

        // GET: /TaiKhoan/QuenMatKhau
        public ActionResult QuenMatKhau()
        {
            if (NguoiDungHienTai != null) return RedirectToAction("Index", "Home");
            ViewBag.Title = "Quên mật khẩu";
            string email = Session[SkEmail] as string;
            ViewBag.Buoc = BuocHienTai();        // 1 = nhập email, 2 = nhập OTP, 3 = mật khẩu mới
            return View(new QuenMatKhauViewModel { Email = email });
        }

        private int BuocHienTai()
        {
            if (Session[SkEmail] == null) return 1;
            return (Session[SkDaXacMinh] is bool && (bool)Session[SkDaXacMinh]) ? 3 : 2;
        }

        // GET: /TaiKhoan/DoiEmailQuenMatKhau — quay lại bước 1
        public ActionResult DoiEmailQuenMatKhau()
        {
            XoaPhienOtp();
            return RedirectToAction("QuenMatKhau");
        }

        // POST: /TaiKhoan/GuiMaOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiMaOtp(string email)
        {
            if (NguoiDungHienTai != null) return RedirectToAction("Index", "Home");
            email = (email ?? "").Trim();

            if (email.Length == 0 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            {
                ThongBao("Vui lòng nhập email hợp lệ.", "danger");
                return RedirectToAction("QuenMatKhau");
            }

            if (!GuiMail.DaCauHinh())
            {
                ThongBao("Chức năng gửi mã chưa được cấu hình (thiếu SmtpUser/SmtpPass trong Secrets.config).", "danger");
                return RedirectToAction("QuenMatKhau");
            }

            int soLanGui = Session[SkSoLanGui] is int ? (int)Session[SkSoLanGui] : 0;
            if (soLanGui >= OtpToiDaGui)
            {
                ThongBao("Bạn đã yêu cầu mã quá nhiều lần. Vui lòng thử lại sau.", "warning");
                return RedirectToAction("QuenMatKhau");
            }

            var lanCuoi = Session[SkLanGuiCuoi] as DateTime?;
            if (lanCuoi.HasValue)
            {
                double daQua = (DateTime.Now - lanCuoi.Value).TotalSeconds;
                if (daQua < OtpCho)
                {
                    ThongBao("Vui lòng chờ " + (int)Math.Ceiling(OtpCho - daQua) + " giây rồi gửi lại mã.", "warning");
                    return RedirectToAction("QuenMatKhau");
                }
            }

            var nd = db.NguoiDungs.FirstOrDefault(n => n.Email == email);
            string ma = null;
            if (nd != null && nd.VaiTro != NguoiDung.RoleSeller)
            {
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    var b = new byte[4];
                    rng.GetBytes(b);
                    ma = (BitConverter.ToUInt32(b, 0) % 1000000).ToString("D6");
                }
                try
                {
                    GuiMail.Gui(nd.Email, "SugarChills - Mã xác nhận đặt lại mật khẩu",
                        "<div style=\"font-family:Arial,sans-serif;font-size:15px;color:#333\">" +
                        "<p>Xin chào <b>" + System.Net.WebUtility.HtmlEncode(nd.HoTen) + "</b>,</p>" +
                        "<p>Mã xác nhận đặt lại mật khẩu của bạn là:</p>" +
                        "<p style=\"font-size:30px;font-weight:bold;letter-spacing:6px;color:#e8517f\">" + ma + "</p>" +
                        "<p>Mã có hiệu lực trong " + OtpHieuLucPhut + " phút. Nếu bạn không yêu cầu, hãy bỏ qua email này.</p></div>");
                }
                catch (Exception)
                {
                    ThongBao("Không gửi được email. Vui lòng kiểm tra cấu hình SMTP hoặc thử lại sau.", "danger");
                    return RedirectToAction("QuenMatKhau");
                }
            }

            // Luôn vào bước 2 và báo chung — không tiết lộ email có tồn tại hay không
            Session[SkEmail] = email;
            Session[SkMa] = ma;
            Session[SkHetHan] = DateTime.Now.AddMinutes(OtpHieuLucPhut);
            Session[SkSai] = 0;
            Session[SkSoLanGui] = soLanGui + 1;
            Session[SkLanGuiCuoi] = DateTime.Now;

            ThongBao("Nếu email này đã đăng ký, mã OTP đã được gửi (hiệu lực " + OtpHieuLucPhut + " phút). Hãy kiểm tra cả hòm thư Spam.", "info");
            return RedirectToAction("QuenMatKhau");
        }

        // POST: /TaiKhoan/XacNhanOtp — bước 2: chỉ kiểm tra mã OTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanOtp(string otp)
        {
            ViewBag.Title = "Quên mật khẩu";
            string emailPhien = Session[SkEmail] as string;
            if (emailPhien == null) return RedirectToAction("QuenMatKhau");

            var model = new QuenMatKhauViewModel { Email = emailPhien };
            ViewBag.Buoc = 2;

            string maPhien = Session[SkMa] as string;
            var hetHan = Session[SkHetHan] as DateTime?;
            int soSai = Session[SkSai] is int ? (int)Session[SkSai] : 0;
            otp = (otp ?? "").Trim();

            if (soSai >= OtpToiDaSai || !hetHan.HasValue || DateTime.Now > hetHan.Value)
            {
                ModelState.AddModelError("", "Mã OTP đã hết hạn hoặc nhập sai quá nhiều lần. Vui lòng bấm \"Đổi email / gửi lại mã\".");
                return View("QuenMatKhau", model);
            }
            if (maPhien == null || otp != maPhien)
            {
                Session[SkSai] = soSai + 1;
                ModelState.AddModelError("", "Mã OTP không đúng.");
                return View("QuenMatKhau", model);
            }

            // OTP đúng -> mở bước 3, cho thêm 10 phút để đặt mật khẩu
            Session[SkDaXacMinh] = true;
            Session[SkMa] = null;
            Session[SkHetHan] = DateTime.Now.AddMinutes(10);
            return RedirectToAction("QuenMatKhau");
        }

        // POST: /TaiKhoan/QuenMatKhau — bước 3: đặt mật khẩu mới (chỉ khi đã xác minh OTP)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuenMatKhau(QuenMatKhauViewModel model)
        {
            ViewBag.Title = "Quên mật khẩu";
            string emailPhien = Session[SkEmail] as string;
            var hetHan = Session[SkHetHan] as DateTime?;
            bool daXacMinh = Session[SkDaXacMinh] is bool && (bool)Session[SkDaXacMinh];

            if (emailPhien == null || !daXacMinh || !hetHan.HasValue || DateTime.Now > hetHan.Value)
            {
                XoaPhienOtp();
                ThongBao("Phiên xác minh đã hết hạn. Vui lòng yêu cầu mã mới.", "warning");
                return RedirectToAction("QuenMatKhau");
            }

            model.Email = emailPhien;          // luôn dùng email đã xác minh, bỏ qua giá trị client gửi lên
            ModelState.Remove("Email");
            ViewBag.Buoc = 3;
            if (!ModelState.IsValid) return View(model);

            var nd = db.NguoiDungs.FirstOrDefault(n => n.Email == emailPhien);
            if (nd == null || nd.VaiTro == NguoiDung.RoleSeller)
            {
                XoaPhienOtp();
                ThongBao("Không thể đặt lại mật khẩu cho tài khoản này.", "danger");
                return RedirectToAction("QuenMatKhau");
            }

            nd.MatKhau = model.MatKhauMoi;
            db.SaveChanges();
            XoaPhienOtp();

            ThongBao("Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.");
            return RedirectToAction("DangNhap");
        }

        // =====================================================================
        //  ĐĂNG NHẬP BẰNG GOOGLE / FACEBOOK (OAuth 2.0)
        //  Helper: Helpers/DangNhapMangXaHoi.cs — cấu hình key trong Web.config
        // =====================================================================
        private const string KeyOAuthState = "OAuthState";
        private const string KeyOAuthReturnUrl = "OAuthReturnUrl";

        /// <summary>Địa chỉ Google/Facebook gọi lại — phải khai báo y hệt trong trang quản lý ứng dụng.</summary>
        private string UrlGoiLai(string action)
        {
            return Url.Action(action, "TaiKhoan", null, Request.Url.Scheme);
        }

        // GET: /TaiKhoan/DangNhapNgoai?nhaCungCap=google&returnUrl=/GioHang
        public ActionResult DangNhapNgoai(string nhaCungCap, string returnUrl)
        {
            nhaCungCap = (nhaCungCap ?? "").ToLower();
            if (nhaCungCap != "google" && nhaCungCap != "facebook")
                return RedirectToAction("DangNhap");

            if (!DangNhapMangXaHoi.DaCauHinh(nhaCungCap))
            {
                ThongBao("Chưa cấu hình đăng nhập " + (nhaCungCap == "google" ? "Google" : "Facebook")
                       + ". Hãy điền Client ID / Secret trong Web.config (xem hướng dẫn trong file).", "warning");
                return RedirectToAction("DangNhap", new { returnUrl });
            }

            // "state" ngẫu nhiên lưu trong Session để chống giả mạo yêu cầu (CSRF)
            string state = Guid.NewGuid().ToString("N");
            Session[KeyOAuthState] = state;
            Session[KeyOAuthReturnUrl] = returnUrl;

            string url = nhaCungCap == "google"
                ? DangNhapMangXaHoi.TaoUrlGoogle(UrlGoiLai("GoogleCallback"), state)
                : DangNhapMangXaHoi.TaoUrlFacebook(UrlGoiLai("FacebookCallback"), state);
            return Redirect(url);
        }

        // GET: /TaiKhoan/GoogleCallback?code=...&state=...
        public async Task<ActionResult> GoogleCallback(string code, string state, string error)
        {
            return await XuLyGoiLai("Google", code, state, error,
                () => DangNhapMangXaHoi.LayThongTinGoogle(code, UrlGoiLai("GoogleCallback")));
        }

        // GET: /TaiKhoan/FacebookCallback?code=...&state=...
        public async Task<ActionResult> FacebookCallback(string code, string state, string error)
        {
            return await XuLyGoiLai("Facebook", code, state, error,
                () => DangNhapMangXaHoi.LayThongTinFacebook(code, UrlGoiLai("FacebookCallback")));
        }

        /// <summary>Kiểm tra state, lấy thông tin, tìm/tạo tài khoản rồi đăng nhập.</summary>
        private async Task<ActionResult> XuLyGoiLai(string ten, string code, string state, string error,
                                                    Func<Task<ThongTinMangXaHoi>> layThongTin)
        {
            string stateDaLuu = Session[KeyOAuthState] as string;
            string returnUrl = Session[KeyOAuthReturnUrl] as string;
            Session.Remove(KeyOAuthState);
            Session.Remove(KeyOAuthReturnUrl);

            // Người dùng bấm "Hủy" trên trang Google/Facebook
            if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
            {
                ThongBao("Bạn đã hủy đăng nhập bằng " + ten + ".", "info");
                return RedirectToAction("DangNhap", new { returnUrl });
            }

            if (string.IsNullOrEmpty(stateDaLuu) || stateDaLuu != state)
            {
                ThongBao("Phiên đăng nhập " + ten + " không hợp lệ hoặc đã hết hạn. Vui lòng thử lại.", "danger");
                return RedirectToAction("DangNhap", new { returnUrl });
            }

            ThongTinMangXaHoi tt;
            try
            {
                tt = await layThongTin();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[OAuth " + ten + "] " + ex);
                ThongBao("Không kết nối được với " + ten + ". Vui lòng thử lại sau.", "danger");
                return RedirectToAction("DangNhap", new { returnUrl });
            }

            // Facebook có thể không trả email (tài khoản đăng ký bằng SĐT) -> tạo email nội bộ theo ID
            string email = !string.IsNullOrWhiteSpace(tt.Email) && tt.EmailDaXacMinh
                ? tt.Email.Trim().ToLower()
                : tt.NhaCungCap.ToLower() + "_" + tt.Id + "@sugarchills.local";

            bool laTaiKhoanMoi = false;
            var nd = db.NguoiDungs.FirstOrDefault(n => n.Email == email);
            if (nd == null)
            {
                // Tạo tài khoản Khách hàng mới.
                // Bảng NguoiDung bắt buộc có SĐT + mật khẩu: đặt SĐT tạm "0000000000" (đúng CHECK)
                // và mật khẩu ngẫu nhiên; khách sẽ được yêu cầu cập nhật SĐT ngay sau đây.
                string hoTen = string.IsNullOrWhiteSpace(tt.HoTen) ? email.Split('@')[0] : tt.HoTen.Trim();
                nd = new NguoiDung
                {
                    HoTen = hoTen.Length > 100 ? hoTen.Substring(0, 100) : hoTen,
                    Email = email,
                    MatKhau = Guid.NewGuid().ToString("N").Substring(0, 16),
                    DienThoai = SoDienThoaiTam,
                    VaiTro = NguoiDung.RoleCustomer,
                    NgayDangKy = DateTime.Now
                };
                try
                {
                    db.NguoiDungs.Add(nd);
                    db.SaveChanges();
                    laTaiKhoanMoi = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("[OAuth tạo tài khoản] " + ex);
                    ThongBao("Không tạo được tài khoản từ " + ten + ". Vui lòng đăng ký bằng email.", "danger");
                    return RedirectToAction("DangKy");
                }
            }

            PhienLamViec.DangNhap(Session, nd);

            if (laTaiKhoanMoi || nd.DienThoai == SoDienThoaiTam)
            {
                ThongBao("Chào " + nd.HoTen + "! Bạn đã đăng nhập bằng " + ten
                       + ". Vui lòng cập nhật số điện thoại để đặt hàng và nhận hàng.", "warning");
                return RedirectToAction("CapNhatThongTin");
            }

            ThongBao(string.Format(Ngu.S("Auth_XinChaoFormat"), nd.HoTen));
            if (nd.VaiTro == NguoiDung.RoleSeller && string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", "ThongKe");
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>SĐT tạm cho tài khoản tạo từ Google/Facebook (thỏa CHECK 0 + 9 chữ số).</summary>
        public const string SoDienThoaiTam = "0000000000";

        // GET: /TaiKhoan/KhongCoQuyen
        public ActionResult KhongCoQuyen()
        {
            ViewBag.Title = Ngu.S("Auth_KhongCoQuyenTruyCap");
            return View();
        }
    }
}
