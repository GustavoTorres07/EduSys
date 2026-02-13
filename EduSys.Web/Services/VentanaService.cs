using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class VentanaService : IVentanaService
    {
        private readonly HttpClient _http;
        public VentanaService(HttpClient http) { _http = http; }

        public async Task<List<VentanaOperativaDTO>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<VentanaOperativaDTO>>("api/ventanas") ?? new List<VentanaOperativaDTO>();

        public async Task<bool> CreateAsync(VentanaOperativaDTO dto)
            => (await _http.PostAsJsonAsync("api/ventanas", dto)).IsSuccessStatusCode;

        public async Task<bool> DeleteAsync(int id)
            => (await _http.DeleteAsync($"api/ventanas/{id}")).IsSuccessStatusCode;
    }
}