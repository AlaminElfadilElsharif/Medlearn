// Services/Implementations/NotificationService.cs
using Medlearn.DTOs;
using Medlearn.Services;
using System.Net.Http.Json;

namespace Medlearn.Services.Implementations
{
    public interface INotificationService
    {
        Task<List<NotificationDto>?> GetMyNotificationsAsync(bool? unreadOnly = null, int page = 1, int pageSize = 20);
        Task<int> GetUnreadCountAsync();
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> MarkAllAsReadAsync();
        Task<bool> DeleteNotificationAsync(int id);
        Task<bool> ClearAllNotificationsAsync();
    }

    public class NotificationService : BaseHttpService, INotificationService
    {
        public NotificationService(HttpClient httpClient, ITokenService tokenService)
            : base(httpClient, tokenService) { }

        public async Task<List<NotificationDto>?> GetMyNotificationsAsync(
            bool? unreadOnly = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var queryParams = new List<string>();

                if (unreadOnly.HasValue)
                    queryParams.Add($"unreadOnly={unreadOnly.Value}");

                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");

                var endpoint = "api/notifications/my";
                if (queryParams.Any())
                    endpoint += $"?{string.Join("&", queryParams)}";

                return await GetAsync<List<NotificationDto>>(endpoint);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetMyNotificationsAsync", ex);
                return null;
            }
        }

        public async Task<int> GetUnreadCountAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/notifications/unread-count");
                if (response.IsSuccessStatusCode)
                {
                    var count = await response.Content.ReadFromJsonAsync<int>();
                    return count;
                }
                return 0;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetUnreadCountAsync", ex);
                return 0;
            }
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/notifications/{id}/read", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("MarkAsReadAsync", ex);
                return false;
            }
        }

        public async Task<bool> MarkAllAsReadAsync()
        {
            try
            {
                var response = await _httpClient.PutAsync("api/notifications/read-all", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("MarkAllAsReadAsync", ex);
                return false;
            }
        }

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            try
            {
                return await DeleteAsync($"api/notifications/{id}");
            }
            catch (Exception ex)
            {
                await LogErrorAsync("DeleteNotificationAsync", ex);
                return false;
            }
        }

        public async Task<bool> ClearAllNotificationsAsync()
        {
            try
            {
                return await DeleteAsync("api/notifications/clear-all");
            }
            catch (Exception ex)
            {
                await LogErrorAsync("ClearAllNotificationsAsync", ex);
                return false;
            }
        }
    }
}
