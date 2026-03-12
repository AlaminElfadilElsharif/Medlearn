// Services/Implementations/UserService.cs
using Medlearn.DTOs;
using Medlearn.Model;
using Medlearn.Services;
using System.Net.Http.Json;

namespace Medlearn.Services.Implementations
{
    public interface IUserService
    {
        Task<List<UserDto>?> GetUsersAsync(string? search = null, UserType? userType = null, bool? isActive = null, int page = 1, int pageSize = 20);
        Task<UserDto?> GetUserAsync(int id);
        Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> UpdateProfileAsync(int id, UpdateProfileDto updateProfileDto);
        Task<bool> ToggleUserStatusAsync(int id, bool isActive);
        Task<UserDashboardDto?> GetUserDashboardAsync();
        Task<DoctorDashboardDto?> GetDoctorDashboardAsync();
        Task<DoctorBalanceDto?> GetDoctorBalanceAsync();
        Task<UserDto?> GetStudentByIdAsync(int studentId);
        // Add to IUserService interface
        Task<List<UserDto>?> GetDoctorsAsync(string? search = null, int page = 1, int pageSize = 20);
        Task<List<UserDto>?> GetStudentsAsync(string? search = null, int page = 1, int pageSize = 20);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<bool> DeleteUserAsync(int id);
        // Add to IUserService interface in UserService.cs
        Task<bool> ResetPasswordAsAdminAsync(int userId, string newPassword);
        Task<int> GetUsersCountAsync(UserType? userType = null, bool? isActive = null);
        Task<UserDto?> GetStudentForDoctorAsync(int studentId);

    }

    public class UserService : BaseHttpService, IUserService
    {
        public UserService(HttpClient httpClient, ITokenService tokenService)
            : base(httpClient, tokenService)
        {
        }
        // In IUserService interface
        public async Task<List<UserDto>?> GetDoctorsAsync(string? search = null, int page = 1, int pageSize = 20)
        {
            return await GetUsersAsync(search, UserType.Doctor, true, page, pageSize);
        }

        public async Task<List<UserDto>?> GetStudentsAsync(string? search = null, int page = 1, int pageSize = 20)
        {
            return await GetUsersAsync(search, UserType.User, true, page, pageSize);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            return await GetUserAsync(id);
        }
        // In UserService class
        public async Task<UserDto?> GetStudentForDoctorAsync(int studentId)
        {
            return await GetAsync<UserDto>($"api/users/student/{studentId}");
        }
        public async Task<List<UserDto>?> GetUsersAsync(string? search = null, UserType? userType = null, bool? isActive = null, int page = 1, int pageSize = 20)
        {
            // Build query string
            var queryString = $"?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrEmpty(search))
                queryString += $"&search={Uri.EscapeDataString(search)}";

            if (userType.HasValue)
                queryString += $"&userType={(int)userType.Value}";

            if (isActive.HasValue)
                queryString += $"&isActive={isActive.Value}";

            return await GetAsync<List<UserDto>>($"api/users{queryString}");
        }

        public async Task<UserDto?> GetUserAsync(int id)
        {
            return await GetAsync<UserDto>($"api/users/{id}");
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", updateUserDto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateProfileAsync(int id, UpdateProfileDto updateProfileDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/users/{id}/profile", updateProfileDto);
            return response.IsSuccessStatusCode;
        }
        // Add to UserService class
        public async Task<bool> ResetPasswordAsAdminAsync(int userId, string newPassword)
        {
            // Create the DTO with both properties
            var resetDto = new AdminResetPasswordDto
            {
                NewPassword = newPassword,
                ConfirmPassword = newPassword  // Send the same password for confirmation
            };

            var response = await _httpClient.PutAsJsonAsync($"api/users/{userId}/admin-reset-password", resetDto);
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> ToggleUserStatusAsync(int id, bool isActive)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/users/{id}/activate", new { isActive });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await DeleteAsync($"api/users/{id}");
        }

        public async Task<UserDashboardDto?> GetUserDashboardAsync()
        {
            return await GetAsync<UserDashboardDto>("api/dashboard/user");
        }

        public async Task<DoctorDashboardDto?> GetDoctorDashboardAsync()
        {
            return await GetAsync<DoctorDashboardDto>("api/dashboard/doctor");
        }

        public async Task<DoctorBalanceDto?> GetDoctorBalanceAsync()
        {
            return await GetAsync<DoctorBalanceDto>("api/users/balance");
        }

        // In IUserService.cs

        // In UserService.cs
        public async Task<UserDto?> GetStudentByIdAsync(int studentId)
        {
            try
            {
                return await GetAsync<UserDto>($"api/users/students/{studentId}");
            }
            catch (Exception ex)
            {
                await LogErrorAsync("GetStudentByIdAsync", ex);
                return null;
            }
        }

        public async Task<int> GetUsersCountAsync(UserType? userType = null, bool? isActive = null)
        {
            // You might need to create a specific endpoint for counts
            // For now, we'll use the existing endpoint and parse headers
            var queryString = "?page=1&pageSize=1"; // Minimal data

            if (userType.HasValue)
                queryString += $"&userType={(int)userType.Value}";

            if (isActive.HasValue)
                queryString += $"&isActive={isActive.Value}";

            try
            {
                var response = await _httpClient.GetAsync($"api/users{queryString}");
                if (response.Headers.TryGetValues("X-Total-Count", out var values))
                {
                    if (int.TryParse(values.FirstOrDefault(), out int count))
                    {
                        return count;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user count: {ex.Message}");
            }

            return 0;
        }
    }
}
