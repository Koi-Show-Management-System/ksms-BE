using KSMS.Domain.Dtos.Responses.Categorie;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Services
{
    public interface ICategoryService
    {
        Task<Paginate<CategorieResponse>> GetPagedRegistrationsInShow(Guid showId, int page, int size);
    }
}
