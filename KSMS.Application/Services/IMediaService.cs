using Microsoft.AspNetCore.Http;

namespace KSMS.Application.Services;

public interface IMediaService
{
    Task UploadRegistrationImage(List<IFormFile> imageFiles, Guid registrationId);
    Task UploadRegistrationVideo(List<IFormFile> videoFiles, Guid registrationId);
    Task UploadKoiImage(List<IFormFile> imageFiles, Guid koiProfileId);
    Task UploadKoiVideos(List<IFormFile> videoFiles, Guid koiProfileId);
}