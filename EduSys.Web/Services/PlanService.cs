using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class PlanService : IPlanService
    {
        private readonly HttpClient _http;

        public PlanService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<PlanEstudioDTO>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<PlanEstudioDTO>>("api/planes") ?? new List<PlanEstudioDTO>();
        }

        public async Task<PlanEstudioDTO> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<PlanEstudioDTO>($"api/planes/{id}");
        }

        public async Task<int> CreateAsync(PlanEstudioDTO plan)
        {
            var response = await _http.PostAsJsonAsync("api/planes", plan);
            if (response.IsSuccessStatusCode)
            {
                // Leemos el ID que devuelve el backend
                return await response.Content.ReadFromJsonAsync<int>();
            }
            return 0; // Error
        }

        public async Task<bool> UpdateAsync(PlanEstudioDTO plan)
        {
            var response = await _http.PutAsJsonAsync("api/planes", plan);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/planes/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<PlanMateriaDTO>> GetMateriasByPlanAsync(int idPlan)
        {
            // Llama al endpoint: GET api/planes/{id}/materias
            return await _http.GetFromJsonAsync<List<PlanMateriaDTO>>($"api/planes/{idPlan}/materias")
                   ?? new List<PlanMateriaDTO>();
        }

        public async Task<bool> AgregarMateriaAsync(PlanMateriaDTO dto)
        {
            // Llama al endpoint: POST api/planes/materias
            var response = await _http.PostAsJsonAsync("api/planes/materias", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> QuitarMateriaAsync(int idPlanMateria)
        {
            // Llama al endpoint: DELETE api/planes/materias/{id}
            var response = await _http.DeleteAsync($"api/planes/materias/{idPlanMateria}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ActualizarCorrelativasAsync(int idPlanMateria, List<CorrelativaItemDTO> correlativas)
        {
            var response = await _http.PutAsJsonAsync($"api/planes/materia/{idPlanMateria}/correlativas", correlativas);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EditarMateriaAsync(PlanMateriaDTO dto)
        {
            var response = await _http.PutAsJsonAsync("api/planes/materias", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<PlanMateriaDTO>> GetAllMateriasAsync()
        {
            // Nota: Asegúrate de tener este endpoint en tu PlanesController del Backend
            // Si no lo tienes, créalo: [HttpGet("materias/todas")]
            return await _http.GetFromJsonAsync<List<PlanMateriaDTO>>("api/planes/materias/todas") ?? new List<PlanMateriaDTO>();
        }

        public async Task<List<PlanMateriaDTO>> GetMateriasPorSedeAsync(int idCarrera, int idSede)
        {
            var resultado = await _http.GetFromJsonAsync<List<PlanMateriaDTO>>($"api/planes/materias/carrera/{idCarrera}/sede/{idSede}");
            return resultado ?? new List<PlanMateriaDTO>();
        }

    }
}