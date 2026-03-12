// Services/Implementations/AnnouncementService.cs
using Medlearn.DTOs;
using Medlearn.Services;
using System.Net.Http.Json;

namespace Medlearn.Services.Implementations
{
    public interface IAnnouncementService
    {
        Task<List<AnnouncementDto>?> GetAnnouncementsAsync(bool? important = null, bool? activeOnly = true);
        Task<List<AnnouncementDto>?> GetAllAnnouncementsAsync(bool? activeOnly = null, int page = 1, int pageSize = 20);
        Task<AnnouncementDto?> GetAnnouncementAsync(int id);
        Task<AnnouncementDto?> CreateAnnouncementAsync(CreateAnnouncementDto dto);
        Task<bool> UpdateAnnouncementAsync(int id, UpdateAnnouncementDto dto);
        Task<bool> DeleteAnnouncementAsync(int id);
    }

    public class AnnouncementService : BaseHttpService, IAnnouncementService
    {
        public AnnouncementService(HttpClient httpClient, ITokenService tokenService)
            : base(httpClient, tokenService) { }

        public async Task<List<AnnouncementDto>?> GetAnnouncementsAsync(bool? important = null, bool? activeOnly = true)
        {
            try
            {
                var queryParams = new List<string>();

                if (important.HasValue)
                    queryParams.Add($"important={important.Value}");

                if (activeOnly.HasValue)
                    queryParams.Add($"activeOnly={activeOnly.Value}");

                var endpoint = "api/announcements";
                if (queryParams.Any())
                    endpoint += $"?{string.Join("&", queryParams)}";

                return await GetAsync<List<AnnouncementDto>>(endpoint);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetAnnouncementsAsync", ex);
                return null;
            }
        }

        public async Task<List<AnnouncementDto>?> GetAllAnnouncementsAsync(
            bool? activeOnly = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var queryParams = new List<string>();

                if (activeOnly.HasValue)
                    queryParams.Add($"activeOnly={activeOnly.Value}");

                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");

                var endpoint = $"api/announcements/admin?{string.Join("&", queryParams)}";
                return await GetAsync<List<AnnouncementDto>>(endpoint);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetAllAnnouncementsAsync", ex);
                return null;
            }
        }

        public async Task<AnnouncementDto?> GetAnnouncementAsync(int id)
        {
            try
            {
                return await GetAsync<AnnouncementDto>($"api/announcements/{id}");
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetAnnouncementAsync", ex);
                return null;
            }
        }

        public async Task<AnnouncementDto?> CreateAnnouncementAsync(CreateAnnouncementDto dto)
        {
            try
            {
                return await PostAsync<AnnouncementDto>("api/announcements", dto);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("CreateAnnouncementAsync", ex);
                return null;
            }
        }

        public async Task<bool> UpdateAnnouncementAsync(int id, UpdateAnnouncementDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/announcements/{id}", dto, _jsonOptions);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("UpdateAnnouncementAsync", ex);
                return false;
            }
        }

        public async Task<bool> DeleteAnnouncementAsync(int id)
        {
            try
            {
                return await DeleteAsync($"api/announcements/{id}");
            }
            catch (Exception ex)
            {
                await LogErrorAsync("DeleteAnnouncementAsync", ex);
                return false;
            }
        }
    }
}
