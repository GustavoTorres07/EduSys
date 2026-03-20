using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class DocenteService : IDocenteService
    {
        private readonly HttpClient _http;
        private readonly ILogger<DocenteService> _logger; // ✅ Inyectamos trazabilidad

        public DocenteService(HttpClient http, ILogger<DocenteService> logger)
        {
            _http = http;
            _logger = logger;
        }

        // ==========================================
        // ABM ADMINISTRATIVO
        // ==========================================

        public async Task<List<DocenteListadoDTO>> GetDocentesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<DocenteListadoDTO>>("api/docentes");
                return response ?? new List<DocenteListadoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el listado general de docentes.");
                return new List<DocenteListadoDTO>();
            }
        }

        public async Task<DocenteRequestDTO?> GetDocenteByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<DocenteRequestDTO>($"api/docentes/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el detalle del docente {Id}.", id);
                return null;
            }
        }

        public async Task<bool> CrearDocenteAsync(DocenteRequestDTO docente)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/docentes", docente);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al crear docente (DNI: {Dni}): {ErrorMsg}", docente.Dni, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar crear un docente.");
                return false;
            }
        }

        public async Task<bool> EditarDocenteAsync(DocenteRequestDTO docente)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/docentes", docente);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al actualizar docente {Id}: {ErrorMsg}", docente.IdDocente, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar actualizar el docente {Id}.", docente.IdDocente);
                return false;
            }
        }

        public async Task<bool> EliminarDocenteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/docentes/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar dar de baja al docente {Id}.", id);
                return false;
            }
        }

        // ==========================================
        // PORTAL DEL DOCENTE (DASHBOARD)
        // ==========================================

        public async Task<List<ComisionDocenteDTO>> GetMisComisionesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<ComisionDocenteDTO>>("api/docentes/mis-comisiones");
                return response ?? new List<ComisionDocenteDTO>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // ✅ Capturamos específicamente si la sesión venció
                _logger.LogWarning("Acceso no autorizado (401) al buscar comisiones del docente. Posible token expirado.");
                return new List<ComisionDocenteDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las comisiones asignadas al docente.");
                return new List<ComisionDocenteDTO>();
            }
        }

        // En EduSys.Web.Services.DocenteService

        public async Task<DocenteRequestDTO?> GetMiPerfilAsync()
        {
            try
            {
                // Llamamos al endpoint sin parámetros, la magia la hace el Token JWT
                return await _http.GetFromJsonAsync<DocenteRequestDTO>("api/Docentes/mi-perfil");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil personal del docente.");
                return null;
            }
        }
    }
}