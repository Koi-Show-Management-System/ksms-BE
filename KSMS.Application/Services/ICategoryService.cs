using KSMS.Domain.Dtos.Responses.CompetitionCategory;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSMS.Domain.Dtos.Requests.Categorie;

namespace KSMS.Application.Services
{
    public interface ICategoryService
    {
        Task CreateCompetitionCategory(CreateCompetitionCategoryRequest request);
        
        Task UpdateCompetitionCategory(Guid id, UpdateCompetitionCategoryRequest request);
        Task<GetCompetitionCategoryDetailResponse> GetCompetitionCategoryDetailById(Guid id);
        Task<Paginate<GetPageCompetitionCategoryResponse>> GetPagedCompetitionCategory(Guid showId, bool? hasTank, int page, int size);
    }
}
