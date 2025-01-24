using KSMS.Domain.Common;

namespace KSMS.API.Configuration
{
    public static class ConfigureAppSettings
    {
        public static void SettingsBinding(this IConfiguration configuration)
        {
            AppConfig.ConnectionString = new ConnectionString();
            AppConfig.JwtSetting = new JwtSetting();
            AppConfig.GoogleImage = new GoogleImage();
            AppConfig.MailSetting = new MailSetting();

            configuration.Bind("ConnectionStrings", AppConfig.ConnectionString);
            configuration.Bind("JwtSettings", AppConfig.JwtSetting);
            configuration.Bind("GoogleImages", AppConfig.GoogleImage);
            configuration.Bind("MailSettings", AppConfig.MailSetting);
        }
    }
}
