using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class PlanService : IPlanService
    {
        private readonly HttpClient _http;
        private readonly ILogger<PlanService> _logger; // ✅ Agregado para trazabilidad

        public PlanService(HttpClient http, ILogger<PlanService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<PlanEstudioDTO>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<PlanEstudioDTO>>("api/planes");
                return response ?? new List<PlanEstudioDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista general de planes de estudio.");
                return new List<PlanEstudioDTO>();
            }
        }

        public async Task<PlanEstudioDTO?> GetByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<PlanEstudioDTO>($"api/planes/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el detalle del plan de estudio con ID {Id}.", id);
                return null;
            }
        }

        public async Task<int> CreateAsync(PlanEstudioDTO plan)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/planes", plan);

                if (response.IsSuccessStatusCode)
                {
                    // Leemos el ID que devuelve el backend
                    return await response.Content.ReadFromJsonAsync<int>();
                }

                var errorMsg = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Fallo al crear el plan de estudio '{Nombre}': {ErrorMsg}", plan.Nombre, errorMsg);
                return 0; // 0 indicará error a la vista
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar crear un plan de estudio.");
                return 0;
            }
        }

        public async Task<bool> UpdateAsync(PlanEstudioDTO plan)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/planes", plan);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al actualizar el plan de estudio {Id}: {ErrorMsg}", plan.Id, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar actualizar el plan de estudio {Id}.", plan.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/planes/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de red al intentar eliminar el plan de estudio {Id}.", id);
                return false;
            }
        }

        // ==========================================
        // GESTIÓN DE MATERIAS Y CORRELATIVAS
        // ==========================================

        public async Task<List<PlanMateriaDTO>> GetMateriasByPlanAsync(int idPlan)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<PlanMateriaDTO>>($"api/planes/{idPlan}/materias")
                       ?? new List<PlanMateriaDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las materias del plan {IdPlan}.", idPlan);
                return new List<PlanMateriaDTO>();
            }
        }

        public async Task<bool> AgregarMateriaAsync(PlanMateriaDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/planes/materias", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al agregar materia al plan: {ErrorMsg}", errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al intentar agregar una materia al plan.");
                return false;
            }
        }

        public async Task<bool> EditarMateriaAsync(PlanMateriaDTO dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/planes/materias", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al intentar editar una materia del plan.");
                return false;
            }
        }

        public async Task<bool> QuitarMateriaAsync(int idPlanMateria)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/planes/materias/{idPlanMateria}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar quitar la materia {IdPlanMateria} del plan.", idPlanMateria);
                return false;
            }
        }

        public async Task<bool> ActualizarCorrelativasAsync(int idPlanMateria, List<CorrelativaItemDTO> correlativas)
        {
            try
            {
                // ⬇️ Asegúrate de que diga 'materias' en plural aquí
                var response = await _http.PutAsJsonAsync($"api/planes/materias/{idPlanMateria}/correlativas", correlativas);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al actualizar correlativas de la materia {IdPlanMateria}: {ErrorMsg}", idPlanMateria, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico de conexión al intentar actualizar correlativas de la materia {IdPlanMateria}.", idPlanMateria);
                return false;
            }
        }

        // ==========================================
        // CONSULTAS GLOBALES
        // ==========================================

        public async Task<List<PlanMateriaDTO>> GetAllMateriasAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<PlanMateriaDTO>>("api/planes/materias/todas")
                       ?? new List<PlanMateriaDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las materias de todos los planes.");
                return new List<PlanMateriaDTO>();
            }
        }

        public async Task<List<PlanMateriaDTO>> GetMateriasPorSedeAsync(int idCarrera, int idSede)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<PlanMateriaDTO>>($"api/planes/materias/carrera/{idCarrera}/sede/{idSede}")
                       ?? new List<PlanMateriaDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener materias para la carrera {IdCarrera} en la sede {IdSede}.", idCarrera, idSede);
                return new List<PlanMateriaDTO>();
            }
        }

        public async Task<List<PlanSedeDTO>> GetSedesByPlanAsync(int idPlan)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<PlanSedeDTO>>($"api/planes/{idPlan}/sedes")
                       ?? new List<PlanSedeDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sedes del plan {IdPlan}", idPlan);
                return new List<PlanSedeDTO>();
            }
        }

        public async Task<bool> ActualizarSedesAsync(int idPlan, List<int> idsSedes)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/planes/{idPlan}/sedes", idsSedes);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar sedes del plan {IdPlan}", idPlan);
                return false;
            }
        }
    }
}