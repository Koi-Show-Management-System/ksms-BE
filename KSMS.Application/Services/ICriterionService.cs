using KSMS.Domain.Dtos.Requests.Criterion;
using KSMS.Domain.Dtos.Responses.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services
{
    public interface ICriterionService
    {
        Task<CriteriaResponse> CreateCriteriaAsync(CreateCriteriaRequest createCriteriaRequest);

        
        Task<CriteriaResponse> GetCriteriaByIdAsync(Guid id);

       
        Task<CriteriaResponse> UpdateCriteriaAsync(Guid id, UpdateCriteriaRequest updateCriteriaRequest);

        Task<Paginate<GetAllCriteriaResponse>> GetPagingCriteria(int page, int size);
    
        Task DeleteCriteriaAsync(Guid id);
    }
}
