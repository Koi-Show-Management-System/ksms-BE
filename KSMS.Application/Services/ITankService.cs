using KSMS.Domain.Dtos.Requests.Tank;
using KSMS.Domain.Dtos.Responses.Tank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Services
{
    public interface ITankService
    {
       
        Task<GetAllTankResponse> CreateTankAsync(CreateTankRequest request);

      
        Task<List<GetAllTankResponse>> GetTanksByKoiShowIdAsync(Guid koiShowId);
    }
}
