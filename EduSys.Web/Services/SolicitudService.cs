using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json; // ✅ Necesario para extraer mensajes de error

namespace EduSys.Web.Services
{
    public class SolicitudService : ISolicitudService
    {
        private readonly HttpClient _http;
        private readonly ILogger<SolicitudService> _logger; // ✅ Inyectado para trazabilidad

        public SolicitudService(HttpClient http, ILogger<SolicitudService> logger)
        {
            _http = http;
            _logger = logger;
        }

        // ==========================================
        // PORTAL PÚBLICO
        // ==========================================

        public async Task EnviarSolicitudAsync(SolicitudIngresoRequestDTO solicitud)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/solicitudes/solicitar", solicitud);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var mensajeLimpio = ExtraerMensajeError(errorContent);

                    _logger.LogWarning("Fallo al enviar solicitud de ingreso (DNI: {Dni}): {Mensaje}", solicitud.Dni, mensajeLimpio);

                    // 💡 Lanzamos ApplicationException para que el UI lo capture fácilmente
                    throw new ApplicationException(mensajeLimpio);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Fallo de conectividad al intentar enviar la solicitud de ingreso.");
                throw new ApplicationException("Error de conexión con el servidor. Por favor, reintente en unos minutos.");
            }
        }

        // ==========================================
        // GESTIÓN ADMINISTRATIVA
        // ==========================================

        public async Task<List<SolicitudIngresoDTO>> GetPendientesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<SolicitudIngresoDTO>>("api/solicitudes/pendientes");
                return response ?? new List<SolicitudIngresoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de solicitudes de ingreso pendientes.");
                return new List<SolicitudIngresoDTO>();
            }
        }

        public async Task<SolicitudIngresoDTO?> GetSolicitudByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<SolicitudIngresoDTO>($"api/solicitudes/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el detalle de la solicitud con ID {Id}.", id);
                return null;
            }
        }

        public async Task ProcesarSolicitudAsync(ProcesarSolicitudDTO decision)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/solicitudes/procesar", decision);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var mensajeLimpio = ExtraerMensajeError(errorContent);

                    // ✅ CORREGIDO: Cambiamos decision.IdSolicitud por decision.SolicitudId
                    _logger.LogWarning("Fallo al procesar la solicitud {Id}: {Mensaje}", decision.SolicitudId, mensajeLimpio);
                    throw new ApplicationException(mensajeLimpio);
                }
            }
            catch (HttpRequestException ex)
            {
                // ✅ CORREGIDO: Cambiamos decision.IdSolicitud por decision.SolicitudId
                _logger.LogError(ex, "Fallo de conectividad al intentar procesar la solicitud {Id}.", decision.SolicitudId);
                throw new ApplicationException("Error de conexión al intentar procesar la solicitud.");
            }
        }

        public async Task<List<SolicitudIngresoDTO>> GetHistorialSolicitudesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<SolicitudIngresoDTO>>("api/solicitudes/historial");
                return response ?? new List<SolicitudIngresoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el historial de solicitudes de ingreso.");
                return new List<SolicitudIngresoDTO>();
            }
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private string ExtraerMensajeError(string errorContent)
        {
            if (string.IsNullOrWhiteSpace(errorContent)) return "Ocurrió un error inesperado en el servidor.";

            if (errorContent.Trim().StartsWith("{"))
            {
                try
                {
                    using var jsonDoc = JsonDocument.Parse(errorContent);
                    if (jsonDoc.RootElement.TryGetProperty("message", out var messageProp))
                    {
                        return messageProp.GetString() ?? "Error de validación en el servidor.";
                    }
                }
                catch
                {
                    // Si el JSON es inválido, ignoramos y devolvemos texto genérico
                }
                return "Error de validación: Verifique los datos ingresados.";
            }

            // Si es un string simple, lo devolvemos tal cual
            return errorContent;
        }
    }
}