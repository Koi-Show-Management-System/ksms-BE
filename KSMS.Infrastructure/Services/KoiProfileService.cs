// using System.Linq.Expressions;
// using System.Security.Claims;
// using KSMS.Application.Extensions;
// using KSMS.Application.GoogleServices;
// using KSMS.Application.Repositories;
// using KSMS.Application.Services;
// using KSMS.Domain.Dtos.Requests.KoiProfile;
// using KSMS.Domain.Dtos.Responses.KoiProfile;
// using KSMS.Domain.Entities;
// using KSMS.Domain.Exceptions;
// using KSMS.Domain.Models;
// using KSMS.Domain.Pagination;
// using KSMS.Infrastructure.Database;
// using Mapster;
// using Microsoft.AspNetCore.Http;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
//
// namespace KSMS.Infrastructure.Services;
//
// public class KoiProfileService : BaseService<KoiProfileService>, IKoiProfileService
// {
//     private readonly IFirebaseService _firebaseService;
//    
//     public KoiProfileService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<KoiProfileService> logger, IHttpContextAccessor httpContextAccessor, IFirebaseService firebaseService) : base(unitOfWork, logger, httpContextAccessor)
//     {
//         _firebaseService = firebaseService;
//     }
//     public async Task<object> CreateKoiProfile(ClaimsPrincipal claims, CreateKoiProfileRequest createKoiProfileRequest)
//     {
//         var accountId = claims.GetAccountId();
//         var variety = await _unitOfWork.GetRepository<Variety>()
//             .SingleOrDefaultAsync(predicate: v => v.Id == createKoiProfileRequest.VarietyId);
//         if (variety is null)
//         {
//             throw new NotFoundException("Variety is not found");
//         }
//         var koi = await _unitOfWork.GetRepository<KoiProfile>().SingleOrDefaultAsync(predicate: k =>
//             k.Name.ToLower() == createKoiProfileRequest.Name.ToLower() && k.OwnerId == accountId);
//         if (koi is not null)
//         {
//             throw new BadRequestException("Koi is already add to your list");
//         }
//
//         var koiProfile = createKoiProfileRequest.Adapt<KoiProfile>();
//         koiProfile.OwnerId = accountId;
//         if (createKoiProfileRequest.Img is not null)
//         {
//             koiProfile.ImgUrl = await _firebaseService.UploadImageAsync(createKoiProfileRequest.Img, "koi/");
//         }
//         if (createKoiProfileRequest.Video is not null)
//         {
//             koiProfile.VideoUrl = await _firebaseService.UploadImageAsync(createKoiProfileRequest.Video, "koi/");
//         }
//         
//         await _unitOfWork.GetRepository<KoiProfile>().InsertAsync(koiProfile);
//         await _unitOfWork.CommitAsync();
//         return new
//         {
//             id = koiProfile.Id
//         };
//     }
//
//     public async Task<Paginate<GetAllKoiProfileResponse>> GetPagedKoiProfile(ClaimsPrincipal claims, KoiProfileFilter filter, int page, int size)
//     {
//         var accountId = claims.GetAccountId();
//         var listKoi = await _unitOfWork.GetRepository<KoiProfile>().GetPagingListAsync(predicate: ApplyKoiFilter(filter, accountId),
//             page: page, size: size, include: query => query.Include(k => k.Variety));
//         return listKoi.Adapt<Paginate<GetAllKoiProfileResponse>>();
//     }
//
//     public async Task<object> UpdateKoiProfile(ClaimsPrincipal claims, Guid id, UpdateKoiProfileRequest updateKoiProfileRequest)
//     {
//         var accountId = claims.GetAccountId();
//         var koi = await _unitOfWork.GetRepository<KoiProfile>().SingleOrDefaultAsync(predicate: k => k.Id == id);
//         if (koi is null)
//         {
//             throw new NotFoundException("Koi is not existed");
//         }
//
//         if (koi.OwnerId != accountId)
//         {
//             throw new UnauthorizedException("This koi is not yours!!!!!");
//         }
//         
//         if (updateKoiProfileRequest.Name is not null)
//         {
//             var existingKoi = await _unitOfWork.GetRepository<KoiProfile>().SingleOrDefaultAsync(predicate: k =>
//                 k.Name.ToLower() == updateKoiProfileRequest.Name.ToLower() && k.OwnerId == accountId);
//
//             if (existingKoi is not null && existingKoi.Id != koi.Id)
//             {
//                 throw new BadRequestException("Name is already existed. Please choose another name");
//             }
//         }
//         if (updateKoiProfileRequest.VarietyId is not null)
//         {
//             var variety = await _unitOfWork.GetRepository<Variety>()
//                 .SingleOrDefaultAsync(predicate: v => v.Id == updateKoiProfileRequest.VarietyId);
//             if (variety is null)
//             {
//                 throw new NotFoundException("Variety is not found");
//             }
//         }
//
//         if (koi.ImgUrl is not null)
//         {
//             await _firebaseService.DeleteImageAsync(koi.ImgUrl);
//             koi.ImgUrl = null;
//         }
//
//         if (koi.VideoUrl is not null)
//         {
//             await _firebaseService.DeleteImageAsync(koi.VideoUrl);
//             koi.VideoUrl = null;
//         }
//
//         if (updateKoiProfileRequest.Img is not null)
//         {
//             koi.ImgUrl = await _firebaseService.UploadImageAsync(updateKoiProfileRequest.Img, "koi/");
//             
//         }
//
//         if (updateKoiProfileRequest.Video is not null)
//         {
//             koi.VideoUrl = await _firebaseService.UploadImageAsync(updateKoiProfileRequest.Video, "koi/");
//         }
//         updateKoiProfileRequest.Adapt(koi);
//         _unitOfWork.GetRepository<KoiProfile>().UpdateAsync(koi);
//         await _unitOfWork.CommitAsync();
//         return new { message = "Update Koi Successfully" };
//     }
//
//     private Expression<Func<KoiProfile, bool>> ApplyKoiFilter(KoiProfileFilter? filter, Guid accountId)
//     {
//         if (filter == null) return koi => koi.OwnerId == accountId;
//
//         Expression<Func<KoiProfile, bool>> filterQuery = koi => koi.OwnerId == accountId;
//         if (filter.StartSize < filter.EndSize)
//         {
//             filterQuery = filterQuery.AndAlso(koi => koi.Size >= filter.StartSize && koi.Size <= filter.EndSize);
//         }
//         if (filter.VarietyIds is not [])
//         {
//             filterQuery = filterQuery.AndAlso(koi => filter.VarietyIds.Contains(koi.VarietyId));
//         }
//         if (!string.IsNullOrEmpty(filter.Name))
//         {
//             filterQuery = filterQuery.AndAlso(koi => !string.IsNullOrEmpty(koi.Name) &&
//                                                      koi.Name.ToLower().Contains(filter.Name.ToLower()));
//         }
//         return filterQuery;
//     }
//
//     
// }