using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class ComisionService : IComisionService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ComisionService> _logger; // ✅ Inyectado para depuración

        public ComisionService(HttpClient http, ILogger<ComisionService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<ComisionDTO>> GetAllAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<ComisionDTO>>("api/comisiones") ?? new List<ComisionDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista general de comisiones.");
                return new List<ComisionDTO>();
            }
        }

        public async Task<List<ComisionDTO>> GetByPeriodoAsync(int idPeriodo)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<ComisionDTO>>($"api/comisiones/periodo/{idPeriodo}") ?? new List<ComisionDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comisiones del periodo {IdPeriodo}.", idPeriodo);
                return new List<ComisionDTO>();
            }
        }

        public async Task<ComisionDTO?> GetByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<ComisionDTO>($"api/comisiones/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el detalle de la comisión {Id}.", id);
                return null;
            }
        }

        public async Task<bool> CreateAsync(ComisionDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/comisiones", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al crear comisión: {Error}", error);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al intentar crear una comisión.");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(ComisionDTO dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/comisiones", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al actualizar comisión {Id}: {Error}", dto.Id, error);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al intentar actualizar la comisión {Id}.", dto.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/comisiones/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar eliminar la comisión {Id}.", id);
                return false;
            }
        }

        public async Task<List<ComisionDTO>> GetByPeriodoAndCarreraAsync(int idPeriodo, int idCarrera, int? idAlumno = null)
        {
            try
            {
                string url = $"api/comisiones/periodo/{idPeriodo}/carrera/{idCarrera}";

                if (idAlumno.HasValue)
                {
                    url += $"?idAlumno={idAlumno.Value}";
                }

                return await _http.GetFromJsonAsync<List<ComisionDTO>>(url) ?? new List<ComisionDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comisiones para periodo {IdPeriodo} y carrera {IdCarrera}.", idPeriodo, idCarrera);
                return new List<ComisionDTO>();
            }
        }

        public async Task<List<ComisionDTO>> GetComisionesPorSedeAsync(int idSede)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<ComisionDTO>>($"api/comisiones/sede/{idSede}") ?? new List<ComisionDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comisiones para la sede {IdSede}.", idSede);
                return new List<ComisionDTO>();
            }
        }

        // ==========================================
        // MÉTODOS DE GESTIÓN DOCENTE
        // ==========================================

        public async Task<List<DocenteComisionListadoDTO>> GetDocentesPorComisionAsync(int idComision)
        {
            try
            {
                var response = await _http.GetAsync($"api/comisiones/{idComision}/docentes");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<DocenteComisionListadoDTO>>() ?? new List<DocenteComisionListadoDTO>();
                }
                return new List<DocenteComisionListadoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener docentes asignados a la comisión {IdComision}.", idComision);
                return new List<DocenteComisionListadoDTO>();
            }
        }

        public async Task<ResultadoOperacionDTO> AsignarDocenteAsync(DocenteComisionRequestDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/comisiones/asignar-docente", dto);
                var resultado = await response.Content.ReadFromJsonAsync<ResultadoOperacionDTO>();

                return resultado ?? new ResultadoOperacionDTO { Exito = false, Mensaje = "Error: Respuesta vacía del servidor." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar docente a la comisión {IdComision}.", dto.IdComision);
                return new ResultadoOperacionDTO { Exito = false, Mensaje = "Error de conexión con el servidor." };
            }
        }

        public async Task<bool> DesasignarDocenteAsync(int idAsignacion)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/comisiones/docentes/{idAsignacion}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar desasignar al docente (Asignación ID: {IdAsignacion}).", idAsignacion);
                return false;
            }
        }
    }
}