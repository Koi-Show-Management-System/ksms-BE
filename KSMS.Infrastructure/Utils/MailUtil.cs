using System.Net;
using System.Net.Mail;
using KSMS.Domain.Common;

namespace KSMS.Infrastructure.Utils;

public static class MailUtil
{
    public static bool SendEmail(string to, string subject, string body, string attachFile)
    {
        try
        {
            MailMessage msg = new(AppConfig.MailSetting.EmailSender, to, subject, body)
            {
                IsBodyHtml = true
            };

            using var client = new SmtpClient(AppConfig.MailSetting.HostEmail, AppConfig.MailSetting.PortEmail);
            client.EnableSsl = true;
            if (!string.IsNullOrEmpty(attachFile))
            {
                Attachment attachment = new(attachFile);
                msg.Attachments.Add(attachment);
            }
            NetworkCredential credential = new(AppConfig.MailSetting.EmailSender, AppConfig.MailSetting.PasswordSender);
            client.UseDefaultCredentials = false;
            client.Credentials = credential;
            client.Send(msg);
        
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static class ContentMailUtil
    {
      public readonly static string Title_ThankingForRegisAccount =
        "[KOI SHOW SYSTEM] Xác nhận đăng kí tài khoản thành công";

      public static string ThankingForRegistration(string fullname, string activationLink)
      {

        return @"<!doctype html>
<html>
  <head>
    <title></title>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
    <style type=""text/css"">
      /* Giữ nguyên phần CSS như cũ */
      body { font-family: sans-serif; }
      body, table, td, a { -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; }
      table, td { mso-table-lspace: 0pt; mso-table-rspace: 0pt; }
      img { -ms-interpolation-mode: bicubic; border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none; }
      table { border-collapse: collapse !important; }
      body { height: 100% !important; margin: 0 !important; padding: 0 !important; width: 100% !important; }
      /* Các style khác giữ nguyên */
    </style>
  </head>

  <body style=""background-color: #f4f4f4; margin: 0 !important; padding: 0 !important;"">
    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
      <tbody>
        <tr>
          <td bgcolor=""#422A14"" align=""center"">
            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px"">
              <tbody>
                <tr>
                  <td align=""center"" valign=""top"" style=""padding: 30px 10px""></td>
                </tr>
              </tbody>
            </table>
          </td>
        </tr>
        <tr>
          <td bgcolor=""#422A14"" align=""center"" style=""padding: 0px 10px"">
            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 800px; height: auto"">
              <tbody>
                <tr>
                  <td bgcolor=""#ffffff"" align=""center"" valign=""top"" 
                      style=""padding: 30px 50px; border-radius: 4px 4px 0px 0px; color: #000; font-weight: 400; text-align: left;"">
                    <h1 style=""font-weight: bolder; font-size: 24px; color: #422a14; margin: 0px; margin-bottom: 10px; text-align: left;"">
                      KOI SHOW
                    </h1>
                    <h1 style=""font-size: 18px; font-weight: 400; margin: 0px; margin-bottom: 10px; color: #422a14; text-align: left; font-weight: 600;"">
                      Thân gửi " + fullname + @"
                    </h1>
                    <p style=""margin: 0px; text-align: left; line-height: 20px;"">
                      Chúng tôi rất vui mừng thông báo rằng bạn đã đăng ký thành công tài khoản tại Koi Show. 
                    </p>
                    <p style=""margin: 15px 0; text-align: left; line-height: 20px;"">
                      Với tài khoản này, bạn có thể:
                    </p>
                    <ul style=""margin: 0; padding-left: 20px; line-height: 24px;"">
                      <li>Đăng ký tham gia các cuộc thi Koi Show</li>
                      <li>Theo dõi và cập nhật thông tin về các sự kiện</li>
                      <li>Quản lý thông tin cá nhân và cá Koi của bạn</li>
                      <li>Tương tác với cộng đồng người yêu cá Koi</li>
                    </ul>
                    <p style=""margin: 15px 0; text-align: left; line-height: 20px;"">
                      Chúng tôi khuyến khích bạn cập nhật đầy đủ thông tin cá nhân và bắt đầu khám phá các tính năng của hệ thống.
                    </p>
                    <p style=""margin: 15px 0; text-align: left; line-height: 20px;"">
                      <strong>Nhấn vào đây để kích hoạt tài khoản và sử dụng tất cả dịch vụ:</strong><br>
                      <a href=""" + activationLink +
               @""" target=""_blank"" style=""color: #007BFF; text-decoration: none;"">Kích hoạt tài khoản</a>
                    </p>
                    <p style=""margin: 15px 0; text-align: left; line-height: 20px;"">
                      Nếu bạn cần hỗ trợ hoặc có bất kỳ thắc mắc nào, xin đừng ngần ngại liên hệ với chúng tôi qua email hoặc hotline.
                    </p>
                    <p style=""margin: 15px 0 5px 0; text-align: left;"">
                      Trân trọng,
                    </p>
                    <p style=""margin: 0; text-align: left;"">
                      Đội ngũ Koi Show
                    </p>
                  </td>
                </tr>
              </tbody>
            </table>
          </td>
        </tr>
        <tr>
          <td bgcolor=""#422A14"" align=""center"">
            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px"">
              <tbody>
                <tr>
                  <td align=""center"" valign=""top"" style=""padding: 30px 10px""></td>
                </tr>
              </tbody>
            </table>
          </td>
        </tr>
      </tbody>
    </table>
  </body>
</html>";
      }

    }
}