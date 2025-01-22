using Microsoft.AspNetCore.Http;

namespace KSMS.Application.GoogleServices;

public interface IFirebaseService
{
    Task<string> UploadImageAsync(IFormFile imageFile, string imagePath);

    string GetImageUrl(string folderName, string imageName);

    Task DeleteImageAsync(string imageName);

    Task DeleteImagesAsync(List<string> imageUrls);

    Task<string[]> UploadImagesAsync(List<IFormFile> imageFiles, string folderPath);
    
}