using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    // DashboardService.cs
    public class DashboardService : IDashboardService
    {
        private readonly HttpClient _http;
        public DashboardService(HttpClient http) { _http = http; }

        public async Task<DashboardDTO> GetResumenAsync()
        {
            return await _http.GetFromJsonAsync<DashboardDTO>("api/dashboard") ?? new DashboardDTO();
        }
    }
}
