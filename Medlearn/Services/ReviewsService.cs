// Services/Implementations/ReviewService.cs
using Medlearn.DTOs;
using Medlearn.Model;
using System.Net.Http.Json;

namespace Medlearn.Services.Implementations
{
    public interface IReviewService
    {
        Task<object?> GetLectureReviewsAsync(int lectureId, int page = 1, int pageSize = 10);
        Task<object?> GetCourseReviewsAsync(int courseId, int page = 1, int pageSize = 10);
        Task<object?> GetSpecializationReviewsAsync(int specializationId, int page = 1, int pageSize = 10);
        Task<ReviewDto?> GetReviewByEnrollmentAsync(int enrollmentId);
        Task<ReviewDto?> CreateReviewAsync(CreateReviewDto dto);
        Task<bool> UpdateReviewAsync(int id, UpdateReviewDto dto);
        Task<bool> DeleteReviewAsync(int id);
        Task<List<ReviewDto>?> GetUserReviewsAsync(int userId);

    }

    public class ReviewService : BaseHttpService, IReviewService
    {
        public ReviewService(HttpClient httpClient, ITokenService tokenService) : base(httpClient, tokenService) { }

        public async Task<object?> GetLectureReviewsAsync(int lectureId, int page = 1, int pageSize = 10)
        {
            var endpoint = $"api/reviews/lecture/{lectureId}?page={page}&pageSize={pageSize}";
            return await GetAsync<object>(endpoint);
        }

        public async Task<object?> GetCourseReviewsAsync(int courseId, int page = 1, int pageSize = 10)
        {
            var endpoint = $"api/reviews/course/{courseId}?page={page}&pageSize={pageSize}";
            return await GetAsync<object>(endpoint);
        }
        // Services/Implementations/ReviewService.cs

        // Add to the IReviewService interface:

        // Add to the ReviewService class:
        public async Task<List<ReviewDto>?> GetUserReviewsAsync(int userId)
        {
            try
            {
                return await GetAsync<List<ReviewDto>>($"api/reviews/user/{userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user reviews: {ex.Message}");
                return null;
            }
        }
        public async Task<object?> GetSpecializationReviewsAsync(int specializationId, int page = 1, int pageSize = 10)
        {
            var endpoint = $"api/reviews/specialization/{specializationId}?page={page}&pageSize={pageSize}";
            return await GetAsync<object>(endpoint);
        }

        public async Task<ReviewDto?> GetReviewByEnrollmentAsync(int enrollmentId)
        {
            return await GetAsync<ReviewDto>($"api/reviews/enrollment/{enrollmentId}");
        }

        public async Task<ReviewDto?> CreateReviewAsync(CreateReviewDto dto)
        {
            return await PostAsync<ReviewDto>("api/reviews", dto);
        }

        public async Task<bool> UpdateReviewAsync(int id, UpdateReviewDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/reviews/{id}", dto, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            return await DeleteAsync($"api/reviews/{id}");
        }
    }
}
