using KSMS.Domain.Dtos.Requests.Tank;
using KSMS.Domain.Dtos.Responses.Tank;
using KSMS.Domain.Enums;
using KSMS.Domain.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Services
{
    public interface ITankService
    {
        Task<int> GetCurrentFishCount(Guid tankId);

        Task<bool> IsTankFull(Guid tankId);
        Task<Paginate<TankResponse>> GetPagedTanksByKoiShowIdAsync(Guid koiShowId, int page, int size);
        Task UpdateTankStatusAsync(Guid id, TankStatus status);
         
        Task CreateTankAsync(CreateTankRequest request);
        Task UpdateTankAsync(Guid id, UpdateTankRequest request);

      //  Task<List<TankResponse>> GetTanksByKoiShowIdAsync(Guid koiShowId);
    }
}
