using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class ModalidadService : IModalidadService
    {
        private readonly HttpClient _http;
        public ModalidadService(HttpClient http) { _http = http; }

        public async Task<List<ModalidadDTO>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<ModalidadDTO>>("api/modalidades") ?? new List<ModalidadDTO>();

        public async Task<ModalidadDTO> GetByIdAsync(int id) =>
            await _http.GetFromJsonAsync<ModalidadDTO>($"api/modalidades/{id}");

        public async Task<bool> CreateAsync(ModalidadDTO dto) =>
            (await _http.PostAsJsonAsync("api/modalidades", dto)).IsSuccessStatusCode;

        public async Task<bool> UpdateAsync(ModalidadDTO dto) =>
            (await _http.PutAsJsonAsync("api/modalidades", dto)).IsSuccessStatusCode;

        public async Task<bool> DeleteAsync(int id) =>
            (await _http.DeleteAsync($"api/modalidades/{id}")).IsSuccessStatusCode;
    }
}