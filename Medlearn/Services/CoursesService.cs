using Medlearn.DTOs;
using System.Net.Http.Json;

namespace Medlearn.Services.Implementations
{
    public interface ICourseService
    {
        Task<List<CourseDto>?> GetCoursesBySpecializationAsync(int specializationId);
        Task<CourseDto?> GetCourseAsync(int id);
        Task<CourseDto?> CreateCourseAsync(CreateCourseRequestDto dto, Stream? thumbnailStream, string fileName);
        Task<bool> UpdateCourseAsync(int id, UpdateCourseRequestDto dto, Stream? thumbnailStream, string fileName);
        Task<bool> DeleteCourseAsync(int id);
        Task<bool> UpdateCourseStatusAsync(int id, bool isActive);

    }

    public class CourseService : BaseHttpService, ICourseService
    {
        public CourseService(HttpClient httpClient, ITokenService tokenService)
        : base(httpClient, tokenService) { }

        public async Task<List<CourseDto>?> GetCoursesBySpecializationAsync(int specializationId)
        {
            return await GetAsync<List<CourseDto>>($"api/courses/specialization/{specializationId}");
        }

        public async Task<CourseDto?> GetCourseAsync(int id)
        {
            return await GetAsync<CourseDto>($"api/courses/{id}");
        }
        public async Task<bool> UpdateCourseStatusAsync(int id, bool isActive)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Updating course {id} status to: {isActive}");

                var statusDto = new { IsActive = isActive };

                var response = await _httpClient.PutAsJsonAsync(
                    $"api/courses/{id}/status",
                    statusDto,
                    _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Course {id} status updated successfully to: {isActive}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to update course status: {response.StatusCode}, Content: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"UpdateCourseStatusAsync for id {id}", ex);
                return false;
            }
        }
        public async Task<CourseDto?> CreateCourseAsync(CreateCourseRequestDto dto, Stream? thumbnailStream, string fileName)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Creating course with file: {fileName != null}, Title: {dto.Title}");

                if (thumbnailStream != null && !string.IsNullOrEmpty(fileName))
                {
                    // Create multipart form data for file upload
                    var content = new MultipartFormDataContent();

                    // Add all required form fields as StringContent
                    content.Add(new StringContent(dto.Title), "Title");
                    content.Add(new StringContent(dto.Description), "Description");
                    content.Add(new StringContent(dto.Price.ToString()), "Price");
                    content.Add(new StringContent(dto.Order.ToString()), "Order");
                    content.Add(new StringContent(dto.SpecializationId.ToString()), "SpecializationId");

                    // DO NOT add ThumbnailUrl for new courses - it's not needed!
                    // The backend will generate it from the uploaded file

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

                    var response = await _httpClient.PostAsync("api/courses", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<CourseDto>(_jsonOptions);
                        Console.WriteLine("Course created successfully with file");
                        return result;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to create course with file: {response.StatusCode}, Content: {errorContent}");
                        return null;
                    }
                }
                else
                {
                    // No file, send as JSON
                    Console.WriteLine("Sending course data as JSON (no file)");

                    // Convert CreateCourseRequestDto to CreateCourseDto for JSON serialization
                    var createCourseDto = new
                    {
                        Title = dto.Title,
                        Description = dto.Description,
                        Price = dto.Price,
                        Order = dto.Order,
                        SpecializationId = dto.SpecializationId
                        // No ThumbnailUrl needed
                    };

                    var response = await _httpClient.PostAsJsonAsync("api/courses", createCourseDto, _jsonOptions);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<CourseDto>(_jsonOptions);
                        Console.WriteLine("Course created successfully without file");
                        return result;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to create course without file: {response.StatusCode}, Content: {errorContent}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"CreateCourseAsync", ex);
                return null;
            }
        }

        public async Task<bool> UpdateCourseAsync(
            int id, UpdateCourseRequestDto dto, Stream? thumbnailStream, string fileName)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Updating course {id} with file: {fileName != null}");

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

                    if (dto.Order.HasValue)
                        content.Add(new StringContent(dto.Order.Value.ToString()), "Order");

                    // ALWAYS send ThumbnailUrl (even if empty)
                    content.Add(new StringContent(dto.ThumbnailUrl ?? ""), "ThumbnailUrl");

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

                    var response = await _httpClient.PutAsync($"api/courses/{id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Course {id} updated successfully with file");
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to update course with file: {response.StatusCode}, Content: {errorContent}");
                        return false;
                    }
                }
                else
                {
                    // No file, send as JSON
                    Console.WriteLine("Sending course update data as JSON (no file)");

                    // Convert UpdateCourseRequestDto to UpdateCourseDto for JSON serialization
                    var updateCourseDto = new UpdateCourseDto
                    {
                        Title = dto.Title,
                        Description = dto.Description,
                        Price = dto.Price,
                        Order = dto.Order,
                        ThumbnailUrl = dto.ThumbnailUrl ?? "", // Ensure non-null
                        ThumbnailImage = null // No file
                    };

                    var response = await _httpClient.PutAsJsonAsync($"api/courses/{id}", updateCourseDto, _jsonOptions);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Course {id} updated successfully without file");
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to update course without file: {response.StatusCode}, Content: {errorContent}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"UpdateCourseAsync for id {id}", ex);
                return false;
            }
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            return await DeleteAsync($"api/courses/{id}");
        }
    }
}
