using System.Net;
using System.Net.Mail;
using KSMS.Domain.Common;
using KSMS.Domain.Dtos.Responses.RefereeAssignment;
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

        public readonly static string Title_ShowInternalReviewManager =
            "[KOI SHOW SYSTEM] Thông báo quản lí triển lãm";

        public readonly static string Title_ShowInternalReviewStaff =
            "[KOI SHOW SYSTEM] Thông báo phân công nhân viên triển lãm";

        public readonly static string Title_RefereeAssignment =
            "[KOI SHOW SYSTEM] Thông báo phân công trọng tài";

        public readonly static string Title_NewStaffAccount =
            "[KOI SHOW SYSTEM] Thông báo tạo tài khoản nhân viên/quản lý";

        public readonly static string Title_NewRefereeAccount =
            "[KOI SHOW SYSTEM] Thông báo tạo tài khoản trọng tài";

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
                                <li><strong>Số báo danh:</strong> <span style='font-weight: bold; color: #b21f1f; font-size: 18px;'>{registration.RegistrationNumber}</span></li>
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
                                <li><strong>Hiệu lực:</strong> Mã QR chỉ có hiệu lực trong thời gian check-in cá Koi theo lịch trình sự kiện. Sau thời gian này, mã QR sẽ hết hiệu lực và không thể check-in</li>
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

        public static string RejectRegistration(Registration registration, string rejectReason)
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
                            <p><strong>Lý do từ chối:</strong></p>
                            <div style='background-color: #f9f9f9; border-left: 4px solid #b21f1f; padding: 15px; margin: 15px 0;'>
                                <p style='margin: 0; color: #333;'>{(string.IsNullOrEmpty(rejectReason) ? "Không có lý do cụ thể được cung cấp." : rejectReason)}</p>
                            </div>
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
                                            <p style='margin: 5px 0; color: #666;'><strong>Thời gian bắt đầu sự kiện:</strong> {koiShow.StartDate:dd/MM/yyyy HH:mm}</p>
                                            <p style='margin: 5px 0; color: #666;'><strong>Địa điểm:</strong> {koiShow.Location}</p>
                                            <p style='margin: 5px 0; color: #666;'><strong>Lưu ý:</strong> Quý khách vui lòng tham khảo lịch trình chính thức của sự kiện để nắm rõ khung giờ check-in khuyến nghị</p>
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
                                <li>Khung giờ check-in khuyến nghị nằm trong giai đoạn TicketCheckIn theo lịch trình chính thức của sự kiện. Trong thời gian này, quý khách sẽ được phục vụ ưu tiên và có trải nghiệm tốt nhất. Sau thời gian này, quý khách vẫn có thể check-in bình thường để tham dự triển lãm</li>
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

        public static string ShowInternalReviewNotificationForManager(string managerName, KoiShow show)
        {
            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Thông báo quản lý triển lãm</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color: #f4f4f4;'>
        <tr>
            <td align='center' style='padding: 20px 0;'>
                <table border='0' cellpadding='0' cellspacing='0' width='600' style='background-color: #ffffff; border-radius: 10px; padding: 20px;'>
                    <tr>
                        <td align='center' style='font-family: Arial, sans-serif; color: #1a2a6c;'>
                            <h1 style='font-size: 24px; margin: 20px 0; color: #b21f1f;'>KOI SHOW MANAGEMENT SYSTEM</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style='font-family: Arial, sans-serif; font-size: 16px; line-height: 1.8; padding: 20px; color: #333;'>
                            <h2 style='color: #b21f1f; font-size: 20px;'>Kính gửi <span style='font-weight: bold;'>{managerName}</span>,</h2>
                            <p>Chúng tôi xin thông báo rằng triển lãm sau đây đã được chuyển sang trạng thái <strong>Nội bộ (InternalReview)</strong> và bạn đã được chỉ định làm <strong>Quản lý</strong> cho triển lãm này:</p>
                            
                            <div style='background-color: #f9f9f9; padding: 15px; border-left: 4px solid #b21f1f; margin: 15px 0;'>
                                <h3 style='color: #b21f1f; margin-top: 0; font-size: 18px;'>{show.Name}</h3>
                                <p><strong>Thời gian:</strong> {show.StartDate:dd/MM/yyyy} - {show.EndDate:dd/MM/yyyy}</p>
                                <p><strong>Địa điểm:</strong> {show.Location}</p>
                            </div>
                            
                            <p><strong>Trách nhiệm của bạn với vai trò Quản lý bao gồm:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li>Quản lý tổng thể việc tổ chức triển lãm</li>
                                <li>Phân công nhiệm vụ cho các nhân viên</li>
                                <li>Giám sát tiến độ và chất lượng công việc</li>
                                <li>Phê duyệt các quyết định quan trọng</li>
                                <li>Báo cáo tình hình cho ban tổ chức</li>
                            </ul>
                            
                            <p>Hiện triển lãm đang ở trạng thái nội bộ, chỉ quản lý và nhân viên được phân công mới có thể xem. Vui lòng truy cập hệ thống để cập nhật thông tin và bắt đầu phân công nhiệm vụ cho nhân viên.</p>
                            
                            
                            <p>Nếu bạn có bất kỳ câu hỏi hoặc cần hỗ trợ, vui lòng liên hệ với quản trị viên hệ thống.</p>
                        </td>
                    </tr>
                    <tr>
                        <td align='center' style='padding: 20px;'>
                            <p style='font-size: 14px; color: #666; border-top: 1px solid #ddd; padding-top: 10px;'>Trân trọng,</p>
                            <p style='font-family: Arial, sans-serif; font-size: 18px; font-weight: bold; color: #1a2a6c;'>KOI SHOW TEAM</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        public static string ShowInternalReviewNotificationForStaff(string staffName, KoiShow show)
        {
            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Thông báo triển lãm</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color: #f4f4f4;'>
        <tr>
            <td align='center' style='padding: 20px 0;'>
                <table border='0' cellpadding='0' cellspacing='0' width='600' style='background-color: #ffffff; border-radius: 10px; padding: 20px;'>
                    <tr>
                        <td align='center' style='font-family: Arial, sans-serif; color: #1a2a6c;'>
                            <h1 style='font-size: 24px; margin: 20px 0; color: #b21f1f;'>KOI SHOW MANAGEMENT SYSTEM</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style='font-family: Arial, sans-serif; font-size: 16px; line-height: 1.8; padding: 20px; color: #333;'>
                            <h2 style='color: #b21f1f; font-size: 20px;'>Kính gửi <span style='font-weight: bold;'>{staffName}</span>,</h2>
                            <p>Chúng tôi xin thông báo rằng triển lãm sau đây đã được chuyển sang trạng thái <strong>Nội bộ (InternalReview)</strong> và bạn đã được phân công làm <strong>Nhân viên</strong> cho triển lãm này:</p>
                            
                            <div style='background-color: #f9f9f9; padding: 15px; border-left: 4px solid #b21f1f; margin: 15px 0;'>
                                <h3 style='color: #b21f1f; margin-top: 0; font-size: 18px;'>{show.Name}</h3>
                                <p><strong>Thời gian:</strong> {show.StartDate:dd/MM/yyyy} - {show.EndDate:dd/MM/yyyy}</p>
                                <p><strong>Địa điểm:</strong> {show.Location}</p>
                            </div>
                            
                            <p><strong>Nhiệm vụ của bạn có thể bao gồm:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li>Hỗ trợ kiểm tra thông tin đăng ký</li>
                                <li>Hỗ trợ quy trình check-in</li>
                                <li>Xử lý các vấn đề phát sinh trong triển lãm</li>
                                <li>Thực hiện các nhiệm vụ do quản lý phân công</li>
                            </ul>
                            
                            <p>Hiện triển lãm đang ở trạng thái nội bộ, chỉ quản lý và nhân viên được phân công mới có thể xem. Vui lòng theo dõi hệ thống để nhận nhiệm vụ cụ thể từ quản lý.</p>
                            
                            
                            <p>Nếu bạn có bất kỳ câu hỏi hoặc cần hỗ trợ, vui lòng liên hệ với quản lý hoặc quản trị viên hệ thống.</p>
                        </td>
                    </tr>
                    <tr>
                        <td align='center' style='padding: 20px;'>
                            <p style='font-size: 14px; color: #666; border-top: 1px solid #ddd; padding-top: 10px;'>Trân trọng,</p>
                            <p style='font-family: Arial, sans-serif; font-size: 18px; font-weight: bold; color: #1a2a6c;'>KOI SHOW TEAM</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        public static string RefereeAssignmentNotification(string refereeName, KoiShow show,
            List<RefereeAssignmentInfo> assignments)
        {
            var groupedAssignments = assignments
                .GroupBy(a => a.CategoryName)
                .Select(group => new
                {
                    CategoryName = group.Key,
                    RoundTypes = string.Join(", ", group.Select(a => a.RoundTypeName))
                });

            var categoryList = string.Join("", groupedAssignments.Select(g => $@"
        <tr>
            <td style='padding: 12px; border-bottom: 1px solid #ddd;'>{g.CategoryName}</td>
            <td style='padding: 12px; border-bottom: 1px solid #ddd;'>{g.RoundTypes}</td>
        </tr>
    "));

            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Phân công trọng tài</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color: #f4f4f4;'>
        <tr>
            <td align='center' style='padding: 20px 0;'>
                <table border='0' cellpadding='0' cellspacing='0' width='600' style='background-color: #ffffff; border-radius: 10px; padding: 20px;'>
                    <tr>
                        <td align='center' style='font-family: Arial, sans-serif; color: #1a2a6c;'>
                            <h1 style='font-size: 24px; margin: 20px 0; color: #b21f1f;'>KOI SHOW</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style='font-family: Arial, sans-serif; font-size: 16px; line-height: 1.8; padding: 20px; color: #333;'>
                            <h2 style='color: #b21f1f; font-size: 20px;'>Kính gửi <span style='font-weight: bold;'>{refereeName}</span>,</h2>
                            <p>Chúng tôi xin thông báo rằng bạn đã được phân công làm <strong>Trọng tài</strong> cho sự kiện sau:</p>
                            
                            <div style='background-color: #f9f9f9; padding: 15px; border-left: 4px solid #b21f1f; margin: 15px 0;'>
                                <h3 style='color: #b21f1f; margin-top: 0; font-size: 18px;'>{show.Name}</h3>
                                <p><strong>Thời gian:</strong> {show.StartDate:dd/MM/yyyy} - {show.EndDate:dd/MM/yyyy}</p>
                                <p><strong>Địa điểm:</strong> {show.Location}</p>
                            </div>
                            
                            <p><strong>Chi tiết phân công của bạn:</strong></p>
                            
                            <table style='width: 100%; border-collapse: collapse; margin: 15px 0;'>
                                <thead>
                                    <tr style='background-color: #f0f0f0;'>
                                        <th style='padding: 12px; text-align: left; border-bottom: 2px solid #ddd;'>Hạng mục thi</th>
                                        <th style='padding: 12px; text-align: left; border-bottom: 2px solid #ddd;'>Vòng thi</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {categoryList}
                                </tbody>
                            </table>
                            
                            <p>Vui lòng đăng nhập vào hệ thống để xem chi tiết và chuẩn bị cho việc chấm điểm. Chúng tôi rất mong được làm việc cùng bạn để đảm bảo sự kiện diễn ra công bằng và thành công.</p>
                            
                            
                            <p>Nếu bạn có bất kỳ câu hỏi hoặc cần hỗ trợ, vui lòng liên hệ ban tổ chức.</p>
                        </td>
                    </tr>
                    <tr>
                        <td align='center' style='padding: 20px;'>
                            <p style='font-size: 14px; color: #666; border-top: 1px solid #ddd; padding-top: 10px;'>Trân trọng,</p>
                            <p style='font-family: Arial, sans-serif; font-size: 18px; font-weight: bold; color: #1a2a6c;'>KOI SHOW TEAM</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        public static string NewStaffAccountNotification(string fullName, string email, string password, string role)
        {
            string roleDisplay = role.ToLower() == "manager" ? "Quản lý" : "Nhân viên";

            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Thông báo tạo tài khoản {roleDisplay}</title>
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
                            
                            <p>Chúng tôi xin thông báo rằng tài khoản <span style='font-weight: bold; color: #b21f1f;'>{roleDisplay}</span> của bạn đã được tạo thành công trong Hệ thống Quản lý Triển lãm Cá Koi (KSMS).</p>
                            
                            <p><strong>Thông tin đăng nhập của bạn:</strong></p>
                            <div style='background-color: #f9f9f9; border-left: 4px solid #b21f1f; padding: 15px; margin: 15px 0;'>
                                <p><strong>Email:</strong> {email}</p>
                                <p><strong>Mật khẩu:</strong> {password}</p>
                                <p><strong>Vai trò:</strong> {roleDisplay}</p>
                            </div>
                            
                            <p>Với vai trò {roleDisplay}, bạn sẽ có quyền truy cập vào các tính năng quản lý của hệ thống. Vui lòng đăng nhập bằng thông tin trên và đổi mật khẩu của bạn sau khi đăng nhập lần đầu tiên.</p>
                            
                            <div style='margin: 25px 0; text-align: center;'>
                                <a href='https://ksms.news' style='background-color: #b21f1f; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>ĐĂNG NHẬP NGAY</a>
                            </div>
                            
                            <p><strong>Lưu ý quan trọng:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li style='margin-bottom: 10px;'>Vui lòng đổi mật khẩu của bạn ngay sau khi đăng nhập lần đầu tiên.</li>
                                <li style='margin-bottom: 10px;'>Giữ thông tin đăng nhập của bạn bảo mật và không chia sẻ với người khác.</li>
                                <li>Thông tin chi tiết về các cuộc triển lãm bạn được phân công sẽ được thông báo riêng.</li>
                            </ul>
                            
                            <p>Chúng tôi rất vui được hợp tác với bạn và mong đợi sự đóng góp của bạn cho sự thành công của các triển lãm cá Koi sắp tới.</p>
                            
                            <p>Nếu bạn cần hỗ trợ hoặc có bất kỳ câu hỏi nào, vui lòng liên hệ với quản trị viên hệ thống.</p>
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

        public static string NewRefereeAccountNotification(string fullName, string email, string password)
        {
            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Thông báo tạo tài khoản Trọng tài</title>
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
                            
                            <p>Chúng tôi vô cùng vinh dự thông báo rằng bạn đã được chọn làm <span style='font-weight: bold; color: #b21f1f;'>Trọng tài</span> cho các cuộc triển lãm cá Koi của chúng tôi. Tài khoản của bạn đã được tạo trong Hệ thống Quản lý Triển lãm Cá Koi (KSMS).</p>
                            
                            <p><strong>Thông tin đăng nhập của bạn:</strong></p>
                            <div style='background-color: #f9f9f9; border-left: 4px solid #b21f1f; padding: 15px; margin: 15px 0;'>
                                <p><strong>Email:</strong> {email}</p>
                                <p><strong>Mật khẩu:</strong> {password}</p>
                                <p><strong>Vai trò:</strong> Trọng tài</p>
                            </div>
                            
                            <p>Với kinh nghiệm và chuyên môn của bạn trong lĩnh vực cá Koi, chúng tôi tin rằng bạn sẽ đóng vai trò quan trọng trong việc đánh giá công bằng và chính xác các cá thể thi đấu.</p>
                            
                            
                            <p><strong>Thông tin quan trọng:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li style='margin-bottom: 10px;'>Vui lòng đổi mật khẩu của bạn ngay sau khi đăng nhập lần đầu tiên.</li>
                                <li style='margin-bottom: 10px;'>Giữ thông tin đăng nhập của bạn bảo mật và không chia sẻ với người khác.</li>
                                <li style='margin-bottom: 10px;'>Thông tin chi tiết về các cuộc triển lãm và hạng mục bạn được phân công chấm điểm sẽ được thông báo riêng sau khi cuộc triển lãm được công bố.</li>
                            </ul>
                            
                            <p>Chúng tôi rất vui mừng được hợp tác với bạn và mong đợi sự đóng góp chuyên môn của bạn cho sự thành công của các triển lãm cá Koi sắp tới.</p>
                            
                            <p>Nếu bạn cần hỗ trợ hoặc có bất kỳ câu hỏi nào, vui lòng liên hệ với ban tổ chức.</p>
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