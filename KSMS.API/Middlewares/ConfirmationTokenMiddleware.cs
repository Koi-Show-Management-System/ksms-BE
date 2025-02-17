using KSMS.Application.Repositories;
using KSMS.Domain.Entities;
using KSMS.Infrastructure.Database;

namespace KSMS.API.Middlewares;

public class ConfirmationTokenMiddleware
{
    private readonly RequestDelegate _next;
    
    // Định nghĩa các icon SVG như là private static readonly fields

    public ConfirmationTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using (var scope = context.RequestServices.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();


            var token = context.Request.Query["token"].ToString();

            if (!string.IsNullOrEmpty(token))
            {
                var user = await unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(predicate:x => x.ConfirmationToken == token);
                if (user.IsConfirmed != true)
                {
                    user.IsConfirmed = true;
                    //user.ConfirmationToken = null;
                    unitOfWork.GetRepository<Account>().UpdateAsync(user);
                    await unitOfWork.CommitAsync();

                    var emailContent = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Email Confirmation</title>
    <link href='https://fonts.googleapis.com/icon?family=Material+Icons' rel='stylesheet'>
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
                            <div style='text-align: center;'>
                                <span class='material-icons' style='font-size: 64px; color: #b21f1f;'>check_circle</span>
                                <h2 style='color: #b21f1f; margin: 10px 0;'>Email Confirmed Successfully!</h2>
                            </div>
                            <p style='text-align: center;'>Your email has been successfully verified. You can now access all features of the Koi Show platform.</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='http://localhost:5173/login' 
                                   style='font-size: 16px; font-weight: bold; color: #ffffff; background-color: #b21f1f; 
                                          padding: 15px 30px; text-decoration: none; border-radius: 5px;'>
                                    Go to Login
                                </a>
                            </div>
                            <p style='text-align: center; color: #666; font-size: 14px;'>
                                If you have any questions or need assistance, please don't hesitate to contact our support team.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td align='center' style='padding: 20px;'>
                            <p style='font-size: 14px; color: #666; border-top: 1px solid #ddd; padding-top: 10px;'>Best Regards,</p>
                            <p style='font-family: Arial, sans-serif; font-size: 18px; font-weight: bold; color: #1a2a6c;'>Koi Show Team</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(emailContent);
                    return;
                }
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KOI SHOW - Invalid Token</title>
    <link href='https://fonts.googleapis.com/icon?family=Material+Icons' rel='stylesheet'>
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
                            <div style='text-align: center;'>
                                <span class='material-icons' style='font-size: 64px; color: #b21f1f;'>error</span>
                                <h2 style='color: #b21f1f; margin: 10px 0;'>Invalid or Expired Token</h2>
                            </div>
                            <p style='text-align: center;'>The confirmation token is invalid or your account has already been confirmed.</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='http://localhost:5173/login' 
                                   style='font-size: 16px; font-weight: bold; color: #ffffff; background-color: #b21f1f; 
                                          padding: 15px 30px; text-decoration: none; border-radius: 5px;'>
                                    Return to Login
                                </a>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td align='center' style='padding: 20px;'>
                            <p style='font-size: 14px; color: #666; border-top: 1px solid #ddd; padding-top: 10px;'>Best Regards,</p>
                            <p style='font-family: Arial, sans-serif; font-size: 18px; font-weight: bold; color: #1a2a6c;'>Koi Show Team</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>");
            }
        }
        await _next(context);
    }
}