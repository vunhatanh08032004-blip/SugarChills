using System.Web.Optimization;

namespace QL_MatHangAnUong
{
    public class BundleConfig
    {
        /// <summary>
        /// Dự án SugarChills nhúng thẳng file CSS/JS trong _Layout.cshtml
        /// (xem thẻ &lt;link&gt; và &lt;script&gt;) nên không dùng bundling.
        ///
        /// Lý do: Scripts.Render/Styles.Render thêm một lớp trung gian dễ gây lỗi
        /// (NullReferenceException khi bundle không phân giải được) và khó debug,
        /// trong khi nhúng trực tiếp thì đường dẫn nhìn thấy ngay trong View Source.
        ///
        /// Hàm vẫn được giữ và vẫn được Global.asax gọi để dễ mở rộng về sau.
        /// </summary>
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = false;
        }
    }
}
