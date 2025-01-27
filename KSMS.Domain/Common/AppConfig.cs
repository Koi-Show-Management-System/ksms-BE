using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Common
{
    public class AppConfig
    {
        public static JwtSetting JwtSetting { get; set; } = null!;
        public static ConnectionString ConnectionString { get; set; } = null!;
        public static GoogleImage GoogleImage { get; set; } = null!;
        public static MailSetting MailSetting { get; set; } = null!;
        public static PayOs PayOs { get; set; } = null!;

    }
    public class ConnectionString
    {
        public string DefaultConnection { get; set; } = string.Empty;
    }
    public class JwtSetting
    {
        public string Key { get; set; } = "Secret Key";
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
    }
    public class GoogleImage
    {
        public string? Type { get; set; }
        public string? ProjectId { get; set; }
        public string? PrivateKeyId { get; set; }
        public string? PrivateKey { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientId { get; set; }
    }
    public class MailSetting
    {
        public string HostEmail { get; set; }
        public int PortEmail { get; set; }
        public string EmailSender { get; set; }
        public string PasswordSender { get; set; }
    }

    public class PayOs
    {
        public string? ClientId { get; set; }
        public string? ApiKey { get; set; }
        public string? ChecksumKey { get; set; }
    }
}
