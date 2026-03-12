// Services/Implementations/WithdrawalService.cs
using Medlearn.DTOs;
using Medlearn.Model;
using System.Net.Http.Json;

namespace Medlearn.Services.Implementations
{
    public interface IWithdrawalService
    {
        Task<DoctorBalanceDto?> GetDoctorBalanceAsync();
        Task<List<WithdrawalRequestDto>?> GetMyWithdrawalsAsync();
        Task<WithdrawalRequestDto?> CreateWithdrawalAsync(CreateWithdrawalRequestDto dto);
        Task<bool> ProcessWithdrawalAsync(int id, ProcessWithdrawalDto dto);
        Task<List<WithdrawalRequestDto>?> GetAdminWithdrawalsAsync(
            WithdrawalStatus? status = null,
            string? search = null,
            int page = 1,
            int pageSize = 20);
    }

    public class WithdrawalService : BaseHttpService, IWithdrawalService
    {
        public WithdrawalService(HttpClient httpClient, ITokenService tokenService)
            : base(httpClient, tokenService) { }

        public async Task<DoctorBalanceDto?> GetDoctorBalanceAsync()
        {
            return await GetAsync<DoctorBalanceDto>("api/withdrawals/balance");
        }

        public async Task<List<WithdrawalRequestDto>?> GetMyWithdrawalsAsync()
        {
            return await GetAsync<List<WithdrawalRequestDto>>("api/withdrawals/my");
        }

        public async Task<WithdrawalRequestDto?> CreateWithdrawalAsync(CreateWithdrawalRequestDto dto)
        {
            return await PostAsync<WithdrawalRequestDto>("api/withdrawals", dto);
        }

        public async Task<bool> ProcessWithdrawalAsync(int id, ProcessWithdrawalDto dto)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync(
                $"api/withdrawals/{id}/status", dto, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<WithdrawalRequestDto>?> GetAdminWithdrawalsAsync(
            WithdrawalStatus? status = null,
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

            var endpoint = $"api/withdrawals/admin?{string.Join("&", queryParams)}";
            return await GetAsync<List<WithdrawalRequestDto>>(endpoint);
        }
    }
}
