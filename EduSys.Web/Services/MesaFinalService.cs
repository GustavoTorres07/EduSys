using System.Net.Http.Json;
using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;

namespace EduSys.Web.Services
{
    public class MesaFinalService : IMesaFinalService
    {
        private readonly HttpClient _http;
        public MesaFinalService(HttpClient http) => _http = http;

        public async Task<List<MesaFinalDTO>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<MesaFinalDTO>>("api/mesasfinales") ?? new List<MesaFinalDTO>();
        }

        public async Task<List<MesaFinalDTO>> GetByPeriodoAsync(int idPeriodo)
        {
            return await _http.GetFromJsonAsync<List<MesaFinalDTO>>($"api/mesasfinales/periodo/{idPeriodo}") ?? new List<MesaFinalDTO>();
        }

        public async Task<ResultadoOperacionDTO> CreateAsync(MesaFinalRequestDTO dto)
        {
            var res = await _http.PostAsJsonAsync("api/mesasfinales", dto);
            return await res.Content.ReadFromJsonAsync<ResultadoOperacionDTO>() ?? new ResultadoOperacionDTO { Exito = false, Mensaje = "Error" };
        }

        public async Task<ResultadoOperacionDTO> UpdateAsync(MesaFinalRequestDTO dto)
        {
            var res = await _http.PutAsJsonAsync("api/mesasfinales", dto);
            return await res.Content.ReadFromJsonAsync<ResultadoOperacionDTO>() ?? new ResultadoOperacionDTO { Exito = false, Mensaje = "Error" };
        }

        public async Task<ResultadoOperacionDTO> DeleteAsync(int id)
        {
            var res = await _http.DeleteAsync($"api/mesasfinales/{id}");
            return await res.Content.ReadFromJsonAsync<ResultadoOperacionDTO>() ?? new ResultadoOperacionDTO { Exito = false, Mensaje = "Error" };
        }
    }
}