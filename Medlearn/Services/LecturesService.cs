using Medlearn.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace Medlearn.Services.Implementations
{
    public interface ILectureService
    {
        Task<List<LectureDto>?> GetLecturesByCourseAsync(int courseId);
        Task<LectureDto?> GetLectureAsync(int id);
        Task<LectureDto?> CreateLectureAsync(CreateLectureDtos dto);
        Task<bool> UpdateLectureAsync(int id, UpdateLectureDtos dto);
        Task<bool> DeleteLectureAsync(int id);
        Task<bool> AddMaterialAsync(int lectureId, MaterialUploadDtos materialDto, Stream? fileStream, string? fileName);
        Task<bool> DeleteMaterialAsync(int materialId);
        Task<bool> UpdateLectureStatusAsync(int id, bool isActive);

    }

    public class LectureService : BaseHttpService, ILectureService
    {
        public LectureService(HttpClient httpClient, ITokenService tokenService) : base(httpClient, tokenService) { }

        public async Task<List<LectureDto>?> GetLecturesByCourseAsync(int courseId)
        {
            return await GetAsync<List<LectureDto>>($"api/lectures/course/{courseId}");
        }

        public async Task<LectureDto?> GetLectureAsync(int id)
        {
            return await GetAsync<LectureDto>($"api/lectures/{id}");
        }
        public async Task<bool> UpdateLectureStatusAsync(int id, bool isActive)
        {
            try
            {
                Console.WriteLine($"=== UpdateLectureStatusAsync START ===");
                Console.WriteLine($"Lecture ID: {id}, IsActive: {isActive}");

                // Check authentication before making the call
                var token = await _tokenService.GetTokenAsync();
                Console.WriteLine($"Token exists: {!string.IsNullOrEmpty(token)}");
                Console.WriteLine($"Token length: {token?.Length ?? 0}");

                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"ERROR: No token found!");
                    return false;
                }

                await EnsureAuthHeaderAsync();

                var endpoint = $"api/lectures/{id}/status";
                Console.WriteLine($"Endpoint: {endpoint}");

                // Log the full URL
                var baseAddress = _httpClient.BaseAddress;
                Console.WriteLine($"Base Address: {baseAddress}");
                Console.WriteLine($"Full URL: {baseAddress}{endpoint}");

                var statusDto = new { IsActive = isActive };

                Console.WriteLine($"Sending request...");
                var response = await _httpClient.PutAsJsonAsync(endpoint, statusDto, _jsonOptions);

                Console.WriteLine($"Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"SUCCESS: Lecture {id} status updated");
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine($"ERROR: Unauthorized (401) - Token may be invalid/expired");
                    return false;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"ERROR: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP EXCEPTION: {httpEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex.Message}");
                return false;
            }
        }
        public async Task<LectureDto?> CreateLectureAsync(CreateLectureDtos dto)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Creating lecture with {dto.Materials?.Count ?? 0} materials");

                if (dto.Materials != null && dto.Materials.Any(m => m.File != null))
                {
                    // Create multipart form data when files are present
                    var content = new MultipartFormDataContent();

                    // Add basic fields
                    content.Add(new StringContent(dto.Title), "Title");
                    content.Add(new StringContent(dto.Description), "Description");
                    content.Add(new StringContent(dto.Order.ToString()), "Order");
                    content.Add(new StringContent(dto.CourseId.ToString()), "CourseId");
                    content.Add(new StringContent(dto.Price.ToString()), "Price");
                    content.Add(new StringContent(dto.IsFreePreview.ToString()), "IsFreePreview");
                    content.Add(new StringContent(dto.EstimatedDurationMinutes.ToString()), "EstimatedDurationMinutes");

                    // Add materials - only files that are actually provided
                    if (dto.Materials != null)
                    {
                        for (int i = 0; i < dto.Materials.Count; i++)
                        {
                            var material = dto.Materials[i];

                            content.Add(new StringContent(material.Title), $"Materials[{i}].Title");
                            content.Add(new StringContent(material.Description ?? ""), $"Materials[{i}].Description");
                            content.Add(new StringContent(((int)material.Type).ToString()), $"Materials[{i}].Type");
                            content.Add(new StringContent(material.Order.ToString()), $"Materials[{i}].Order");
                            content.Add(new StringContent(material.IsDownloadable.ToString()), $"Materials[{i}].IsDownloadable");

                            // Only add file if it exists
                            if (material.File != null)
                            {
                                var fileStream = material.File.OpenReadStream(maxAllowedSize: 500 * 1024 * 1024);
                                var fileContent = new StreamContent(fileStream);

                                // Try to determine content type from file name
                                var contentType = GetContentType(material.File.Name);
                                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                                content.Add(fileContent, $"Materials[{i}].File", material.File.Name);
                            }
                        }
                    }

                    Console.WriteLine($"Sending lecture with {dto.Materials?.Count ?? 0} materials");
                    var response = await _httpClient.PostAsync("api/lectures", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<LectureDto>(_jsonOptions);
                        Console.WriteLine("Lecture created successfully with materials");
                        return result;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to create lecture: {response.StatusCode}, Content: {errorContent}");
                        return null;
                    }
                }
                else
                {
                    // No files, send as JSON
                    Console.WriteLine("Sending lecture without files (as JSON)");
                    var response = await _httpClient.PostAsJsonAsync("api/lectures", dto, _jsonOptions);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<LectureDto>(_jsonOptions);
                        Console.WriteLine("Lecture created successfully without files");
                        return result;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to create lecture: {response.StatusCode}, Content: {errorContent}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"CreateLectureAsync", ex);
                return null;
            }
        }

        public async Task<bool> UpdateLectureAsync(int id, UpdateLectureDtos dto)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Updating lecture {id}");

                var response = await _httpClient.PutAsJsonAsync($"api/lectures/{id}", dto, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Lecture {id} updated successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to update lecture {id}: {response.StatusCode}, Content: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"UpdateLectureAsync for id {id}", ex);
                return false;
            }
        }

        public async Task<bool> DeleteLectureAsync(int id)
        {
            return await DeleteAsync($"api/lectures/{id}");
        }

        public async Task<bool> AddMaterialAsync(int lectureId, MaterialUploadDtos materialDto, Stream? fileStream, string? fileName)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                Console.WriteLine($"Adding material to lecture {lectureId}, file: {fileName != null}");

                if (fileStream != null && !string.IsNullOrEmpty(fileName))
                {
                    // Create multipart form data for file upload
                    var content = new MultipartFormDataContent();

                    // Add form fields
                    content.Add(new StringContent(materialDto.Title), "Title");

                    if (!string.IsNullOrEmpty(materialDto.Description))
                        content.Add(new StringContent(materialDto.Description), "Description");

                    content.Add(new StringContent(((int)materialDto.Type).ToString()), "Type");
                    content.Add(new StringContent(materialDto.Order.ToString()), "Order");
                    content.Add(new StringContent(materialDto.IsDownloadable.ToString()), "IsDownloadable");

                    // Reset stream position to beginning
                    if (fileStream.CanSeek)
                        fileStream.Position = 0;

                    // Add file content
                    var fileContent = new StreamContent(fileStream);

                    // Determine content type from file name
                    string contentType = GetContentType(fileName);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    content.Add(fileContent, "File", fileName);

                    Console.WriteLine($"Sending multipart form data with file: {fileName}");

                    var response = await _httpClient.PostAsync($"api/lectures/{lectureId}/materials", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Material added to lecture {lectureId} successfully with file");
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to add material with file: {response.StatusCode}, Content: {errorContent}");
                        return false;
                    }
                }
                else
                {
                    // No file, send as JSON
                    Console.WriteLine("Sending material data as JSON (no file)");

                    var response = await _httpClient.PostAsJsonAsync($"api/lectures/{lectureId}/materials", materialDto, _jsonOptions);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Material added to lecture {lectureId} successfully without file");
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to add material without file: {response.StatusCode}, Content: {errorContent}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"AddMaterialAsync for lecture {lectureId}", ex);
                return false;
            }
        }

        public async Task<bool> DeleteMaterialAsync(int materialId)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/lectures/materials/{materialId}");
            return response.IsSuccessStatusCode;
        }

        private string GetContentType(string fileName)
        {
            // Map file extensions to MIME types
            return fileName.ToLowerInvariant() switch
            {
                string f when f.EndsWith(".pdf") => "application/pdf",
                string f when f.EndsWith(".doc") || f.EndsWith(".docx") => "application/msword",
                string f when f.EndsWith(".ppt") || f.EndsWith(".pptx") => "application/vnd.ms-powerpoint",
                string f when f.EndsWith(".xls") || f.EndsWith(".xlsx") => "application/vnd.ms-excel",
                string f when f.EndsWith(".txt") => "text/plain",
                string f when f.EndsWith(".zip") => "application/zip",
                string f when f.EndsWith(".mp4") => "video/mp4",
                string f when f.EndsWith(".mp3") => "audio/mpeg",
                string f when f.EndsWith(".jpg") || f.EndsWith(".jpeg") => "image/jpeg",
                string f when f.EndsWith(".png") => "image/png",
                string f when f.EndsWith(".gif") => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}
