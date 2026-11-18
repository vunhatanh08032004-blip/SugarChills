using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace QL_MatHangAnUong.Helpers
{
    /// <summary>
    /// Gửi email qua SMTP (mặc định Gmail). Cấu hình trong Secrets.config:
    /// SmtpUser (địa chỉ Gmail), SmtpPass (App Password 16 ký tự); tùy chọn SmtpHost, SmtpPort.
    /// </summary>
    public static class GuiMail
    {
        private static string Lay(string khoa)
        {
            return (ConfigurationManager.AppSettings[khoa] ?? "").Trim();
        }

        public static bool DaCauHinh()
        {
            return Lay("SmtpUser") != "" && Lay("SmtpPass") != "";
        }

        /// <summary>Gửi email HTML. Ném exception nếu lỗi — nơi gọi tự bắt.</summary>
        public static void Gui(string den, string tieuDe, string noiDungHtml)
        {
            string user = Lay("SmtpUser");
            string pass = Lay("SmtpPass").Replace(" ", "");   // App Password hay được Google hiển thị kèm dấu cách
            string host = Lay("SmtpHost"); if (host == "") host = "smtp.gmail.com";
            int port; if (!int.TryParse(Lay("SmtpPort"), out port)) port = 587;

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            using (var msg = new MailMessage())
            {
                msg.From = new MailAddress(user, "SugarChills");
                msg.To.Add(den);
                msg.Subject = tieuDe;
                msg.SubjectEncoding = Encoding.UTF8;
                msg.BodyEncoding = Encoding.UTF8;
                msg.IsBodyHtml = true;
                msg.Body = noiDungHtml;

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(user, pass);
                    client.Timeout = 15000;
                    client.Send(msg);
                }
            }
        }
    }
}
