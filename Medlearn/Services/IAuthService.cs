// Services/Implementations/AuthService.cs
using Medlearn.DTOs;
using Medlearn.Services;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Medlearn.Services.Implementations
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginDto loginDto);
        Task<UserDto?> RegisterAsync(CreateUserDto createUserDto);
        Task<UserDto?> GetCurrentUserAsync();
        Task LogoutAsync();
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<string?> UploadProfileImageAsync(Stream fileStream, string fileName);
        Task InitializeAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string?> UploadProfileImageForUserAsync(int userId, Stream fileStream, string fileName);

    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly ILocalStorageService _localStorage;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string UserKey = "currentUser";

        public AuthService(HttpClient httpClient, ITokenService tokenService, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _localStorage = localStorage;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Initialize authentication on app start
        public async Task InitializeAsync()
        {
            var token = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }
        public async Task<string?> UploadProfileImageForUserAsync(int userId, Stream fileStream, string fileName)
        {
            try
            {
                var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Add(streamContent, "file", fileName);

                // Use the admin endpoint that accepts user ID as parameter
                var response = await _httpClient.PostAsync($"api/users/{userId}/upload-profile-image-admin", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading profile image for user: {ex.Message}");
                return null;
            }
        }
        // Login method
        public async Task<AuthResponse?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/users/login", loginDto, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);

                    if (authResponse != null && !string.IsNullOrEmpty(authResponse.Token))
                    {
                        // Store token using TokenService
                        await _tokenService.SetTokenAsync(authResponse.Token);
                        await _localStorage.SetItemAsync(UserKey, authResponse.User);

                        // Set authorization header
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token);
                    }

                    return authResponse;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return null;
            }
        }

        // Register method
        public async Task<UserDto?> RegisterAsync(CreateUserDto createUserDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/users/register", createUserDto, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Register error: {ex.Message}");
                return null;
            }
        }

        // Get current user
        // In AuthService.cs - Update GetCurrentUserAsync method
        public async Task<UserDto?> GetCurrentUserAsync()
        {
            try
            {
                // Check if we have a token first using TokenService
                var token = await _tokenService.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                    return null;

                // Always fetch from API to get latest data including balance
                var response = await _httpClient.GetAsync("api/users/me");
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);
                    if (user != null)
                    {
                        // Store in localStorage for faster access next time
                        await _localStorage.SetItemAsync(UserKey, user);
                    }
                    return user;
                }

                // Fallback to localStorage if API fails
                return await _localStorage.GetItemAsync<UserDto>(UserKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCurrentUser error: {ex.Message}");
                return null;
            }
        }
        // Logout method
        public async Task LogoutAsync()
        {
            try
            {
                // Clear token using TokenService
                await _tokenService.RemoveTokenAsync();
                await _localStorage.RemoveItemAsync(UserKey);

                // Clear authorization header
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");
            }
        }

        // Change password
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/users/{userId}/password", changePasswordDto, _jsonOptions);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChangePassword error: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> UploadProfileImageAsync(Stream fileStream, string fileName)
        {
            try
            {
                Console.WriteLine($"=== UploadProfileImageAsync called ===");
                Console.WriteLine($"FileName: {fileName}");
                Console.WriteLine($"FileStream length: {fileStream.Length}");
                Console.WriteLine($"FileStream type: {fileStream.GetType().Name}");
                Console.WriteLine($"FileStream CanSeek: {fileStream.CanSeek}");
                Console.WriteLine($"FileStream Position: {fileStream.Position}");

                var token = await _tokenService.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("ERROR: No authentication token found");
                    return null;
                }
                Console.WriteLine($"Token found: Length={token.Length}");

                // CRITICAL FIX: Copy BrowserFileStream to MemoryStream to avoid Blazor disposal issues
                MemoryStream memoryStream;

                if (fileStream.CanSeek)
                {
                    // Reset position if we can seek
                    fileStream.Position = 0;
                    Console.WriteLine("Reset file stream position to 0");

                    memoryStream = new MemoryStream();
                    await fileStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                }
                else
                {
                    // If we can't seek, we need to read the stream differently
                    Console.WriteLine("FileStream cannot seek, using alternative approach");

                    // Read all bytes into a buffer first
                    using var tempStream = new MemoryStream();
                    await fileStream.CopyToAsync(tempStream);
                    memoryStream = new MemoryStream(tempStream.ToArray());
                }

                Console.WriteLine($"MemoryStream created: Length={memoryStream.Length}, Position={memoryStream.Position}");

                // Create multipart form data
                using var form = new MultipartFormDataContent();

                // Use the MemoryStream instead of the original BrowserFileStream
                using var fileContent = new StreamContent(memoryStream);

                // Get content type from filename extension
                var contentType = GetContentType(fileName);
                Console.WriteLine($"ContentType: {contentType}");

                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = fileName
                };

                form.Add(fileContent, "file", fileName);

                var endpoint = "api/users/upload-profile-image";
                Console.WriteLine($"Sending request to: {endpoint}");

                // Set authorization header
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(endpoint, form);
                Console.WriteLine($"Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Upload successful. Response: {result}");

                    // Clean up the response
                    result = result?.Trim('"', ' ');
                    Console.WriteLine($"Cleaned result: {result}");

                    return result;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"ERROR - Upload failed: {response.StatusCode} - {error}");

                    // Try to get more details
                    if (response.Content.Headers.ContentType?.MediaType == "application/json")
                    {
                        try
                        {
                            var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                            Console.WriteLine($"Error details: {string.Join(", ", errorObj)}");
                        }
                        catch { }
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in UploadProfileImage: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Log inner exception if exists
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }

                return null;
            }
        }

        // Helper method to get content type from file extension
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }

        // Check if authenticated
        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                return await _tokenService.IsAuthenticatedAsync();
            }
            catch
            {
                return false;
            }
        }
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new UserDto();
    }
}

// Services/Interfaces/ILocalStorageService.cs

namespace Medlearn.Services.Implementations
{
    public interface ILocalStorageService
    {
        Task<T?> GetItemAsync<T>(string key);
        Task SetItemAsync<T>(string key, T value);
        Task RemoveItemAsync(string key);
        Task ClearAsync();
    }

    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<T?> GetItemAsync<T>(string key)
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

                if (string.IsNullOrEmpty(json))
                    return default;

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from localStorage: {ex.Message}");
                return default;
            }
        }

        public async Task SetItemAsync<T>(string key, T value)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to localStorage: {ex.Message}");
            }
        }

        public async Task RemoveItemAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }

        public async Task ClearAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
        }
    }
}
