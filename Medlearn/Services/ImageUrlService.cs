using Microsoft.JSInterop;

namespace Medlearn.Services.Implementations
{
    public interface IImageUrlService
    {
        string GetImageUrl(string? relativePath);
    }

    public class ImageUrlService : IImageUrlService
    {
        private readonly string _apiBaseUrl = "https://localhost:7127";

        public string GetImageUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return "/placeholder-image.jpg";

            // Remove the hardcoded base URL and return relative path
            // This will go through your proxy at localhost:3000
            return relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
        }
    }
}
