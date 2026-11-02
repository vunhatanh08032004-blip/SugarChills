using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QL_MatHangAnUong.Helpers
{
    /// <summary>Thông tin tối thiểu lấy về từ Google / Facebook sau khi người dùng đồng ý.</summary>
    public class ThongTinMangXaHoi
    {
        public string NhaCungCap { get; set; }   // "Google" | "Facebook"
        public string Id { get; set; }
        public string Email { get; set; }
        public bool EmailDaXacMinh { get; set; }
        public string HoTen { get; set; }
    }

    /// <summary>
    /// Đăng nhập bằng Google / Facebook theo chuẩn OAuth 2.0 (Authorization Code Flow),
    /// gọi trực tiếp bằng HttpClient — KHÔNG cần cài thêm gói NuGet (OWIN/Identity).
    ///
    /// Luồng: 1) Chuyển người dùng sang trang đồng ý của Google/Facebook (TaoUrl...)
    ///        2) Google/Facebook gọi lại /TaiKhoan/GoogleCallback?code=...&state=...
    ///        3) Server đổi "code" lấy access_token rồi gọi API lấy email + họ tên (LayThongTin...)
    ///
    /// Client ID / Secret đặt trong Web.config > appSettings.
    /// </summary>
    public static class DangNhapMangXaHoi
    {
        private static readonly HttpClient http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

        static DangNhapMangXaHoi()
        {
            // Google / Facebook chỉ chấp nhận TLS 1.2 trở lên
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        private static string CauHinh(string khoa)
        {
            return (ConfigurationManager.AppSettings[khoa] ?? "").Trim();
        }

        public static bool DaCauHinh(string nhaCungCap)
        {
            return nhaCungCap == "google"
                ? CauHinh("GoogleClientId") != "" && CauHinh("GoogleClientSecret") != ""
                : CauHinh("FacebookAppId") != "" && CauHinh("FacebookAppSecret") != "";
        }

        // ================= GOOGLE =================
        public static string TaoUrlGoogle(string redirectUri, string state)
        {
            return "https://accounts.google.com/o/oauth2/v2/auth"
                + "?client_id=" + Uri.EscapeDataString(CauHinh("GoogleClientId"))
                + "&redirect_uri=" + Uri.EscapeDataString(redirectUri)
                + "&response_type=code"
                + "&scope=" + Uri.EscapeDataString("openid email profile")
                + "&state=" + Uri.EscapeDataString(state)
                + "&prompt=select_account";
        }

        public static async Task<ThongTinMangXaHoi> LayThongTinGoogle(string code, string redirectUri)
        {
            // B1: đổi code -> access_token
            var phanHoiToken = await http.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", CauHinh("GoogleClientId") },
                    { "client_secret", CauHinh("GoogleClientSecret") },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                }));
            var token = JObject.Parse(await phanHoiToken.Content.ReadAsStringAsync());
            string accessToken = (string)token["access_token"];
            if (string.IsNullOrEmpty(accessToken))
                throw new InvalidOperationException("Google: " + ((string)token["error_description"] ?? (string)token["error"]));

            // B2: lấy thông tin người dùng
            var yeuCau = new HttpRequestMessage(HttpMethod.Get, "https://openidconnect.googleapis.com/v1/userinfo");
            yeuCau.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var nguoiDung = JObject.Parse(await (await http.SendAsync(yeuCau)).Content.ReadAsStringAsync());

            return new ThongTinMangXaHoi
            {
                NhaCungCap = "Google",
                Id = (string)nguoiDung["sub"],
                Email = (string)nguoiDung["email"],
                EmailDaXacMinh = (bool?)nguoiDung["email_verified"] ?? false,
                HoTen = (string)nguoiDung["name"]
            };
        }

        // ================= FACEBOOK =================
        public static string TaoUrlFacebook(string redirectUri, string state)
        {
            return "https://www.facebook.com/dialog/oauth"
                + "?client_id=" + Uri.EscapeDataString(CauHinh("FacebookAppId"))
                + "&redirect_uri=" + Uri.EscapeDataString(redirectUri)
                + "&response_type=code"
                + "&scope=" + Uri.EscapeDataString("email,public_profile")
                + "&state=" + Uri.EscapeDataString(state);
        }

        public static async Task<ThongTinMangXaHoi> LayThongTinFacebook(string code, string redirectUri)
        {
            // B1: đổi code -> access_token
            string urlToken = "https://graph.facebook.com/oauth/access_token"
                + "?client_id=" + Uri.EscapeDataString(CauHinh("FacebookAppId"))
                + "&client_secret=" + Uri.EscapeDataString(CauHinh("FacebookAppSecret"))
                + "&redirect_uri=" + Uri.EscapeDataString(redirectUri)
                + "&code=" + Uri.EscapeDataString(code);
            var token = JObject.Parse(await http.GetStringAsyncSafe(urlToken));
            string accessToken = (string)token["access_token"];
            if (string.IsNullOrEmpty(accessToken))
                throw new InvalidOperationException("Facebook: " + (string)token.SelectToken("error.message"));

            // B2: lấy id, tên, email
            var nguoiDung = JObject.Parse(await http.GetStringAsyncSafe(
                "https://graph.facebook.com/me?fields=id,name,email&access_token=" + Uri.EscapeDataString(accessToken)));

            string email = (string)nguoiDung["email"];
            return new ThongTinMangXaHoi
            {
                NhaCungCap = "Facebook",
                Id = (string)nguoiDung["id"],
                Email = email,
                EmailDaXacMinh = !string.IsNullOrEmpty(email),   // Facebook chỉ trả email đã xác minh
                HoTen = (string)nguoiDung["name"]
            };
        }

        /// <summary>GET và đọc nội dung kể cả khi HTTP lỗi (để lấy được thông báo lỗi JSON).</summary>
        private static async Task<string> GetStringAsyncSafe(this HttpClient client, string url)
        {
            var phanHoi = await client.GetAsync(url);
            return await phanHoi.Content.ReadAsStringAsync();
        }
    }
}
