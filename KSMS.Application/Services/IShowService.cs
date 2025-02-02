using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Dtos.Responses.Show;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Services
{
    public interface IShowService
    {
        Task<IEnumerable<ShowResponse>> GetAllShowsAsync();
        Task<ShowResponse> GetShowByIdAsync(Guid id);
        Task<ShowResponse> CreateShowAsync(CreateShowRequest request);
        Task UpdateShowAsync(Guid id, UpdateShowRequest request);
        Task PatchShowStatusAsync(Guid id, string statusName);
    }

}
