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

        // GET: /TaiKhoan/QuenMatKhau
        public ActionResult QuenMatKhau()
        {
            if (NguoiDungHienTai != null) return RedirectToAction("Index", "Home");
            ViewBag.Title = "Quên mật khẩu";
            return View(new QuenMatKhauViewModel());
        }

        // POST: /TaiKhoan/QuenMatKhau
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuenMatKhau(QuenMatKhauViewModel model)
        {
            ViewBag.Title = "Quên mật khẩu";
            if (!ModelState.IsValid) return View(model);

            var nd = db.NguoiDungs.FirstOrDefault(n => n.Email == model.Email.Trim());
            if (nd == null)
            {
                ModelState.AddModelError("Email", "Email này chưa được đăng ký trong hệ thống.");
                return View(model);
            }

            nd.MatKhau = model.MatKhauMoi;
            db.SaveChanges();

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
