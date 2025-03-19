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
      public readonly static string Title_ApproveForRegisterSh =
          "[KOI SHOW SYSTEM] Xác nhận duyệt đơn đăng kí cá koi thành công";
      public readonly static string Title_ForgotPassword =
          "[KOI SHOW SYSTEM] Mã OTP đặt lại mật khẩu";



        public static string StaffRoleNotification(string staffFullName, string showName, string username, string defaultPassword)
        {
            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - New Role Assigned</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <div style='max-width: 600px; margin: 20px auto; padding: 20px; background-color: #fff; border-radius: 8px;'>
        <h2 style='color: #b21f1f;'>Dear {staffFullName},</h2>
        <p>We are excited to inform you that you have been assigned as the Show Manager for the event:</p>
        <h3 style='color: #b21f1f;'>{showName}</h3>
        <p>Your role will include overseeing the successful execution of this event. We are confident that your contributions will help make this a spectacular show!</p>
        
        <h3 style='color: #b21f1f;'>Account Details</h3>
        <p>Your login credentials are as follows:</p>
        <ul>
            <li><strong>Username:</strong> {username}</li>
            <li><strong>Password:</strong> {defaultPassword}</li>
        </ul>

        <p>Please <a href='https://www.facebook.com/khoa.phung.12177/' style='font-weight: bold; color: #ffffff; background-color: #b21f1f; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>click here to change your password</a> after your first login.</p>

        <p>If you didn't request this, please ignore this email.</p>
        <p>Best Regards,<br>KOI SHOW TEAM</p>
    </div>
</body>
</html>";
        }



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
   public static string ConfirmingRegistration(Registration registration)
{
    var images = registration.KoiMedia
        .Where(m => m.MediaType.ToLower() == "image")
        .Select(m => m.MediaUrl)
        .ToList();

    var videos = registration.KoiMedia
        .Where(m => m.MediaType.ToLower() == "video")
        .Select(m => m.MediaUrl)
        .ToList();

    return $@"
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
                            <p>Thân gửi <span style='font-weight: bold; color: #b21f1f;'>{registration.Account.FullName}</span>,</p>
                            <p>Chúng tôi xác nhận bạn đã đăng ký và thanh toán thành công cho sự kiện <span style='font-weight: bold; color: #b21f1f;'>{registration.KoiShow.Name}</span>.</p>
                            
                            <p>Thông tin cá Koi đã đăng ký:</p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Tên người đăng ký:</strong> {registration.RegisterName}</li>
                                <li><strong>Giống loại:</strong> {registration.KoiProfile.Variety.Name}</li>
                                <li><strong>Tên cá:</strong> {registration.KoiProfile.Name}</li>
                                <li><strong>Kích thước:</strong> {registration.KoiSize} cm</li>
                                <li><strong>Giới tính:</strong> {registration.KoiProfile.Gender}</li>
                                <li><strong>Tuổi:</strong> {registration.KoiAge} năm</li>
                                <li><strong>Dòng máu:</strong> {(string.IsNullOrEmpty(registration.KoiProfile.Bloodline) ? "Không xác định" : registration.KoiProfile.Bloodline)}</li>
                            </ul>

                            <p><strong>Hạng mục thi đấu được chỉ định:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Tên hạng mục:</strong> {registration.CompetitionCategory.Name}</li>
                                <li><strong>Mô tả:</strong> {registration.CompetitionCategory.Description}</li>
                                <li><strong>Phạm vi kích thước:</strong> {registration.CompetitionCategory.SizeMin} - {registration.CompetitionCategory.SizeMax} cm</li>
                            </ul>

                            <p><strong>Danh sách hình ảnh:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                {string.Join("\n", images.Select((img, index) => 
                                    $@"<li>Ảnh {index + 1}: <a href='{img}' target='_blank'>Xem ảnh</a></li>"))}
                            </ul>

                            <p><strong>Danh sách video:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                {string.Join("\n", videos.Select((video, index) => 
                                    $@"<li>Video {index + 1}: <a href='{video}' target='_blank'>Xem video</a></li>"))}
                            </ul>

                            <p><strong>Ghi chú:</strong> {(string.IsNullOrEmpty(registration.Notes) ? "Không có" : registration.Notes)}</p>

                            <p style='font-weight: bold; color: #b21f1f;'>Thông tin quan trọng:</p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li style='margin-bottom: 10px;'>Đơn đăng ký của bạn đang chờ staff xét duyệt.</li>
                                <li style='margin-bottom: 10px;'>Sau khi được duyệt, bạn sẽ nhận được email kèm mã QR để check-in tại sự kiện.</li>
                                <li style='margin-bottom: 10px;'>Nếu đơn bị từ chối, chúng tôi sẽ liên hệ để hoàn trả phí đăng ký.</li>
                                <li>Vui lòng kiểm tra email thường xuyên để nhận thông báo mới nhất.</li>
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
public static string ConfirmCategoryAssignment(Registration registration)
{
    var images = registration.KoiMedia
        .Where(m => m.MediaType.ToLower() == "image")
        .Select(m => m.MediaUrl)
        .ToList();

    var videos = registration.KoiMedia
        .Where(m => m.MediaType.ToLower() == "video")
        .Select(m => m.MediaUrl)
        .ToList();

    return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Duyệt đơn đăng ký thành công</title>
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
                            <p>Thân gửi <span style='font-weight: bold; color: #b21f1f;'>{registration.Account.FullName}</span>,</p>
                            
                            <p>Chúng tôi vui mừng thông báo rằng đơn đăng ký tham gia sự kiện <span style='font-weight: bold; color: #b21f1f;'>{registration.KoiShow.Name}</span> của bạn đã được <span style='font-weight: bold; color: #1a2a6c;'>duyệt thành công</span>.</p>

                            <p>Thông tin cá Koi đã đăng ký:</p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Tên người đăng ký:</strong> {registration.RegisterName}</li>
                                <li><strong>Giống loại:</strong> {registration.KoiProfile.Variety.Name}</li>
                                <li><strong>Tên cá:</strong> {registration.KoiProfile.Name}</li>
                                <li><strong>Kích thước:</strong> {registration.KoiSize} cm</li>
                                <li><strong>Giới tính:</strong> {registration.KoiProfile.Gender}</li>
                                <li><strong>Tuổi:</strong> {registration.KoiAge} năm</li>
                                <li><strong>Dòng máu:</strong> {(string.IsNullOrEmpty(registration.KoiProfile.Bloodline) ? "Không xác định" : registration.KoiProfile.Bloodline)}</li>
                            </ul>

                            <p><strong>Hạng mục thi đấu:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Tên hạng mục:</strong> {registration.CompetitionCategory.Name}</li>
                                <li><strong>Mô tả:</strong> {registration.CompetitionCategory.Description}</li>
                                <li><strong>Phạm vi kích thước:</strong> {registration.CompetitionCategory.SizeMin} - {registration.CompetitionCategory.SizeMax} cm</li>
                            </ul>

                            <p><strong>Thông tin check-in:</strong></p>
                            <div style='text-align: center; margin: 20px 0;'>
                                <img src='{registration.RegistrationPayment.QrcodeData}' alt='QR Code' style='width: 200px; height: 200px; border: 1px solid #ddd;'>
                            </div>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Mã giao dịch:</strong> {registration.RegistrationPayment.TransactionCode}</li>
                                <li><strong>Hiệu lực:</strong> Mã QR có hiệu lực trong vòng 30 phút kể từ khi check-in</li>
                                <li><strong>Lưu ý:</strong> Vui lòng đến đúng giờ để đảm bảo quyền lợi tham gia cuộc thi</li>
                            </ul>

                            <p style='font-weight: bold; color: #b21f1f;'>Thông tin quan trọng:</p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li style='margin-bottom: 10px;'>Vui lòng xuất trình mã QR khi check-in tại sự kiện.</li>
                                <li style='margin-bottom: 10px;'>Đảm bảo đến đúng giờ để hoàn tất thủ tục check-in.</li>
                                <li>Theo dõi email để cập nhật thông tin mới nhất về sự kiện.</li>
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
public static string ForgotPasswordOTP(string fullName, string otpCode)
{
    return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Mã OTP đặt lại mật khẩu</title>
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
                            <p>Thân gửi <span style='font-weight: bold; color: #b21f1f;'>{fullName}</span>,</p>
                            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                            <p>Mã OTP của bạn là:</p>
                            <div style='text-align: center; margin: 20px 0;'>
                                <div style='font-size: 32px; font-weight: bold; color: #b21f1f; letter-spacing: 5px; padding: 10px; background-color: #f8f8f8; display: inline-block; border-radius: 5px;'>
                                    {otpCode}
                                </div>
                            </div>
                            <p>Mã OTP này sẽ hết hạn sau 5 phút.</p>
                            <p style='color: #b21f1f; font-weight: bold;'>Lưu ý: Không chia sẻ mã này với bất kỳ ai.</p>
                            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này hoặc liên hệ với chúng tôi ngay.</p>
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
public static string RejectRegistration(Registration registration)
{
    var images = registration.KoiMedia
        .Where(m => m.MediaType.ToLower() == "image")
        .Select(m => m.MediaUrl)
        .ToList();

    var videos = registration.KoiMedia
        .Where(m => m.MediaType.ToLower() == "video")
        .Select(m => m.MediaUrl)
        .ToList();

    return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Thông báo từ chối đơn đăng ký</title>
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
                            <p>Thân gửi <span style='font-weight: bold; color: #b21f1f;'>{registration.Account.FullName}</span>,</p>
                            
                            <p>Chúng tôi rất tiếc phải thông báo rằng đơn đăng ký tham gia sự kiện <span style='font-weight: bold; color: #b21f1f;'>{registration.KoiShow.Name}</span> của bạn đã bị <span style='font-weight: bold; color: #b21f1f;'>từ chối</span>.</p>

                            <p><strong>Thông tin cá Koi đã đăng ký:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Tên người đăng ký:</strong> {registration.RegisterName}</li>
                                <li><strong>Giống loại:</strong> {registration.KoiProfile.Variety.Name}</li>
                                <li><strong>Tên cá:</strong> {registration.KoiProfile.Name}</li>
                                <li><strong>Kích thước:</strong> {registration.KoiSize} cm</li>
                                <li><strong>Giới tính:</strong> {registration.KoiProfile.Gender}</li>
                                <li><strong>Tuổi:</strong> {registration.KoiAge} năm</li>
                                <li><strong>Dòng máu:</strong> {(string.IsNullOrEmpty(registration.KoiProfile.Bloodline) ? "Không xác định" : registration.KoiProfile.Bloodline)}</li>
                            </ul>

                            <p><strong>Hạng mục thi đấu đã đăng ký:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Tên hạng mục:</strong> {registration.CompetitionCategory.Name}</li>
                                <li><strong>Mô tả:</strong> {registration.CompetitionCategory.Description}</li>
                                <li><strong>Phạm vi kích thước:</strong> {registration.CompetitionCategory.SizeMin} - {registration.CompetitionCategory.SizeMax} cm</li>
                            </ul>

                            <p><strong>Danh sách hình ảnh:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                {string.Join("\n", images.Select((img, index) => 
                                    $@"<li>Ảnh {index + 1}: <a href='{img}' target='_blank'>Xem ảnh</a></li>"))}
                            </ul>

                            <p><strong>Danh sách video:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                {string.Join("\n", videos.Select((video, index) => 
                                    $@"<li>Video {index + 1}: <a href='{video}' target='_blank'>Xem video</a></li>"))}
                            </ul>

                            <p style='font-weight: bold; color: #b21f1f;'>Về việc hoàn phí đăng ký:</p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li style='margin-bottom: 10px;'>Đội ngũ của chúng tôi sẽ liên hệ với bạn qua email {registration.Account.Email} để xác nhận thông tin và thực hiện quy trình hoàn phí.</li>
                                <li style='margin-bottom: 10px;'>Vui lòng chuẩn bị sẵn thông tin tài khoản ngân hàng để chúng tôi có thể hoàn trả phí đăng ký ({registration.RegistrationFee:N0} VNĐ).</li>
                                <li>Nếu sau 24h làm việc bạn chưa nhận được liên hệ, vui lòng thông báo cho chúng tôi qua các kênh hỗ trợ bên dưới.</li>
                            </ul>

                            <p>Chúng tôi chân thành xin lỗi vì sự bất tiện này và hy vọng được đón tiếp bạn trong những sự kiện tiếp theo.</p>

                            <p>Nếu bạn cần hỗ trợ thêm hoặc có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi qua:</p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Hotline:</strong> 1900 xxxx</li>
                                <li><strong>Email:</strong> support@koishow.com</li>
                            </ul>
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

public static string ConfirmTicketOrder(TicketOrder order)
{
    var koiShow = order.TicketOrderDetails.First().TicketType.KoiShow;

    return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Xác nhận đơn hàng vé thành công</title>
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
                            <p>Thân gửi <span style='font-weight: bold; color: #b21f1f;'>{order.FullName}</span>,</p>
                            
                            <p>Cảm ơn bạn đã đặt vé tham dự sự kiện <span style='font-weight: bold; color: #b21f1f;'>{koiShow.Name}</span>. Đơn hàng của bạn đã được xác nhận thành công!</p>

                            <p><strong>Thông tin sự kiện:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Tên sự kiện:</strong> {koiShow.Name}</li>
                                <li><strong>Thời gian:</strong> {koiShow.StartDate:dd/MM/yyyy HH:mm} - {koiShow.EndDate:dd/MM/yyyy HH:mm}</li>
                                <li><strong>Địa điểm:</strong> {koiShow.Location}</li>
                            </ul>

                            <p><strong>Thông tin đơn hàng:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Mã đơn hàng:</strong> {order.TransactionCode}</li>
                                <li><strong>Ngày đặt:</strong> {order.OrderDate:dd/MM/yyyy HH:mm}</li>
                                <li><strong>Tổng tiền:</strong> {order.TotalAmount:N0} VNĐ</li>
                            </ul>

                            <p><strong>Chi tiết vé:</strong></p>
                            <table style='width: 100%; border-collapse: collapse; margin: 10px 0;'>
                                <tr style='background-color: #f8f8f8;'>
                                    <th style='padding: 10px; border: 1px solid #ddd; text-align: left;'>Loại vé</th>
                                    <th style='padding: 10px; border: 1px solid #ddd; text-align: center;'>Số lượng</th>
                                    <th style='padding: 10px; border: 1px solid #ddd; text-align: right;'>Đơn giá</th>
                                    <th style='padding: 10px; border: 1px solid #ddd; text-align: right;'>Thành tiền</th>
                                </tr>
                                {string.Join("\n", order.TicketOrderDetails.Select(detail => $@"
                                <tr>
                                    <td style='padding: 10px; border: 1px solid #ddd;'>{detail.TicketType.Name}</td>
                                    <td style='padding: 10px; border: 1px solid #ddd; text-align: center;'>{detail.Quantity}</td>
                                    <td style='padding: 10px; border: 1px solid #ddd; text-align: right;'>{detail.UnitPrice:N0} VNĐ</td>
                                    <td style='padding: 10px; border: 1px solid #ddd; text-align: right;'>{detail.Quantity * detail.UnitPrice:N0} VNĐ</td>
                                </tr>"))}
                            </table>

                            <p><strong>Vé điện tử của bạn:</strong></p>
                            <div style='margin: 20px 0;'>
                                {string.Join("\n", order.TicketOrderDetails.SelectMany(detail => detail.Tickets.Select(ticket => $@"
                                <div style='background-color: #ffffff; border: 2px solid #e0e0e0; border-radius: 10px; margin-bottom: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                                    <div style='background-color: #1a2a6c; color: white; padding: 15px; border-top-left-radius: 8px; border-top-right-radius: 8px;'>
                                        <h3 style='margin: 0; font-size: 18px;'>{detail.TicketType.Name}</h3>
                                    </div>
                                    <div style='padding: 20px; display: flex; justify-content: space-between; align-items: center;'>
                                        <div style='flex: 1;'>
                                            <p style='margin: 5px 0; color: #666;'><strong>Sự kiện:</strong> {koiShow.Name}</p>
                                            <p style='margin: 5px 0; color: #666;'><strong>Thời gian:</strong> {koiShow.StartDate:dd/MM/yyyy HH:mm}</p>
                                            <p style='margin: 5px 0; color: #666;'><strong>Địa điểm:</strong> {koiShow.Location}</p>
                                        </div>
                                        <div style='margin-left: 20px; text-align: center;'>
                                            <img src='{ticket.QrcodeData}' alt='QR Code' style='width: 120px; height: 120px; border: 1px solid #ddd; padding: 5px; background-color: white;'>
                                            <p style='margin: 5px 0; font-size: 12px; color: #666;'>Quét mã để check-in</p>
                                        </div>
                                    </div>
                                    <div style='background-color: #f8f8f8; padding: 10px; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px; text-align: center; border-top: 1px dashed #ddd;'>
                                        <p style='margin: 0; font-size: 12px; color: #666;'>Vui lòng xuất trình mã QR này khi check-in tại sự kiện</p>
                                    </div>
                                </div>")))}
                            </div>

                            <p style='font-weight: bold; color: #b21f1f;'>Lưu ý quan trọng:</p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li>Vui lòng xuất trình mã QR khi check-in tại sự kiện</li>
                                <li>Mỗi mã QR chỉ có thể sử dụng một lần</li>
                                <li>Không chia sẻ mã QR với người khác</li>
                            </ul>

                            <p>Nếu bạn cần hỗ trợ thêm hoặc có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi qua:</p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Hotline:</strong> 1900 xxxx</li>
                                <li><strong>Email:</strong> support@koishow.com</li>
                            </ul>
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
public static string RefundRegistrationPayment(Registration registration)
{
    return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Thông báo hoàn phí đăng ký</title>
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
                            <p>Thân gửi <span style='font-weight: bold; color: #b21f1f;'>{registration.Account.FullName}</span>,</p>
                            
                            <p>Chúng tôi xin thông báo rằng phí đăng ký tham gia sự kiện <span style='font-weight: bold; color: #b21f1f;'>{registration.KoiShow.Name}</span> của bạn đã được hoàn trả.</p>

                            <p><strong>Thông tin hoàn phí:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Số tiền hoàn trả:</strong> {registration.RegistrationFee:N0} VNĐ</li>
                                <li><strong>Ngày hoàn tiền:</strong> {VietNamTimeUtil.GetVietnamTime():dd/MM/yyyy}</li>
                            </ul>

                            <p><strong>Thông tin đăng ký:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Tên cá:</strong> {registration.KoiProfile.Name}</li>
                                <li><strong>Giống:</strong> {registration.KoiProfile.Variety.Name}</li>
                                <li><strong>Hạng mục:</strong> {registration.CompetitionCategory.Name}</li>
                            </ul>

                            <p style='font-weight: bold; color: #b21f1f;'>Lưu ý:</p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li>Vui lòng kiểm tra tài khoản ngân hàng của bạn</li>
                                <li>Nếu có bất kỳ thắc mắc nào về việc hoàn phí, vui lòng liên hệ với chúng tôi</li>
                            </ul>

                            <p>Chúng tôi chân thành xin lỗi vì sự bất tiện này và hy vọng được đón tiếp bạn trong những sự kiện tiếp theo của Koi Show.</p>

                            <p>Nếu bạn cần hỗ trợ thêm, vui lòng liên hệ với chúng tôi qua:</p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li><strong>Hotline:</strong> 1900 xxxx</li>
                                <li><strong>Email:</strong> support@koishow.com</li>
                            </ul>
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