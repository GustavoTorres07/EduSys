using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class MateriaService : IMateriaService
    {
        private readonly HttpClient _http;
        public MateriaService(HttpClient http) { _http = http; }

        public async Task<List<MateriaDTO>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<MateriaDTO>>("api/materias") ?? new List<MateriaDTO>();

        public async Task<MateriaDTO> GetByIdAsync(int id) =>
            await _http.GetFromJsonAsync<MateriaDTO>($"api/materias/{id}");

        public async Task<bool> CreateAsync(MateriaDTO dto) =>
            (await _http.PostAsJsonAsync("api/materias", dto)).IsSuccessStatusCode;

        public async Task<bool> UpdateAsync(MateriaDTO dto) =>
            (await _http.PutAsJsonAsync("api/materias", dto)).IsSuccessStatusCode;

        public async Task<bool> DeleteAsync(int id) =>
            (await _http.DeleteAsync($"api/materias/{id}")).IsSuccessStatusCode;
    }
}