using System.Net;
using System.Net.Mail;
using KSMS.Domain.Common;
using KSMS.Domain.Entities;

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
      public readonly static string Title_ThankingForRegisterSh =
          "[KOI SHOW SYSTEM] Xác nhận đăng kí tham gia giải cá koi thành công";

      public static string ThankingForRegistration(string fullname, string confirmationLink)
      {
          return @"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Thân gửi " + fullname + @"</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color: #f4f4f4;'>
        <tr>
            <td align='center' style='padding: 20px 0;'>
                <table border='0' cellpadding='0' cellspacing='0' width='600' style='background-color: #ffffff; border-radius: 10px; padding: 20px;'>
                    <tr>
                        <td align='center' style='font-family: Arial, sans-serif; color: #1a2a6c;'>
                            <h1 style='font-size: 36px; margin: 20px 0;'>KOI SHOW</h1>
                            <hr style='border: none; border-top: 3px solid #b21f1f; width: 60px; margin: 10px auto;'>
                        </td>
                    </tr>
                    <tr>
                        <td style='font-family: Arial, sans-serif; font-size: 16px; line-height: 1.8; padding: 20px; color: #333;'>
                            <p>Thân gửi <span style='font-weight: bold; color: #b21f1f;'>" + fullname + @"</span>,</p>
                            <p>Chúng tôi vô cùng hân hạnh thông báo rằng bạn đã trở thành thành viên chính thức của <span style='font-weight: bold; color: #b21f1f;'>Koi Show</span> - nơi hội tụ những tâm hồn đam mê cá Koi.</p>
                            <a href='" + confirmationLink +
                 @"' class='confirmation-link' style='font-size: 16px; font-weight: bold; color: #ffffff; background-color: #b21f1f; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Xác nhận tài khoản của bạn tại đây</a>
                            <p>Với tư cách là thành viên, bạn sẽ được trải nghiệm:</p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li style='margin-bottom: 10px;'>Tham gia độc quyền vào các cuộc thi Koi Show đẳng cấp</li>
                                <li style='margin-bottom: 10px;'>Cập nhật thông tin mới nhất về sự kiện và xu hướng trong giới Koi</li>
                                <li style='margin-bottom: 10px;'>Quyền trưng bày bộ sưu tập cá Koi quý giá của bạn</li>
                                <li>Kết nối với cộng đồng đam mê cá Koi trên toàn quốc</li>
                            </ul>
                            <p>Chúng tôi khuyến khích bạn khám phá và tận hưởng mọi tính năng độc đáo mà hệ thống cung cấp. Hãy để Koi Show trở thành ngôi nhà thứ hai của bạn trong thế giới cá Koi tuyệt vời.</p>
                            <p>Nếu bạn cần hỗ trợ hoặc có bất kỳ thắc mắc nào, đội ngũ chuyên gia của chúng tôi luôn sẵn sàng phục vụ bạn qua email hoặc đường dây nóng.</p>
                        </td>
                    </tr>
                    <tr>
                        <td align='center' style='padding: 20px;'>
                            <p style='font-size: 14px; color: #666; border-top: 1px solid #ddd; padding-top: 10px;'>Trân trọng,</p>
                            <p style='font-family: Arial, sans-serif; font-size: 18px; font-weight: bold; color: #1a2a6c;'>Đội ngũ Koi Show</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
      }
    public static string ConfirmingRegistration(RegistrationPayment registrationPayment)
{
    return @"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Xác nhận đăng ký thành công</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color: #f4f4f4;'>
        <tr>
            <td align='center' style='padding: 20px 0;'>
                <table border='0' cellpadding='0' cellspacing='0' width='600' style='background-color: #ffffff; border-radius: 10px; padding: 20px;'>
                    <tr>
                        <td align='center' style='font-family: Arial, sans-serif; color: #1a2a6c;'>
                            <h1 style='font-size: 36px; margin: 20px 0;'>KOI SHOW</h1>
                            <hr style='border: none; border-top: 3px solid #b21f1f; width: 60px; margin: 10px auto;'>
                        </td>
                    </tr>
                    <tr>
                        <td style='font-family: Arial, sans-serif; font-size: 16px; line-height: 1.8; padding: 20px; color: #333;'>
                            <p>Thân gửi <span style='font-weight: bold; color: #b21f1f;'>" + registrationPayment.Registration.Account.FullName + @"</span>,</p>
                            <p>Chúng tôi rất vui thông báo rằng bạn đã đăng ký thành công tham gia sự kiện <span style='font-weight: bold; color: #b21f1f;'>" + registrationPayment.Registration.Category.Show.Name + @"</span> và hạng mục <span style='font-weight: bold; color: #b21f1f;'>" + registrationPayment.Registration.Category.Name + @"</span>.</p>
                            <p>Thông tin cá Koi đã đăng ký:</p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Giống loại:</strong> " + registrationPayment.Registration.Variety.Name + @"</li>
                                <li><strong>Tên:</strong> " + registrationPayment.Registration.Name + @"</li>
                                <li><strong>Kích thước:</strong> " + registrationPayment.Registration.Size + @" cm</li>
                                <li><strong>Tuổi:</strong> " + registrationPayment.Registration.Age + @" năm</li>
                                <li><strong>Giới tính:</strong> " + registrationPayment.Registration.Gender + @"</li>
                                <li><strong>Dòng máu:</strong> " + (string.IsNullOrEmpty(registrationPayment.Registration.Bloodline) ? "Không xác định" : registrationPayment.Registration.Bloodline) + @"</li>
                                <li><strong>Ảnh:</strong> <a href='" + registrationPayment.Registration.ImgUrl + @"' target='_blank'>Xem ảnh</a></li>
                                <li><strong>Video:</strong> <a href='" + registrationPayment.Registration.VideoUrl + @"' target='_blank'>Xem video</a></li>
                                <li><strong>Ghi chú:</strong> " + (string.IsNullOrEmpty(registrationPayment.Registration.Notes) ? "Không có" : registrationPayment.Registration.Notes) + @"</li>
                            </ul>
                            <p>Đơn đăng ký của bạn đang được đội ngũ chúng tôi xử lý. Vui lòng chờ staff của chúng tôi duyệt đơn đăng ký. Sau khi duyệt, chúng tôi sẽ gửi mã QR để bạn có thể sử dụng khi check-in tham gia sự kiện.</p>
                            <p>Chúng tôi mong rằng bạn sẽ có một trải nghiệm tuyệt vời tại sự kiện.</p>

                            <p style='font-weight: bold; color: #b21f1f;'>Thông tin quan trọng:</p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li style='margin-bottom: 10px;'>Đảm bảo thông tin đăng ký của bạn là chính xác để nhận được mã QR.</li>
                                <li style='margin-bottom: 10px;'>Mã QR sẽ được gửi qua email sau khi đơn được duyệt.</li>
                                <li>Quý khách vui lòng kiểm tra email thường xuyên để nhận thông báo mới nhất.</li>
                            </ul>
                            <p>Nếu bạn cần hỗ trợ hoặc có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi qua email hoặc hotline.</p>
                        </td>
                    </tr>
                    <tr>
                        <td align='center' style='padding: 20px;'>
                            <p style='font-size: 14px; color: #666; border-top: 1px solid #ddd; padding-top: 10px;'>Trân trọng,</p>
                            <p style='font-family: Arial, sans-serif; font-size: 18px; font-weight: bold; color: #1a2a6c;'>Đội ngũ Koi Show</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
}


    }
}