using Medlearn.DTOs;
using System.Net.Http.Json;

namespace Medlearn.Services.Implementations
{
    public interface ISearchService
    {
        Task<SearchResultsDto?> SearchAsync(string query, string type = "all", int page = 1, int pageSize = 10);
        Task<QuickSearchResultsDto?> QuickSearchAsync(string query, int limit = 5);
    }

    public class SearchService : BaseHttpService, ISearchService
    {
        public SearchService(HttpClient httpClient, ITokenService tokenService)
            : base(httpClient, tokenService) { }

        public async Task<SearchResultsDto?> SearchAsync(string query, string type = "all", int page = 1, int pageSize = 10)
        {
            var endpoint = $"api/search?q={Uri.EscapeDataString(query)}&type={type}&page={page}&pageSize={pageSize}";
            return await GetAsync<SearchResultsDto>(endpoint);
        }

        public async Task<QuickSearchResultsDto?> QuickSearchAsync(string query, int limit = 5)
        {
            var endpoint = $"api/search/quick?q={Uri.EscapeDataString(query)}&limit={limit}";
            return await GetAsync<QuickSearchResultsDto>(endpoint);
        }
    }
}
