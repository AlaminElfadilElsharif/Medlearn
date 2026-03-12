using Medlearn.DTOs;
using Medlearn.Model;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Medlearn.Services.Implementations
{
    public interface ISecureFileViewerService
    {
        Task<bool> CheckAccessAsync(int materialId);
        Task<string?> GetSecureViewUrlAsync(int materialId);
        Task<string?> GetSecureStreamUrlAsync(int materialId);
        Task<string?> GetSecureDownloadUrlAsync(int materialId);
        Task<string?> GetTextContentAsync(int materialId);
        Task<byte[]?> GetDocumentBytesAsync(int materialId);
        Task DownloadMaterialAsync(int materialId, string fileName);
        Task<LearningMaterialDto?> GetMaterialAsync(int materialId);

        // Clear cache methods
        void ClearAccessCache();
        void ClearAccessCacheForMaterial(int materialId);

        // File type detection methods
        bool IsPdfFile(string url);
        bool IsImageFile(string url);
        bool IsVideoFile(string url);
        bool IsAudioFile(string url);
        bool IsTextFile(string url);
        bool IsWordDocument(string url);
        bool IsExcelDocument(string url);
        bool IsPowerPointDocument(string url);
        string GetVideoMimeType(string url);
        string GetAudioMimeType(string url);
    }

    public class SecureFileViewerService : ISecureFileViewerService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly IJSRuntime _jsRuntime;
        private readonly IImageUrlService _imageUrlService;
        private readonly ISecureMaterialService _secureMaterialService;
        private readonly ILectureService _lectureService;
        private readonly JsonSerializerOptions _jsonOptions;

        // Cache dictionaries - static so they persist across component instances
        private static readonly Dictionary<int, (bool Access, DateTime Expiry)> _accessCache = new();
        private static readonly Dictionary<int, (bool IsFreePreview, DateTime Expiry)> _freePreviewCache = new();
        private static readonly Dictionary<int, (LearningMaterialDto Material, DateTime Expiry)> _materialCache = new();
        private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        public SecureFileViewerService(
            HttpClient httpClient,
            ITokenService tokenService,
            IJSRuntime jsRuntime,
            IImageUrlService imageUrlService,
            ISecureMaterialService secureMaterialService,
            ILectureService lectureService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _jsRuntime = jsRuntime;
            _imageUrlService = imageUrlService;
            _secureMaterialService = secureMaterialService;
            _lectureService = lectureService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<bool> CheckAccessAsync(int materialId)
        {
            Console.WriteLine($"[Cache] Checking access for material {materialId}");

            // STEP 1: Check access cache first (ZERO BANDWIDTH)
            await _cacheLock.WaitAsync();
            try
            {
                if (_accessCache.TryGetValue(materialId, out var cached))
                {
                    if (cached.Expiry > DateTime.UtcNow)
                    {
                        Console.WriteLine($"[Cache] HIT: Returning cached access {cached.Access}");
                        return cached.Access;
                    }
                    _accessCache.Remove(materialId);
                }
            }
            finally
            {
                _cacheLock.Release();
            }

            Console.WriteLine($"[Cache] MISS: No cached access for material {materialId}");

            try
            {
                // STEP 2: Get material info (cached or single fetch)
                var material = await GetMaterialFromCacheOrFetch(materialId);
                if (material == null)
                {
                    Console.WriteLine($"[Cache] Material {materialId} not found");
                    await CacheAccessResult(materialId, false, 1); // Cache negative for 1 minute
                    return false;
                }

                Console.WriteLine($"[Cache] Material {materialId}: {material.Title}, Lecture: {material.LectureId}");

                // STEP 3: Check if lecture is free preview (cached check)
                bool isFreePreview = await IsLectureFreePreview(material.LectureId);
                if (isFreePreview)
                {
                    Console.WriteLine($"[Cache] Lecture is free preview - granting immediate access");
                    await CacheAccessResult(materialId, true, 60); // Cache free preview for 60 minutes
                    return true;
                }

                // STEP 4: For non-free content, make ONE network call
                Console.WriteLine($"[Network] Making ONE network call for access check");
                bool hasAccess = await CheckAccessDirectlyAsync(materialId);

                // Cache result based on type
                int cacheMinutes = hasAccess ? 10 : 1; // Cache positive longer, negative shorter
                await CacheAccessResult(materialId, hasAccess, cacheMinutes);

                return hasAccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] CheckAccessAsync for {materialId}: {ex.Message}");
                return false;
            }
        }

        private async Task<LearningMaterialDto?> GetMaterialFromCacheOrFetch(int materialId)
        {
            // Check cache first
            await _cacheLock.WaitAsync();
            try
            {
                if (_materialCache.TryGetValue(materialId, out var cached) &&
                    cached.Expiry > DateTime.UtcNow)
                {
                    Console.WriteLine($"[Cache] Material cache HIT");
                    return cached.Material;
                }
            }
            finally
            {
                _cacheLock.Release();
            }

            // Fetch from API
            Console.WriteLine($"[Network] Fetching material {materialId}");
            var material = await GetMaterialFromApiAsync(materialId);

            if (material != null)
            {
                await _cacheLock.WaitAsync();
                try
                {
                    _materialCache[materialId] = (material, DateTime.UtcNow.AddMinutes(30));
                    Console.WriteLine($"[Cache] Material cached for 30 minutes");
                }
                finally
                {
                    _cacheLock.Release();
                }
            }

            return material;
        }

        private async Task<bool> IsLectureFreePreview(int lectureId)
        {
            if (lectureId <= 0) return false;

            // Check cache first
            await _cacheLock.WaitAsync();
            try
            {
                if (_freePreviewCache.TryGetValue(lectureId, out var cached) &&
                    cached.Expiry > DateTime.UtcNow)
                {
                    Console.WriteLine($"[Cache] Free preview cache HIT: {cached.IsFreePreview}");
                    return cached.IsFreePreview;
                }
            }
            finally
            {
                _cacheLock.Release();
            }

            // Fetch from service
            Console.WriteLine($"[Network] Checking if lecture {lectureId} is free preview");
            try
            {
                var lecture = await _lectureService.GetLectureAsync(lectureId);
                bool isFreePreview = lecture?.IsFreePreview == true;

                await _cacheLock.WaitAsync();
                try
                {
                    _freePreviewCache[lectureId] = (isFreePreview, DateTime.UtcNow.AddMinutes(60));
                    Console.WriteLine($"[Cache] Free preview cached for 60 minutes: {isFreePreview}");
                }
                finally
                {
                    _cacheLock.Release();
                }

                return isFreePreview;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to check free preview: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> CheckAccessDirectlyAsync(int materialId)
        {
            try
            {
                var token = await _tokenService.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("[Network] No token available");
                    return false;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/SecureMaterials/check-access/{materialId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<bool>();
                    Console.WriteLine($"[Network] Access check result: {result}");
                    return result;
                }

                Console.WriteLine($"[Network] Access check failed: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Direct access check: {ex.Message}");
                return false;
            }
        }

        private async Task CacheAccessResult(int materialId, bool hasAccess, int minutes)
        {
            await _cacheLock.WaitAsync();
            try
            {
                _accessCache[materialId] = (hasAccess, DateTime.UtcNow.AddMinutes(minutes));
                Console.WriteLine($"[Cache] Access result cached for {minutes} minutes: {hasAccess}");
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        private async Task<LearningMaterialDto?> GetMaterialFromApiAsync(int materialId)
        {
            try
            {
                var token = await _tokenService.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("[Network] No token for material fetch");
                    return null;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Materials/{materialId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var material = await response.Content.ReadFromJsonAsync<LearningMaterialDto>(_jsonOptions);
                    return material;
                }

                Console.WriteLine($"[Network] Failed to get material: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Get material from API: {ex.Message}");
                return null;
            }
        }

        public async Task<LearningMaterialDto?> GetMaterialAsync(int materialId)
        {
            return await GetMaterialFromCacheOrFetch(materialId);
        }

        public async Task<string?> GetSecureViewUrlAsync(int materialId)
        {
            try
            {
                var token = await _tokenService.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("[URL] No token available");
                    return null;
                }

                var secureUrl = $"/api/SecureMaterials/{materialId}/secure?token={Uri.EscapeDataString(token)}&t={DateTime.Now.Ticks}";
                return _imageUrlService.GetImageUrl(secureUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Get secure view URL: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> GetSecureStreamUrlAsync(int materialId)
        {
            try
            {
                var token = await _tokenService.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("[URL] No token available");
                    return null;
                }

                var secureUrl = $"/api/SecureMaterials/{materialId}/secure?token={Uri.EscapeDataString(token)}";
                return _imageUrlService.GetImageUrl(secureUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Get secure stream URL: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> GetSecureDownloadUrlAsync(int materialId)
        {
            try
            {
                var secureUrl = $"/api/SecureMaterials/{materialId}/download?t={DateTime.Now.Ticks}";
                return _imageUrlService.GetImageUrl(secureUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Get secure download URL: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> GetTextContentAsync(int materialId)
        {
            try
            {
                return await _secureMaterialService.GetMaterialTextContentAsync(materialId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Get text content: {ex.Message}");
                return null;
            }
        }

        public async Task<byte[]?> GetDocumentBytesAsync(int materialId)
        {
            try
            {
                return await _secureMaterialService.GetSecureMaterialViewAsync(materialId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Get document bytes: {ex.Message}");
                return null;
            }
        }

        public async Task DownloadMaterialAsync(int materialId, string fileName)
        {
            try
            {
                var token = await _tokenService.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                    throw new UnauthorizedAccessException("Not authenticated");

                var downloadUrl = $"/api/SecureMaterials/{materialId}/download";
                var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Failed to download: {response.StatusCode}");

                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                if (fileBytes == null || fileBytes.Length == 0)
                    throw new Exception("Empty file content");

                var contentType = response.Content.Headers.ContentType?.MediaType;
                string downloadedFileName = fileName;

                if (response.Content.Headers.ContentDisposition != null)
                {
                    downloadedFileName = response.Content.Headers.ContentDisposition.FileNameStar
                        ?? response.Content.Headers.ContentDisposition.FileName
                        ?? fileName;
                }

                downloadedFileName = downloadedFileName.Trim('"');

                if (string.IsNullOrEmpty(Path.GetExtension(downloadedFileName)) && !string.IsNullOrEmpty(contentType))
                {
                    var extension = GetExtensionFromContentType(contentType);
                    if (!string.IsNullOrEmpty(extension))
                    {
                        downloadedFileName += extension;
                    }
                }

                var base64 = Convert.ToBase64String(fileBytes);
                await _jsRuntime.InvokeVoidAsync("downloadFile", base64, downloadedFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Download material: {ex.Message}");
                throw;
            }
        }

        public void ClearAccessCache()
        {
            _cacheLock.Wait();
            try
            {
                _accessCache.Clear();
                _freePreviewCache.Clear();
                _materialCache.Clear();
                Console.WriteLine("[Cache] All caches cleared");
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        public void ClearAccessCacheForMaterial(int materialId)
        {
            _cacheLock.Wait();
            try
            {
                _accessCache.Remove(materialId);
                _materialCache.Remove(materialId);
                Console.WriteLine($"[Cache] Cache cleared for material {materialId}");
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        private string GetExtensionFromContentType(string contentType)
        {
            return contentType switch
            {
                "application/pdf" => ".pdf",
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "video/mp4" => ".mp4",
                "audio/mpeg" => ".mp3",
                "text/plain" => ".txt",
                "text/html" => ".html",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                "application/msword" => ".doc",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
                "application/vnd.ms-excel" => ".xls",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation" => ".pptx",
                "application/vnd.ms-powerpoint" => ".ppt",
                _ => ""
            };
        }

        // File type detection methods (local, no bandwidth)
        public bool IsPdfFile(string url)
        {
            var ext = Path.GetExtension(url)?.ToLower();
            return ext == ".pdf";
        }

        public bool IsImageFile(string url)
        {
            var ext = Path.GetExtension(url)?.ToLower();
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
            return imageExtensions.Contains(ext);
        }

        public bool IsVideoFile(string url)
        {
            var ext = Path.GetExtension(url)?.ToLower();
            var videoExtensions = new[] { ".mp4", ".webm", ".ogg", ".avi", ".mov", ".wmv", ".flv", ".mkv" };
            return videoExtensions.Contains(ext);
        }

        public bool IsAudioFile(string url)
        {
            var ext = Path.GetExtension(url)?.ToLower();
            var audioExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a", ".flac", ".aac" };
            return audioExtensions.Contains(ext);
        }

        public bool IsTextFile(string url)
        {
            var ext = Path.GetExtension(url)?.ToLower();
            var textExtensions = new[] { ".txt", ".md", ".rtf", ".html", ".htm", ".xml", ".json", ".csv" };
            return textExtensions.Contains(ext);
        }

        public bool IsWordDocument(string url)
        {
            var ext = Path.GetExtension(url)?.ToLower();
            var wordExtensions = new[] { ".doc", ".docx", ".dot", ".dotx", ".docm", ".dotm" };
            return wordExtensions.Contains(ext);
        }

        public bool IsExcelDocument(string url)
        {
            var ext = Path.GetExtension(url)?.ToLower();
            var excelExtensions = new[] { ".xls", ".xlsx", ".xlsm", ".xlsb", ".xlt", ".xltx", ".xltm" };
            return excelExtensions.Contains(ext);
        }

        public bool IsPowerPointDocument(string url)
        {
            var ext = Path.GetExtension(url)?.ToLower();
            var powerpointExtensions = new[] { ".ppt", ".pptx", ".pps", ".ppsx", ".pot", ".potx", ".pptm", ".potm", ".ppsm" };
            return powerpointExtensions.Contains(ext);
        }

        public string GetVideoMimeType(string url)
        {
            var ext = Path.GetExtension(url)?.ToLower();
            return ext switch
            {
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".ogg" => "video/ogg",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                ".wmv" => "video/x-ms-wmv",
                _ => "video/mp4"
            };
        }

        public string GetAudioMimeType(string url)
        {
            var ext = Path.GetExtension(url)?.ToLower();
            return ext switch
            {
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".ogg" => "audio/ogg",
                ".m4a" => "audio/mp4",
                ".flac" => "audio/flac",
                ".aac" => "audio/aac",
                _ => "audio/mpeg"
            };
        }
    }
}