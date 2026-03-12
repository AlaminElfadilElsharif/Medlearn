// Services/Implementations/DashboardService.cs
namespace Medlearn.Services.Implementations
{
    public interface IDashboardService
    {
        Task<object?> GetAdminDashboardStatsAsync();
        Task<object?> GetDoctorDashboardStatsAsync();
        Task<object?> GetUserDashboardStatsAsync();
    }

    public class DashboardService : BaseHttpService, IDashboardService
    {
        public DashboardService(HttpClient httpClient, ITokenService tokenService) : base(httpClient, tokenService) { }

        public async Task<object?> GetAdminDashboardStatsAsync()
        {
            return await GetAsync<object>("api/dashboard/admin");
        }

        public async Task<object?> GetDoctorDashboardStatsAsync()
        {
            return await GetAsync<object>("api/dashboard/doctor");
        }

        public async Task<object?> GetUserDashboardStatsAsync()
        {
            return await GetAsync<object>("api/dashboard/user");
        }
    }
}
