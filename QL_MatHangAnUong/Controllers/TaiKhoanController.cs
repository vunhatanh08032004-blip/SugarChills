using System.Web.Mvc;
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

            ViewBag.Title = "Đăng ký tài khoản";
            return View(new DangKyViewModel());
        }

        // POST: /TaiKhoan/DangKy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(DangKyViewModel model)
        {
            // Kiểm tra trùng email — validation nghiệp vụ, Data Annotation không làm được
            if (!string.IsNullOrWhiteSpace(model.Email) && KhoDuLieu.EmailDaTonTai(model.Email))
                ModelState.AddModelError("Email", "Email này đã được đăng ký. Bạn hãy đăng nhập hoặc dùng email khác.");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Đăng ký tài khoản";
                return View(model);
            }

            var nd = new NguoiDung
            {
                HoTen = model.HoTen.Trim(),
                Email = model.Email.Trim(),
                MatKhau = model.MatKhau,          // Lưu ý: đồ án demo nên lưu thẳng.
                DienThoai = model.DienThoai,      // Thực tế cần băm mật khẩu (SHA256 + salt).
                DiaChi = model.DiaChi,
                VaiTro = NguoiDung.RoleCustomer
            };
            KhoDuLieu.ThemNguoiDung(nd);

            // Đăng ký xong đăng nhập luôn cho tiện
            PhienLamViec.DangNhap(Session, nd);
            ThongBao("Đăng ký thành công. Chào mừng bạn đến với SugarChills!");

            return RedirectToAction("Index", "Home");
        }

        // GET: /TaiKhoan/DangNhap
        public ActionResult DangNhap(string returnUrl)
        {
            if (NguoiDungHienTai != null) return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Title = "Đăng nhập";
            return View(new DangNhapViewModel());
        }

        // POST: /TaiKhoan/DangNhap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(DangNhapViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Title = "Đăng nhập";

            if (!ModelState.IsValid) return View(model);

            var nd = KhoDuLieu.KiemTraDangNhap(model.Email, model.MatKhau);
            if (nd == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            PhienLamViec.DangNhap(Session, nd);
            ThongBao(string.Format("Xin chào {0}!", nd.HoTen));

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
            ViewBag.Title = "Tài khoản của tôi";
            ViewBag.DonHangs = KhoDuLieu.LayDonHangCuaKhach(NguoiDungHienTai.MaND);
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
                DienThoai = nd.DienThoai,
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

            KhoDuLieu.CapNhatThongTinNguoiDung(model.MaND, model.HoTen, model.DienThoai, model.DiaChi);

            // Đồng bộ lại Session để _Layout / trang Thông tin hiển thị dữ liệu mới ngay,
            // không cần đăng nhập lại.
            var ndMoi = KhoDuLieu.LayNguoiDung(model.MaND);
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

            bool thanhCong = KhoDuLieu.DoiMatKhau(NguoiDungHienTai.MaND, model.MatKhauHienTai, model.MatKhauMoi);
            if (!thanhCong)
            {
                ModelState.AddModelError("MatKhauHienTai", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            // Đồng bộ lại Session với mật khẩu mới
            var ndMoi = KhoDuLieu.LayNguoiDung(NguoiDungHienTai.MaND);
            PhienLamViec.DangNhap(Session, ndMoi);

            ThongBao("Đổi mật khẩu thành công.");
            return RedirectToAction("ThongTin");
        }

        // GET: /TaiKhoan/KhongCoQuyen
        public ActionResult KhongCoQuyen()
        {
            ViewBag.Title = "Không có quyền truy cập";
            return View();
        }
    }
}
