using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Services
{
    public interface    ICacheService
    {
        Task<bool> LockAsync(string key, TimeSpan expiration);
       Task UnlockAsync(string key);
    }


}
