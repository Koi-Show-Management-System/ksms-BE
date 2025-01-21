using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Infrastructure.Utils
{
    public static class PasswordUtil
    {
        public static string HashPassword(string rawPassword)
        {
            byte[] bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(rawPassword));
            return Convert.ToBase64String(bytes);
        }
    }
}
