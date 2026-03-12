// Services/Implementations/PaymentService.cs
using Medlearn.DTOs;
using Medlearn.Model;
using System.Net.Http.Json;

namespace Medlearn.Services.Implementations
{
    public interface IPaymentService
    {
        // Fixed: Add optional parameters to match the implementation
        Task<List<PaymentRequestDto>?> GetPaymentRequestsAsync(
            PaymentStatus? status = null,
            string? search = null,
            int page = 1,
            int pageSize = 20);

        Task<List<PaymentRequestDto>?> GetMyPaymentRequestsAsync();
        Task<PaymentRequestDto?> CreatePaymentRequestAsync(
            CreatePaymentRequestDto dto,
            Stream? invoicePdfStream = null,
            string? fileName = null);
        Task<bool> UpdatePaymentStatusAsync(int id, UpdatePaymentRequestDto dto);
        Task<object?> GetPaymentStatsAsync();
        Task<List<PaymentRequestDto>?> GetUserPaymentRequestsAsync(int userId);

    }

    public class PaymentService : BaseHttpService, IPaymentService
    {
        public PaymentService(HttpClient httpClient, ITokenService tokenService) : base(httpClient, tokenService) { }

        public async Task<List<PaymentRequestDto>?> GetPaymentRequestsAsync(
            PaymentStatus? status = null,
            string? search = null,
            int page = 1,
            int pageSize = 20)
        {
            var queryParams = new List<string>();

            if (status.HasValue)
                queryParams.Add($"status={(int)status.Value}");

            if (!string.IsNullOrEmpty(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var endpoint = $"api/paymentrequests/admin?{string.Join("&", queryParams)}";
            return await GetAsync<List<PaymentRequestDto>>(endpoint);
        }

        // Services/Implementations/PaymentService.cs

        // Add to the IPaymentService interface:

        // Add to the PaymentService class:
        public async Task<List<PaymentRequestDto>?> GetUserPaymentRequestsAsync(int userId)
        {
            try
            {
                // For admin access to get other user's payments
                return await GetAsync<List<PaymentRequestDto>>($"api/paymentrequests/user/{userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user payment requests: {ex.Message}");
                return null;
            }
        }
        public async Task<List<PaymentRequestDto>?> GetMyPaymentRequestsAsync()
        {
            return await GetAsync<List<PaymentRequestDto>>("api/paymentrequests/my");
        }

        // Services/Implementations/PaymentService.cs
        public async Task<PaymentRequestDto?> CreatePaymentRequestAsync(
            CreatePaymentRequestDto dto,
            Stream? invoicePdfStream = null,
            string? fileName = null)
        {
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(((int)dto.EnrollmentType).ToString()), "EnrollmentType");

            if (dto.LectureId.HasValue)
                content.Add(new StringContent(dto.LectureId.Value.ToString()), "LectureId");

            if (dto.CourseId.HasValue)
                content.Add(new StringContent(dto.CourseId.Value.ToString()), "CourseId");

            if (dto.SpecializationId.HasValue)
                content.Add(new StringContent(dto.SpecializationId.Value.ToString()), "SpecializationId");

            // Always include the file - if not provided, create empty content
            if (invoicePdfStream != null && !string.IsNullOrEmpty(fileName))
            {
                var fileContent = new StreamContent(invoicePdfStream);
                content.Add(fileContent, "InvoicePdf", fileName);
            }
            else
            {
                // Add empty file content to satisfy the [FromForm] requirement
                var emptyBytes = new byte[0];
                var emptyContent = new ByteArrayContent(emptyBytes);
                content.Add(emptyContent, "InvoicePdf", "empty.pdf");
            }

            var response = await _httpClient.PostAsync("api/paymentrequests", content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PaymentRequestDto>(_jsonOptions);
            }
            else
            {
                // Log the error for debugging
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Payment request failed: {response.StatusCode} - {errorContent}");
                return null;
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int id, UpdatePaymentRequestDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/paymentrequests/{id}/status", dto, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<object?> GetPaymentStatsAsync()
        {
            return await GetAsync<object>("api/paymentrequests/stats");
        }
    }
}
