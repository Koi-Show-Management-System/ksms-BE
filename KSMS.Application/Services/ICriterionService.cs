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
        Task<GetAllCriterionResponse> CreateCriterionAsync(CreateCriterionRequest createCriterionRequest);

        
        Task<GetAllCriterionResponse> GetCriterionByIdAsync(Guid id);

       
        Task<GetAllCriterionResponse> UpdateCriterionAsync(Guid id, UpdateCriterionRequest updateCriterionRequest);

        Task<Paginate<GetAllCriteriaResponse>> GetAllCriteria(int page, int size);
    
        Task DeleteCriterionAsync(Guid id);
    }
}
