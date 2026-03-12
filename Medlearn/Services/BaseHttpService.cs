// Services/BaseHttpService.cs
using Medlearn.Services;
using Medlearn.Services.Implementations;
using System.Net.Http.Json;
using System.Text.Json;

namespace Medlearn.Services
{
    public class BaseHttpService
    {
        protected readonly HttpClient _httpClient;
        protected readonly ITokenService _tokenService;
        protected readonly JsonSerializerOptions _jsonOptions;

        public BaseHttpService(HttpClient httpClient, ITokenService tokenService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Helper method to ensure auth header is set
        protected async Task EnsureAuthHeaderAsync()
        {
            var token = await _tokenService.GetTokenAsync();
            Console.WriteLine($"Token from storage: {!string.IsNullOrEmpty(token)}");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine("Authorization header set");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                Console.WriteLine("No token found, authorization header cleared");
            }
        }

        // Safe error logging method
        protected async Task LogErrorAsync(string context, Exception ex)
        {
            try
            {
                Console.WriteLine($"Error in {context}: {ex.Message}");
            }
            catch
            {
                // Fallback to simple console write
                Console.WriteLine($"Error in {context}: {ex.Message}");
            }
        }

        // HTTP Methods with auth header
        protected async Task<T?> GetAsync<T>(string endpoint)
        {
            await EnsureAuthHeaderAsync();
            try
            {
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"GET {endpoint} failed: {response.StatusCode}");
                    return default;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"GET {endpoint}", ex);
                return default;
            }
        }

        protected async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            await EnsureAuthHeaderAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"POST {endpoint} failed: {response.StatusCode}");
                    return default;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"POST {endpoint}", ex);
                return default;
            }
        }

        protected async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            await EnsureAuthHeaderAsync();
            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"PUT {endpoint} failed: {response.StatusCode}");
                    return default;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"PUT {endpoint}", ex);
                return default;
            }
        }

        protected async Task<bool> DeleteAsync(string endpoint)
        {
            await EnsureAuthHeaderAsync();
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"DELETE {endpoint}", ex);
                return false;
            }
        }

        protected async Task<HttpResponseMessage> PostFormDataAsync(string endpoint, MultipartFormDataContent content)
        {
            await EnsureAuthHeaderAsync();
            return await _httpClient.PostAsync(endpoint, content);
        }
    }
}
