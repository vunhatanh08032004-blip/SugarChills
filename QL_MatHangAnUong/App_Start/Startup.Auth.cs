using System;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Owin;

namespace QL_MatHangAnUong
{
    /// <summary>
    /// Cấu hình OWIN OAuth cho Google và Facebook.
    /// Sau khi OAuth callback, TaiKhoanController sẽ đọc claims rồi ghi vào Session như bình thường.
    /// </summary>
    public partial class Startup
    {
        // Tên cookie tạm dùng trong quá trình OAuth redirect (tự động xóa sau khi callback)
        public const string ExternalCookieAuthType = "ExternalCookie";

        public void ConfigureAuth(IAppBuilder app)
        {
            // Cookie tạm — chỉ tồn tại 5 phút trong lúc OAuth đang chạy
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = ExternalCookieAuthType,
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Passive,
                ExpireTimeSpan = TimeSpan.FromMinutes(5)
            });

            // ──────────────────────── GOOGLE ────────────────────────
            var googleId     = System.Web.Configuration.WebConfigurationManager.AppSettings["GoogleClientId"];
            var googleSecret = System.Web.Configuration.WebConfigurationManager.AppSettings["GoogleClientSecret"];

            if (!string.IsNullOrWhiteSpace(googleId) && googleId != "YOUR_GOOGLE_CLIENT_ID")
            {
                app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
                {
                    ClientId     = googleId,
                    ClientSecret = googleSecret,
                    SignInAsAuthenticationType = ExternalCookieAuthType
                });
            }

            // ──────────────────────── FACEBOOK ──────────────────────
            var fbId     = System.Web.Configuration.WebConfigurationManager.AppSettings["FacebookAppId"];
            var fbSecret = System.Web.Configuration.WebConfigurationManager.AppSettings["FacebookAppSecret"];

            if (!string.IsNullOrWhiteSpace(fbId) && fbId != "YOUR_FACEBOOK_APP_ID")
            {
                app.UseFacebookAuthentication(new FacebookAuthenticationOptions
                {
                    AppId     = fbId,
                    AppSecret = fbSecret,
                    SignInAsAuthenticationType = ExternalCookieAuthType,
                    Scope = { "email" }   // bắt buộc để lấy được email
                });
            }
        }
    }
}
