using System.Web.Mvc;
using System.Web.Routing;
using QL_MatHangAnUong.Helpers;
using QL_MatHangAnUong.Models;

namespace QL_MatHangAnUong.Filters
{
    /// <summary>
    /// Bắt buộc người dùng phải ĐĂNG NHẬP mới vào được action (giỏ hàng, đặt hàng, đơn hàng của tôi...).
    /// Chưa đăng nhập thì chuyển về trang đăng nhập kèm returnUrl để quay lại đúng chỗ đang dở.
    /// </summary>
    public class KiemTraDangNhapAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Trong ActionExecutingContext, HttpContext là HttpContextBase
            // nên Session ở đây đã là HttpSessionStateBase, dùng trực tiếp được.
            var session = filterContext.HttpContext.Session;
            var request = filterContext.HttpContext.Request;

            if (!PhienLamViec.DaDangNhap(session))
            {
                // Nếu là AJAX thì trả JSON để JavaScript xử lý, không redirect
                if (request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new { thanhCong = false, canDangNhap = true, thongBao = "Bạn cần đăng nhập để tiếp tục." },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    return;
                }

                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "TaiKhoan",
                        action = "DangNhap",
                        returnUrl = request.RawUrl
                    }));
            }

            base.OnActionExecuting(filterContext);
        }
    }

    /// <summary>
    /// Bắt buộc người dùng phải là NGƯỜI BÁN (Seller) mới vào được khu vực quản trị.
    /// Khách hàng cố tình gõ URL /QuanLySanPham sẽ bị chặn.
    /// </summary>
    public class KiemTraNguoiBanAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var request = filterContext.HttpContext.Request;
            var nguoiDung = PhienLamViec.LayNguoiDung(session);

            if (nguoiDung == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "TaiKhoan",
                        action = "DangNhap",
                        returnUrl = request.RawUrl
                    }));
                return;
            }

            if (nguoiDung.VaiTro != NguoiDung.RoleSeller)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "TaiKhoan",
                        action = "KhongCoQuyen"
                    }));
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
