using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Dtos.Responses.KoiShow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Services
{
    public interface IShowService
    {
        Task<IEnumerable<KoiShowResponse>> GetAllShowsAsync(); 
        Task<KoiShowResponse> GetShowByIdAsync(Guid id);
        Task<KoiShowResponse> CreateShowAsync(CreateShowRequest request);
        //Task UpdateShowAsync(Guid id, UpdateShowRequest request);
        //Task PatchShowStatusAsync(Guid id, string statusName);
    }

}
