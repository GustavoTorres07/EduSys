using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace EduSys.Web.Services
{
    public class DocenteService : IDocenteService
    {
        private readonly HttpClient _http;

        public DocenteService(HttpClient http)
        {
            _http = http;
        }

        // --- ABM Administrativo ---

        public async Task<List<DocenteListadoDTO>> GetDocentesAsync()
        {
            var response = await _http.GetAsync("api/docentes");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<DocenteListadoDTO>>() ?? new List<DocenteListadoDTO>();
            }
            return new List<DocenteListadoDTO>();
        }

        public async Task<DocenteRequestDTO?> GetDocenteByIdAsync(int id)
        {
            var response = await _http.GetAsync($"api/docentes/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<DocenteRequestDTO>();
            }
            return null;
        }

        public async Task<bool> CrearDocenteAsync(DocenteRequestDTO docente)
        {
            var response = await _http.PostAsJsonAsync("api/docentes", docente);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EditarDocenteAsync(DocenteRequestDTO docente)
        {
            var response = await _http.PutAsJsonAsync("api/docentes", docente);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarDocenteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/docentes/{id}");
            return response.IsSuccessStatusCode;
        }

        // --- ✅ NUEVO IMPLEMENTACIÓN: Dashboard Docente ---
        public async Task<List<ComisionDocenteDTO>> GetMisComisionesAsync()
        {
            // Llamamos al endpoint que creamos en DocentesController
            try
            {
                return await _http.GetFromJsonAsync<List<ComisionDocenteDTO>>("api/docentes/mis-comisiones")
                       ?? new List<ComisionDocenteDTO>();
            }
            catch
            {
                // Si falla o devuelve 404/401, retornamos lista vacía para no romper la UI
                return new List<ComisionDocenteDTO>();
            }
        }
    }
}