// Services/Implementations/MessageService.cs
using Medlearn.DTOs;
using Medlearn.Model;
using Medlearn.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace Medlearn.Services.Implementations
{
    public interface IMessageService
    {
        // Inbox
        Task<List<MessageDto>?> GetInboxAsync(bool? unreadOnly = null, int page = 1, int pageSize = 20);
        Task<int> GetUnreadCountAsync();

        // Sent
        Task<List<MessageDto>?> GetSentMessagesAsync(int page = 1, int pageSize = 20);

        // Conversations
        Task<List<MessageDto>?> GetConversationAsync(int userId, int page = 1, int pageSize = 50);
        Task<List<UserDto>?> GetRecentContactsAsync();

        // Individual messages
        Task<MessageDto?> GetMessageAsync(int id);

        // Send operations
        Task<bool> SendMessageAsync(SendMessageDto message);
        Task<bool> ReplyToMessageAsync(int parentMessageId, string content);

        // Read operations
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> MarkAllAsReadAsync();

        // Delete
        Task<bool> DeleteMessageAsync(int id);

        // User search
        Task<List<UserDto>?> SearchUsersAsync(string searchTerm, UserType? userType = null);
    }

    public class MessageService : BaseHttpService, IMessageService
    {
        public MessageService(HttpClient httpClient, ITokenService tokenService)
            : base(httpClient, tokenService) { }

        public async Task<List<MessageDto>?> GetInboxAsync(
            bool? unreadOnly = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var queryParams = new List<string>();
                if (unreadOnly.HasValue)
                    queryParams.Add($"unreadOnly={unreadOnly.Value}");
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");

                var endpoint = "api/messages/inbox";
                if (queryParams.Any())
                    endpoint += $"?{string.Join("&", queryParams)}";

                return await GetAsync<List<MessageDto>>(endpoint);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetInboxAsync", ex);
                return null;
            }
        }

        public async Task<List<MessageDto>?> GetSentMessagesAsync(int page = 1, int pageSize = 20)
        {
            try
            {
                var endpoint = $"api/messages/sent?page={page}&pageSize={pageSize}";
                return await GetAsync<List<MessageDto>>(endpoint);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetSentMessagesAsync", ex);
                return null;
            }
        }

        public async Task<List<MessageDto>?> GetConversationAsync(int userId, int page = 1, int pageSize = 50)
        {
            try
            {
                var endpoint = $"api/messages/conversation/{userId}?page={page}&pageSize={pageSize}";
                return await GetAsync<List<MessageDto>>(endpoint);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetConversationAsync", ex);
                return null;
            }
        }

        public async Task<MessageDto?> GetMessageAsync(int id)
        {
            try
            {
                return await GetAsync<MessageDto>($"api/messages/{id}");
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetMessageAsync", ex);
                return null;
            }
        }

        public async Task<int> GetUnreadCountAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/messages/unread-count");
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

        public async Task<List<UserDto>?> GetRecentContactsAsync()
        {
            try
            {
                return await GetAsync<List<UserDto>>("api/messages/recent-contacts");
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetRecentContactsAsync", ex);
                return null;
            }
        }

        public async Task<bool> SendMessageAsync(SendMessageDto message)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/messages", message);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"403: {errorMessage}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("SendMessageAsync", ex);
                return false;
            }
        }

        public async Task<bool> ReplyToMessageAsync(int parentMessageId, string content)
        {
            try
            {
                var reply = new { Content = content };
                var response = await _httpClient.PostAsJsonAsync($"api/messages/{parentMessageId}/reply", reply);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"403: {errorMessage}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("ReplyToMessageAsync", ex);
                return false;
            }
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/messages/{id}/read", null);
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
                var response = await _httpClient.PutAsync("api/messages/read-all", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("MarkAllAsReadAsync", ex);
                return false;
            }
        }

        public async Task<bool> DeleteMessageAsync(int id)
        {
            try
            {
                return await DeleteAsync($"api/messages/{id}");
            }
            catch (Exception ex)
            {
                await LogErrorAsync("DeleteMessageAsync", ex);
                return false;
            }
        }

        // In MessageService.cs
        public async Task<List<UserDto>?> SearchUsersAsync(string searchTerm, UserType? userType = null)
        {
            try
            {
                var queryParams = new List<string> { $"searchTerm={Uri.EscapeDataString(searchTerm)}" };
                if (userType.HasValue)
                    queryParams.Add($"userType={(int)userType.Value}");

                var endpoint = $"api/messages/search-users?{string.Join("&", queryParams)}";
                return await GetAsync<List<UserDto>>(endpoint);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("SearchUsersAsync", ex);
                return null;
            }
        }
    }
}
