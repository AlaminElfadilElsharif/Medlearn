// Services/Implementations/ContactService.cs
using Medlearn.DTOs;
using Medlearn.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace Medlearn.Services.Implementations
{
    public interface IContactService
    {
        Task<bool> SendContactMessageAsync(ContactMessageDto message);
        Task<List<ContactMessageDto>?> GetContactMessagesAsync(
                    int page = 1,
                    int pageSize = 20,
                    bool? unreadOnly = null,
                    string? search = null);
        Task<ContactMessageDto?> GetContactMessageAsync(int id);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> ReplyToContactAsync(int id, string reply);
        Task<bool> DeleteContactMessageAsync(int id);
        Task<int> GetUnreadCountAsync();
    }

    public class ContactService : BaseHttpService, IContactService
    {
        public ContactService(HttpClient httpClient, ITokenService tokenService)
            : base(httpClient, tokenService) { }

        // Public endpoint - no authentication required
        public async Task<bool> SendContactMessageAsync(ContactMessageDto message)
        {
            try
            {
                Console.WriteLine($"Sending contact message from: {message.Name}");

                // Don't call EnsureAuthHeaderAsync for public endpoint
                var response = await _httpClient.PostAsJsonAsync("api/contact", message, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Contact message sent successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to send contact message: {response.StatusCode}, Content: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("SendContactMessageAsync", ex);
                return false;
            }
        }

        // Admin endpoints - require authentication
        // Services/Implementations/ContactService.cs
        public async Task<List<ContactMessageDto>?> GetContactMessagesAsync(
            int page = 1,
            int pageSize = 20,
            bool? unreadOnly = null,
            string? search = null)
        {
            try
            {
                var queryParams = new List<string>();
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");

                if (unreadOnly.HasValue)
                    queryParams.Add($"unreadOnly={unreadOnly.Value}");

                if (!string.IsNullOrEmpty(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");

                var endpoint = $"api/contact/admin?{string.Join("&", queryParams)}";

                // Get response with headers
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    // Read total count from headers
                    if (response.Headers.TryGetValues("X-Total-Count", out var values))
                    {
                        // You'll need to expose this to the component
                        // For now, we'll just return the data
                    }

                    return await response.Content.ReadFromJsonAsync<List<ContactMessageDto>>(_jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetContactMessagesAsync", ex);
                return null;
            }
        }

        public async Task<ContactMessageDto?> GetContactMessageAsync(int id)
        {
            try
            {
                return await GetAsync<ContactMessageDto>($"api/contact/{id}");
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetContactMessageAsync", ex);
                return null;
            }
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/contact/{id}/read", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("MarkAsReadAsync", ex);
                return false;
            }
        }

        public async Task<bool> ReplyToContactAsync(int id, string reply)
        {
            try
            {
                var replyDto = new { Reply = reply };
                var response = await _httpClient.PostAsJsonAsync($"api/contact/{id}/reply", replyDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("ReplyToContactAsync", ex);
                return false;
            }
        }

        public async Task<bool> DeleteContactMessageAsync(int id)
        {
            try
            {
                return await DeleteAsync($"api/contact/{id}");
            }
            catch (Exception ex)
            {
                await LogErrorAsync("DeleteContactMessageAsync", ex);
                return false;
            }
        }

        public async Task<int> GetUnreadCountAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/contact/unread-count");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<int>();
                }
                return 0;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetUnreadCountAsync", ex);
                return 0;
            }
        }
    }
}