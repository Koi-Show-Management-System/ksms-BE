using KSMS.Application.GoogleServices;
using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class MediaService : BaseService<MediaService>, IMediaService
{
    private readonly IFirebaseService _firebaseService;
    public MediaService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<MediaService> logger, IHttpContextAccessor httpContextAccessor, IFirebaseService firebaseService) : base(unitOfWork, logger, httpContextAccessor)
    {
        _firebaseService = firebaseService;
    }

    public async Task UploadRegistrationImage(List<IFormFile> imageFiles, Guid registrationId)
    {
        if (imageFiles is [])
        {
            throw new BadRequestException("No picture files found");
        }
        var folderPath = $"registration/{registrationId}";
        var imageUrls = await _firebaseService.UploadImagesAsync(imageFiles, folderPath);
        List<KoiMedium> images = [];
        foreach (var url in imageUrls)
        {
            images.Add(new KoiMedium()
            {
                RegistrationId = registrationId,
                MediaUrl = url,
                MediaType = MediaType.Image.ToString()
            });
        }
        await _unitOfWork.GetRepository<KoiMedium>().InsertRangeAsync(images);
        await _unitOfWork.CommitAsync();
    }

    public async Task UploadRegistrationVideo(List<IFormFile> videoFiles, Guid registrationId)
    {
        if (videoFiles is [])
        {
            throw new BadRequestException("No video files found");
        }
        var folderPath = $"registration/{registrationId}";
        var videoUrls = await _firebaseService.UploadVideosAsync(videoFiles, folderPath);
        List<KoiMedium> videos = [];
        foreach (var url in videoUrls)
        {
            videos.Add(new KoiMedium()
            {
                RegistrationId = registrationId,
                MediaUrl = url,
                MediaType = MediaType.Video.ToString()
            });
        }
        await _unitOfWork.GetRepository<KoiMedium>().InsertRangeAsync(videos);
        await _unitOfWork.CommitAsync();
    }

    public async Task UploadKoiImage(List<IFormFile> imageFiles, Guid koiProfileId)
    {
        if (imageFiles is [])
        {
            throw new BadRequestException("No picture files found");
        }
        var folderPath = $"KoiProfile/{koiProfileId}";
        var imageUrls = await _firebaseService.UploadImagesAsync(imageFiles, folderPath);
        List<KoiMedium> images = [];
        foreach (var url in imageUrls)
        {
            images.Add(new KoiMedium()
            {
                KoiProfileId = koiProfileId,
                MediaUrl = url,
                MediaType = MediaType.Image.ToString()
            });
        }
        await _unitOfWork.GetRepository<KoiMedium>().InsertRangeAsync(images);
        await _unitOfWork.CommitAsync();
    }

    public async Task UploadKoiVideos(List<IFormFile> videoFiles, Guid koiProfileId)
    {
        if (videoFiles is [])
        {
            throw new BadRequestException("No video files found");
        }
        var folderPath = $"KoiProfile/{koiProfileId}";
        var videoUrls = await _firebaseService.UploadVideosAsync(videoFiles, folderPath);
        List<KoiMedium> videos = [];
        foreach (var url in videoUrls)
        {
            videos.Add(new KoiMedium()
            {
                KoiProfileId = koiProfileId,
                MediaUrl = url,
                MediaType = MediaType.Video.ToString()
            });
        }
        await _unitOfWork.GetRepository<KoiMedium>().InsertRangeAsync(videos);
        await _unitOfWork.CommitAsync();
    }
}