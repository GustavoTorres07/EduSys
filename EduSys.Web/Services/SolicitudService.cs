using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class SolicitudService : ISolicitudService
    {
        private readonly HttpClient _http; // <-- Aquí se llama _http

        public SolicitudService(HttpClient http)
        {
            _http = http;
        }

        // 1. Enviar Solicitud (POST)
        public async Task EnviarSolicitudAsync(SolicitudIngresoRequestDTO solicitud)
        {
            var response = await _http.PostAsJsonAsync("api/solicitudes/solicitar", solicitud);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }
        }

        // 2. Obtener Pendientes (GET)
        public async Task<List<SolicitudIngresoDTO>> GetPendientesAsync()
        {
            var response = await _http.GetAsync("api/solicitudes/pendientes");

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error del servidor ({response.StatusCode}): {errorMsg}");
            }

            var resultado = await response.Content.ReadFromJsonAsync<List<SolicitudIngresoDTO>>();
            return resultado ?? new List<SolicitudIngresoDTO>();
        }

        // 3. Obtener por ID (GET)
        public async Task<SolicitudIngresoDTO> GetSolicitudByIdAsync(int id)
        {
            var resultado = await _http.GetFromJsonAsync<SolicitudIngresoDTO>($"api/solicitudes/{id}");

            if (resultado == null) throw new Exception("No se pudo obtener la solicitud.");

            return resultado;
        }

        // 4. Procesar Solicitud (POST)
        public async Task ProcesarSolicitudAsync(ProcesarSolicitudDTO decision)
        {
            var response = await _http.PostAsJsonAsync("api/solicitudes/procesar", decision);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }
        }

        // 5. Historial (CORREGIDO)
        public async Task<List<SolicitudIngresoDTO>> GetHistorialSolicitudes()
        {
            // ✅ CORRECCIÓN: Usar _http en lugar de _httpClient
            return await _http.GetFromJsonAsync<List<SolicitudIngresoDTO>>("api/solicitudes/historial");
        }
    }
}