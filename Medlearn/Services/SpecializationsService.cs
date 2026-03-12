using Medlearn.DTOs;
using System.Net.Http.Json;

namespace Medlearn.Services.Implementations
{
    public interface ISpecializationService
    {
        Task<List<SpecializationDto>?> GetSpecializationsAsync(
            string? search = null,
            bool? featured = null,
            int? doctorId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int page = 1,
            int pageSize = 12);

        Task<SpecializationDto?> GetSpecializationAsync(int id);
        Task<SpecializationDto?> CreateSpecializationAsync(CreateSpecializationDto dto, Stream? thumbnailStream, string fileName);
        Task<bool> UpdateSpecializationAsync(int id, UpdateSpecializationDtos dto, Stream? thumbnailStream, string fileName);
        Task<bool> DeleteSpecializationAsync(int id);
        Task<List<SpecializationDto>?> GetMySpecializationsAsync();
        Task<bool> UpdateSpecializationStatusAsync(int id, bool isActive);


    }

    public class SpecializationService : BaseHttpService, ISpecializationService
    {
        public SpecializationService(HttpClient httpClient, ITokenService tokenService) : base(httpClient, tokenService) { }

        public async Task<List<SpecializationDto>?> GetSpecializationsAsync(
            string? search = null,
            bool? featured = null,
            int? doctorId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int page = 1,
            int pageSize = 12)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            if (featured.HasValue)
                queryParams.Add($"featured={featured.Value}");

            if (doctorId.HasValue)
                queryParams.Add($"doctorId={doctorId.Value}");

            if (minPrice.HasValue)
                queryParams.Add($"minPrice={minPrice.Value}");

            if (maxPrice.HasValue)
                queryParams.Add($"maxPrice={maxPrice.Value}");

            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var endpoint = $"api/specializations?{string.Join("&", queryParams)}";
            return await GetAsync<List<SpecializationDto>>(endpoint);
        }

        public async Task<SpecializationDto?> GetSpecializationAsync(int id)
        {
            return await GetAsync<SpecializationDto>($"api/specializations/{id}");
        }
        // In SpecializationService
        public async Task<bool> UpdateSpecializationStatusAsync(int id, bool isActive)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Calling UpdateSpecializationStatusAsync for ID: {id}, IsActive: {isActive}");

                var statusDto = new { IsActive = isActive };

                var response = await _httpClient.PutAsJsonAsync(
                    $"api/specializations/{id}/status",
                    statusDto,
                    _jsonOptions);

                Console.WriteLine($"Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Specialization {id} status updated successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error updating specialization status: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateSpecializationStatusAsync: {ex.Message}");
                await LogErrorAsync($"UpdateSpecializationStatusAsync for id {id}", ex);
                return false;
            }
        }

        public async Task<bool> DeleteSpecializationAsync(int id)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/specializations/{id}");
            return response.IsSuccessStatusCode;
        }

        // Similar methods for CourseService and LectureService
        public async Task<SpecializationDto?> CreateSpecializationAsync(
            CreateSpecializationDto dto, Stream? thumbnailStream, string fileName)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Creating specialization with file: {fileName != null}, Title: {dto.Title}");

                if (thumbnailStream != null && !string.IsNullOrEmpty(fileName))
                {
                    // Create multipart form data for file upload
                    var content = new MultipartFormDataContent();

                    // Add form fields as StringContent
                    content.Add(new StringContent(dto.Title), "Title");
                    content.Add(new StringContent(dto.Description), "Description");
                    content.Add(new StringContent(dto.Price.ToString()), "Price");
                    content.Add(new StringContent(dto.EstimatedDurationHours.ToString()), "EstimatedDurationHours");
                    content.Add(new StringContent(dto.Prerequisites ?? ""), "Prerequisites");
                    content.Add(new StringContent(dto.IsFeatured.ToString()), "IsFeatured");

                    // Reset stream position to beginning
                    if (thumbnailStream.CanSeek)
                        thumbnailStream.Position = 0;

                    // Add file content
                    var fileContent = new StreamContent(thumbnailStream);

                    // Try to determine content type from file name
                    string contentType = "image/jpeg";
                    if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        contentType = "image/png";
                    else if (fileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                        contentType = "image/gif";

                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    content.Add(fileContent, "ThumbnailImage", fileName);

                    Console.WriteLine($"Sending multipart form data with file: {fileName}");

                    var response = await _httpClient.PostAsync("api/specializations", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<SpecializationDto>(_jsonOptions);
                        Console.WriteLine("Specialization created successfully with file");
                        return result;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to create specialization with file: {response.StatusCode}, Content: {errorContent}");
                        return null;
                    }
                }
                else
                {
                    // No file, send as JSON
                    Console.WriteLine("Sending specialization data as JSON (no file)");

                    var response = await _httpClient.PostAsJsonAsync("api/specializations", dto, _jsonOptions);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<SpecializationDto>(_jsonOptions);
                        Console.WriteLine("Specialization created successfully without file");
                        return result;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to create specialization without file: {response.StatusCode}, Content: {errorContent}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"CreateSpecializationAsync", ex);
                return null;
            }
        }

        public async Task<bool> UpdateSpecializationAsync(
            int id, UpdateSpecializationDtos dto, Stream? thumbnailStream, string fileName)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Updating specialization {id} with file: {fileName != null}");

                if (thumbnailStream != null && !string.IsNullOrEmpty(fileName))
                {
                    // Create multipart form data for file upload
                    var content = new MultipartFormDataContent();

                    // Add form fields only if they have values
                    if (!string.IsNullOrEmpty(dto.Title))
                        content.Add(new StringContent(dto.Title), "Title");

                    if (!string.IsNullOrEmpty(dto.Description))
                        content.Add(new StringContent(dto.Description), "Description");

                    if (dto.Price.HasValue)
                        content.Add(new StringContent(dto.Price.Value.ToString()), "Price");

                    if (dto.EstimatedDurationHours.HasValue)
                        content.Add(new StringContent(dto.EstimatedDurationHours.Value.ToString()), "EstimatedDurationHours");

                    if (!string.IsNullOrEmpty(dto.Prerequisites))
                        content.Add(new StringContent(dto.Prerequisites), "Prerequisites");

                    if (dto.IsFeatured.HasValue)
                        content.Add(new StringContent(dto.IsFeatured.Value.ToString()), "IsFeatured");

                    // Reset stream position to beginning
                    if (thumbnailStream.CanSeek)
                        thumbnailStream.Position = 0;

                    // Add file content
                    var fileContent = new StreamContent(thumbnailStream);

                    // Try to determine content type from file name
                    string contentType = "image/jpeg";
                    if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        contentType = "image/png";
                    else if (fileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                        contentType = "image/gif";

                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    content.Add(fileContent, "ThumbnailImage", fileName);

                    Console.WriteLine($"Sending multipart form data update with file: {fileName}");

                    var response = await _httpClient.PutAsync($"api/specializations/{id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Specialization {id} updated successfully with file");
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to update specialization with file: {response.StatusCode}, Content: {errorContent}");
                        return false;
                    }
                }
                else
                {
                    // No file, send as JSON
                    Console.WriteLine("Sending specialization update data as JSON (no file)");

                    var response = await _httpClient.PutAsJsonAsync($"api/specializations/{id}", dto, _jsonOptions);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Specialization {id} updated successfully without file");
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to update specialization without file: {response.StatusCode}, Content: {errorContent}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"UpdateSpecializationAsync for id {id}", ex);
                return false;
            }
        }

      

        public async Task<List<SpecializationDto>?> GetMySpecializationsAsync()
        {
            return await GetAsync<List<SpecializationDto>>("api/specializations/doctor/my");
        }
    }
}
