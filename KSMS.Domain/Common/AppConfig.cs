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
}
