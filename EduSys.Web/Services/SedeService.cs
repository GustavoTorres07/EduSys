using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class SedeService : ISedeService
    {
        private readonly HttpClient _http;
        public SedeService(HttpClient http) { _http = http; }

        public async Task<List<SedeDTO>> GetAllAsync() => await _http.GetFromJsonAsync<List<SedeDTO>>("api/sedes") ?? new List<SedeDTO>();
        public async Task<SedeDTO> GetByIdAsync(int id) => await _http.GetFromJsonAsync<SedeDTO>($"api/sedes/{id}");
        public async Task<bool> CreateAsync(SedeDTO dto) => (await _http.PostAsJsonAsync("api/sedes", dto)).IsSuccessStatusCode;
        public async Task<bool> UpdateAsync(SedeDTO dto) => (await _http.PutAsJsonAsync("api/sedes", dto)).IsSuccessStatusCode;
        public async Task<bool> DeleteAsync(int id) => (await _http.DeleteAsync($"api/sedes/{id}")).IsSuccessStatusCode;

        public async Task<List<AulaDTO>> GetAulasBySedeAsync(int idSede) => await _http.GetFromJsonAsync<List<AulaDTO>>($"api/sedes/{idSede}/aulas") ?? new List<AulaDTO>();
        public async Task<bool> CreateAulaAsync(AulaDTO dto) => (await _http.PostAsJsonAsync("api/sedes/aulas", dto)).IsSuccessStatusCode;
        public async Task<bool> UpdateAulaAsync(AulaDTO dto) => (await _http.PutAsJsonAsync("api/sedes/aulas", dto)).IsSuccessStatusCode;
        public async Task<bool> DeleteAulaAsync(int id) => (await _http.DeleteAsync($"api/sedes/aulas/{id}")).IsSuccessStatusCode;
    }
}