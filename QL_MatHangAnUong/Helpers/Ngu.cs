using System.Web;

namespace QL_MatHangAnUong.Helpers
{
    /// <summary>
    /// Đọc chuỗi đa ngôn ngữ (VI/EN) từ App_GlobalResources (Strings.resx / Strings.en.resx)
    /// để dùng trong code Controller/Model.
    ///
    /// LÝ DO CẦN LỚP NÀY: App_GlobalResources được ASP.NET biên dịch ĐỘNG lúc chạy
    /// (khi có request đầu tiên), KHÔNG phải lúc MSBuild build project. Controller và Model
    /// (.cs) thì luôn được MSBuild biên dịch TRƯỚC, lúc bấm Build — nên viết trực tiếp
    /// "Resources.Strings.TenKey" trong các file này sẽ báo lỗi CS0103 "The name 'Resources'
    /// does not exist in the current context" vì class đó chưa hề tồn tại ở thời điểm build.
    /// (View .cshtml thì KHÔNG bị lỗi này vì mặc định cũng được biên dịch lúc chạy, cùng lúc
    /// với App_GlobalResources — nên "@Resources.Strings.TenKey" trong .cshtml vẫn viết bình
    /// thường, không cần đổi.)
    ///
    /// HttpContext.GetGlobalResourceObject tra cứu resource THEO TÊN CHUỖI lúc chạy (không
    /// cần biết trước class Resources.Strings khi biên dịch), và tự lấy đúng ngôn ngữ theo
    /// Thread.CurrentUICulture — đúng cái mà BaseController đã set theo cookie "scNgonNgu".
    ///
    /// Cách dùng: thay "Resources.Strings.TenKey" bằng "Ngu.S("TenKey")" trong Controller/Model.
    /// </summary>
    public static class Ngu
    {
        public static string S(string key)
        {
            var gia = HttpContext.GetGlobalResourceObject("Strings", key) as string;
            return gia ?? key;
        }
    }
}
