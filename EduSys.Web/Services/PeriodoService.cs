using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class PeriodoService : IPeriodoService
    {
        private readonly HttpClient _http;
        public PeriodoService(HttpClient http) { _http = http; }

        public async Task<List<PeriodoAcademicoDTO>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<PeriodoAcademicoDTO>>("api/periodos") ?? new List<PeriodoAcademicoDTO>();

        public async Task<PeriodoAcademicoDTO> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<PeriodoAcademicoDTO>($"api/periodos/{id}");

        public async Task<bool> CreateAsync(PeriodoAcademicoDTO dto)
            => (await _http.PostAsJsonAsync("api/periodos", dto)).IsSuccessStatusCode;

        public async Task<bool> UpdateAsync(PeriodoAcademicoDTO dto)
            => (await _http.PutAsJsonAsync("api/periodos", dto)).IsSuccessStatusCode;

        public async Task<bool> DeleteAsync(int id)
            => (await _http.DeleteAsync($"api/periodos/{id}")).IsSuccessStatusCode;
    }
}