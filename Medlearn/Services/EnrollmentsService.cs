// Services/Implementations/EnrollmentService.cs
using Medlearn.DTOs;
using Medlearn.Model;
using System.Net.Http.Json;

namespace Medlearn.Services.Implementations
{
    public interface IEnrollmentService
    {
        Task<List<EnrollmentDto>?> GetMyEnrollmentsAsync(EnrollmentType? type = null, bool? completed = null);
        Task<List<object>?> GetDoctorEnrollmentsAsync();
        Task<EnrollmentDto?> CreateEnrollmentAsync(CreateEnrollmentDto dto);
        Task<bool> UpdateProgressAsync(int enrollmentId, UpdateProgressDto dto);
        Task<bool> CompleteEnrollmentAsync(int enrollmentId);
        // Add to IEnrollmentService interface
        Task<List<UserDto>?> GetMyStudentsAsync();
        Task<List<ProgressTrackingDto>?> GetEnrollmentProgressAsync(int enrollmentId);
        Task<List<EnrollmentDto>?> GetDoctorContentEnrollmentsAsync();
        Task<List<EnrollmentDto>?> GetUserEnrollmentsAsync(int userId, EnrollmentType? type = null, bool? completed = null);
        Task<List<ProgressTrackingDto>?> GetStudentProgressForDoctorAsync(int enrollmentId);
        Task<List<EnrollmentDto>?> GetStudentEnrollmentsForDoctorAsync(int studentId);


    }

    public class EnrollmentService : BaseHttpService, IEnrollmentService
    {
        public EnrollmentService(HttpClient httpClient, ITokenService tokenService) : base(httpClient, tokenService) { }

        public async Task<List<EnrollmentDto>?> GetMyEnrollmentsAsync(EnrollmentType? type = null, bool? completed = null)
        {
            var queryParams = new List<string>();

            if (type.HasValue)
                queryParams.Add($"type={(int)type.Value}");

            if (completed.HasValue)
                queryParams.Add($"completed={completed.Value}");

            var endpoint = "api/enrollments/my";
            if (queryParams.Any())
                endpoint += $"?{string.Join("&", queryParams)}";

            return await GetAsync<List<EnrollmentDto>>(endpoint);
        }

        public async Task<List<UserDto>?> GetMyStudentsAsync()
        {
            try
            {
                // Get enrollments for doctor's content
                var enrollments = await GetDoctorContentEnrollmentsAsync();

                if (enrollments == null || !enrollments.Any())
                    return new List<UserDto>();

                // Extract unique students from enrollments
                var students = enrollments
                    .Where(e => e.User != null)
                    .Select(e => e.User)
                    .DistinctBy(u => u.Id)
                    .ToList();

                return students;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting my students: {ex.Message}");
                return null;
            }
        }

        public async Task<List<EnrollmentDto>?> GetUserEnrollmentsAsync(int userId, EnrollmentType? type = null, bool? completed = null)
        {
            try
            {
                var queryParams = new List<string>
        {
            $"userId={userId}"
        };

                if (type.HasValue)
                    queryParams.Add($"type={(int)type.Value}");

                if (completed.HasValue)
                    queryParams.Add($"completed={completed.Value}");

                var endpoint = $"api/enrollments/user/{userId}";
                if (queryParams.Any())
                    endpoint += $"?{string.Join("&", queryParams)}";

                return await GetAsync<List<EnrollmentDto>>(endpoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user enrollments: {ex.Message}");
                return null;
            }
        }
        public async Task<List<object>?> GetDoctorEnrollmentsAsync()
        {
            return await GetAsync<List<object>>("api/enrollments/doctor/my");
        }
        public async Task<List<EnrollmentDto>?> GetDoctorContentEnrollmentsAsync()
        {
            return await GetAsync<List<EnrollmentDto>>("api/enrollments/doctor/content-enrollments");
        }
        public async Task<EnrollmentDto?> CreateEnrollmentAsync(CreateEnrollmentDto dto)
        {
            return await PostAsync<EnrollmentDto>("api/enrollments", dto);
        }

        public async Task<bool> UpdateProgressAsync(int enrollmentId, UpdateProgressDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/enrollments/{enrollmentId}/progress", dto, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CompleteEnrollmentAsync(int enrollmentId)
        {
            var response = await _httpClient.PutAsync(
                $"api/enrollments/{enrollmentId}/complete", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ProgressTrackingDto>?> GetEnrollmentProgressAsync(int enrollmentId)
        {
            return await GetAsync<List<ProgressTrackingDto>>($"api/enrollments/{enrollmentId}/progress");
        }
        public async Task<List<ProgressTrackingDto>?> GetStudentProgressForDoctorAsync(int enrollmentId)
        {
            return await GetAsync<List<ProgressTrackingDto>>($"api/enrollments/doctor/student-progress/{enrollmentId}");
        }

        public async Task<List<EnrollmentDto>?> GetStudentEnrollmentsForDoctorAsync(int studentId)
        {
            return await GetAsync<List<EnrollmentDto>>($"api/enrollments/doctor/student/{studentId}/enrollments");
        }
    }
}
