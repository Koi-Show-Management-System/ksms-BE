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
        Task<CriterionResponse> CreateCriterionAsync(CreateCriterionRequest createCriterionRequest);

        
        Task<CriterionResponse> GetCriterionByIdAsync(Guid id);

       
        Task<CriterionResponse> UpdateCriterionAsync(Guid id, UpdateCriterionRequest updateCriterionRequest);

        Task<Paginate<GetAllCriteriaResponse>> GetAllCriteria(int page, int size);
    
        Task DeleteCriterionAsync(Guid id);
    }
}
