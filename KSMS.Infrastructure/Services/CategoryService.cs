using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Entities;
using KSMS.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mapster;
using KSMS.Domain.Dtos.Responses.Categorie;
using KSMS.Domain.Pagination;

namespace KSMS.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork<KoiShowManagementSystemContext> _unitOfWork;

        public CategoryService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Paginate<CategorieResponse>> GetPagedRegistrationsInShow(Guid showId, int page, int size)
        {
            var categoryRepository = _unitOfWork.GetRepository<Category>();

          
            var pagedCategories = await categoryRepository.GetPagingListAsync(
                orderBy: query => query.OrderBy(c => c.Name),  
                include: query => query.Include(c => c.Registrations),  
                predicate: c => c.ShowId == showId,  
                page: page,
                size: size
            );

            
            var pagedCategorieResponses = pagedCategories.Adapt<Paginate<CategorieResponse>>();

            
            foreach (var categoryResponse in pagedCategorieResponses.Items)
            {
             
                categoryResponse.Registrations = categoryResponse.Registrations
                    .Adapt<List<RegistrationStaffResponse>>();

            
                
            }

            return pagedCategorieResponses;
        }

    }
}
