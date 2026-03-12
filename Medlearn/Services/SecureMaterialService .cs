using Medlearn.DTOs;
using System.Net.Http.Json;

namespace Medlearn.Services.Implementations
{
    public interface ISecureMaterialService
    {
        Task<byte[]?> GetSecureMaterialViewAsync(int materialId);
        Task<Stream?> StreamSecureMaterialAsync(int materialId);
        Task<byte[]?> DownloadSecureMaterialAsync(int materialId);
        Task<string?> GetMaterialTextContentAsync(int materialId);
        Task<bool> HasAccessToMaterialAsync(int materialId);
    }

    public class SecureMaterialService : BaseHttpService, ISecureMaterialService
    {
        public SecureMaterialService(HttpClient httpClient, ITokenService tokenService) : base(httpClient, tokenService) { }

        public async Task<byte[]?> GetSecureMaterialViewAsync(int materialId)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Getting secure material view for ID: {materialId}");

                var response = await _httpClient.GetAsync($"api/SecureMaterials/{materialId}/view");

                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    Console.WriteLine($"Successfully retrieved material view, size: {bytes.Length} bytes");
                    return bytes;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to get material view: {response.StatusCode}, Content: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"GetSecureMaterialViewAsync for id {materialId}", ex);
                return null;
            }
        }

        public async Task<Stream?> StreamSecureMaterialAsync(int materialId)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Streaming secure material for ID: {materialId}");

                var response = await _httpClient.GetAsync($"api/SecureMaterials/{materialId}/stream", HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    Console.WriteLine($"Successfully started streaming material");
                    return stream;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to stream material: {response.StatusCode}, Content: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"StreamSecureMaterialAsync for id {materialId}", ex);
                return null;
            }
        }

        public async Task<byte[]?> DownloadSecureMaterialAsync(int materialId)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Downloading secure material for ID: {materialId}");

                var response = await _httpClient.GetAsync($"api/SecureMaterials/{materialId}/download");

                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    Console.WriteLine($"Successfully downloaded material, size: {bytes.Length} bytes");

                    // Try to get filename from content-disposition header
                    if (response.Content.Headers.TryGetValues("Content-Disposition", out var values))
                    {
                        var disposition = values.FirstOrDefault();
                        Console.WriteLine($"Download headers - Content-Disposition: {disposition}");
                    }

                    return bytes;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to download material: {response.StatusCode}, Content: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"DownloadSecureMaterialAsync for id {materialId}", ex);
                return null;
            }
        }

        public async Task<string?> GetMaterialTextContentAsync(int materialId)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Getting text content for material ID: {materialId}");

                var response = await _httpClient.GetAsync($"api/SecureMaterials/{materialId}/text");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(_jsonOptions);

                    if (result != null && result.ContainsKey("content"))
                    {
                        Console.WriteLine($"Successfully retrieved text content, length: {result["content"]?.Length ?? 0} characters");
                        return result["content"];
                    }
                    else
                    {
                        Console.WriteLine($"Text content response format invalid");
                        return null;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to get text content: {response.StatusCode}, Content: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"GetMaterialTextContentAsync for id {materialId}", ex);
                return null;
            }
        }

        public async Task<bool> HasAccessToMaterialAsync(int materialId)
        {
            try
            {
                // Try to get the material view - if it succeeds, we have access
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Checking access for material ID: {materialId}");

                var response = await _httpClient.GetAsync($"api/SecureMaterials/{materialId}/view");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"User has access to material {materialId}");
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Console.WriteLine($"User does NOT have access to material {materialId}");
                    return false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine($"User not authenticated for material {materialId}");
                    return false;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error checking access: {response.StatusCode}, Content: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"HasAccessToMaterialAsync for id {materialId}", ex);
                return false;
            }
        }

        // Helper method to get content type for file download
        public string GetContentTypeForDownload(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".ogg" => "video/ogg",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                ".wmv" => "video/x-ms-wmv",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".m4a" => "audio/mp4",
                ".txt" => "text/plain",
                ".html" or ".htm" => "text/html",
                ".md" => "text/markdown",
                ".rtf" => "text/rtf",
                _ => "application/octet-stream"
            };
        }
    }
}
