using Microsoft.AspNetCore.Http;

namespace KSMS.Application.GoogleServices;

public interface IFirebaseService
{
    Task<string> UploadImageAsync(IFormFile imageFile, string imagePath);

    Task<string> UploadVideoAsync(IFormFile videoFile, string videoPath);
    Task<string[]> UploadImagesAsync(List<IFormFile> imageFiles, string folderPath);
    Task<string[]> UploadVideosAsync(List<IFormFile> videoFiles, string folderPath);
    string GetImageUrl(string folderName, string imageName);

    Task DeleteImageAsync(string imageName);

    Task DeleteImagesAsync(List<string> imageUrls);

    
    
    


}