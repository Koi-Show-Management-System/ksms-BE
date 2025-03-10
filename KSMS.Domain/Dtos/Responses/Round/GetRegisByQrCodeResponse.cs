using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Round
{
   
    public class GetRegisByQrCodeResponse
    {
        public Guid RegistrationId { get; set; }
        public string Qrcode { get; set; }
    }

}
