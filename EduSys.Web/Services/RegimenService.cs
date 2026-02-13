using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class RegimenService : IRegimenService
    {
        private readonly HttpClient _http;
        public RegimenService(HttpClient http) { _http = http; }

        public async Task<List<RegimenDTO>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<RegimenDTO>>("api/regimenes") ?? new List<RegimenDTO>();

        public async Task<bool> CreateAsync(RegimenDTO dto) =>
            (await _http.PostAsJsonAsync("api/regimenes", dto)).IsSuccessStatusCode;

        public async Task<bool> UpdateAsync(RegimenDTO dto) =>
            (await _http.PutAsJsonAsync("api/regimenes", dto)).IsSuccessStatusCode;

        public async Task<bool> DeleteAsync(int id) =>
            (await _http.DeleteAsync($"api/regimenes/{id}")).IsSuccessStatusCode;
    }
}