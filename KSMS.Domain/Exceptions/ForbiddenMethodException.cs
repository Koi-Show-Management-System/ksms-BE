using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Exceptions
{
    public class ForbiddenMethodException(string message) : ApplicationException(message)
    {
    }
}
