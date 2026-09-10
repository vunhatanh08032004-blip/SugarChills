using System;
using System.Web;
using System.Web.Mvc;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// Đổi ngôn ngữ hiển thị của toàn bộ website (Tiếng Việt / English).
    /// Lựa chọn được lưu vào cookie "scNgonNgu" và BaseController sẽ đọc cookie này ở mỗi
    /// request để đặt Thread.CurrentUICulture — nhờ đó mọi @Resources.Strings.Xxx trong View
    /// tự động lấy đúng bản dịch, không cần Controller nào khác biết đến cơ chế này.
    /// </summary>
    public class NgonNguController : Controller
    {
        // GET: /NgonNgu/Doi?ma=en
        public ActionResult Doi(string ma)
        {
            // Chỉ chấp nhận 2 giá trị hợp lệ; giá trị lạ/rỗng -> mặc định về tiếng Việt.
            if (ma != "en") ma = "vi";

            Response.Cookies.Add(new HttpCookie("scNgonNgu", ma)
            {
                Expires = DateTime.Now.AddYears(1)
            });

            // Quay lại đúng trang vừa bấm nút đổi ngôn ngữ (giống cách DoiNoiBat đang làm).
            return Redirect(Request.UrlReferrer != null
                ? Request.UrlReferrer.ToString()
                : Url.Action("Index", "Home"));
        }
    }
}
