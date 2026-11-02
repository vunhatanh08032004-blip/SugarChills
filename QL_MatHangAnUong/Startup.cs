using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(QL_MatHangAnUong.Startup))]

namespace QL_MatHangAnUong
{
    /// <summary>
    /// OWIN Startup — chỉ dùng để cấu hình OAuth (Google / Facebook).
    /// Xác thực người dùng vẫn dùng Session (PhienLamViec), không dùng ASP.NET Identity.
    /// </summary>
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
