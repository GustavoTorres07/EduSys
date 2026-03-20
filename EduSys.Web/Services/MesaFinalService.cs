using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class MesaFinalService : IMesaFinalService
    {
        private readonly HttpClient _http;
        private readonly ILogger<MesaFinalService> _logger; // ✅ Agregado para trazabilidad

        public MesaFinalService(HttpClient http, ILogger<MesaFinalService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<MesaFinalDTO>> GetAllAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<MesaFinalDTO>>("api/mesasfinales")
                       ?? new List<MesaFinalDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de red al intentar obtener todas las mesas de finales.");
                return new List<MesaFinalDTO>();
            }
        }

        public async Task<List<MesaFinalDTO>> GetByPeriodoAsync(int idPeriodo)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<MesaFinalDTO>>($"api/mesasfinales/periodo/{idPeriodo}")
                       ?? new List<MesaFinalDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de red al intentar obtener las mesas del periodo {IdPeriodo}.", idPeriodo);
                return new List<MesaFinalDTO>();
            }
        }

        public async Task<ResultadoOperacionDTO> CreateAsync(MesaFinalRequestDTO dto)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/mesasfinales", dto);

                // ✅ Leemos el DTO solo si sabemos que el backend devolvió un JSON válido (Éxito o validación controlada)
                if (res.IsSuccessStatusCode || res.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return await res.Content.ReadFromJsonAsync<ResultadoOperacionDTO>()
                           ?? new ResultadoOperacionDTO { Exito = false, Mensaje = "Respuesta vacía del servidor." };
                }

                _logger.LogWarning("Fallo en el servidor (HTTP {StatusCode}) al intentar crear una mesa final.", res.StatusCode);
                return new ResultadoOperacionDTO { Exito = false, Mensaje = $"Error del servidor: {res.StatusCode}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción crítica al intentar crear la mesa final.");
                return new ResultadoOperacionDTO { Exito = false, Mensaje = "Error de conexión con el servidor. Reintente en unos momentos." };
            }
        }

        public async Task<ResultadoOperacionDTO> UpdateAsync(MesaFinalRequestDTO dto)
        {
            try
            {
                var res = await _http.PutAsJsonAsync("api/mesasfinales", dto);

                if (res.IsSuccessStatusCode || res.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return await res.Content.ReadFromJsonAsync<ResultadoOperacionDTO>()
                           ?? new ResultadoOperacionDTO { Exito = false, Mensaje = "Respuesta vacía del servidor." };
                }

                _logger.LogWarning("Fallo en el servidor (HTTP {StatusCode}) al intentar actualizar la mesa final.", res.StatusCode);
                return new ResultadoOperacionDTO { Exito = false, Mensaje = $"Error del servidor: {res.StatusCode}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción crítica al intentar actualizar la mesa final.");
                return new ResultadoOperacionDTO { Exito = false, Mensaje = "Error de conexión con el servidor. Reintente en unos momentos." };
            }
        }

        public async Task<ResultadoOperacionDTO> DeleteAsync(int id)
        {
            try
            {
                var res = await _http.DeleteAsync($"api/mesasfinales/{id}");

                if (res.IsSuccessStatusCode || res.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return await res.Content.ReadFromJsonAsync<ResultadoOperacionDTO>()
                           ?? new ResultadoOperacionDTO { Exito = false, Mensaje = "Respuesta vacía del servidor." };
                }

                _logger.LogWarning("Fallo en el servidor (HTTP {StatusCode}) al intentar eliminar la mesa final {Id}.", res.StatusCode, id);
                return new ResultadoOperacionDTO { Exito = false, Mensaje = $"Error del servidor: {res.StatusCode}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción crítica al intentar eliminar la mesa final {Id}.", id);
                return new ResultadoOperacionDTO { Exito = false, Mensaje = "Error de conexión con el servidor. Reintente en unos momentos." };
            }
        }
    }
}