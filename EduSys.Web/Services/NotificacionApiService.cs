using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class NotificacionApiService : INotificacionApiService
    {
        private readonly HttpClient _http;

        public NotificacionApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<NotificacionDTO>> GetMisNotificacionesAsync()
        {
            try
            {
                var result = await _http.GetFromJsonAsync<List<NotificacionDTO>>("api/notificaciones/mis-notificaciones");
                return result ?? new List<NotificacionDTO>();
            }
            catch
            {
                // Si hay un error (ej. se cayó el internet), devolvemos una lista vacía para no romper la UI
                return new List<NotificacionDTO>();
            }
        }

        public async Task<bool> MarcarComoLeidaAsync(int id)
        {
            try
            {
                var response = await _http.PutAsync($"api/notificaciones/marcar-leida/{id}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> MarcarTodasComoLeidasAsync()
        {
            try
            {
                var response = await _http.PutAsync("api/notificaciones/marcar-todas-leidas", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EnviarNotificacionMasivaAsync(NotificacionMasivaDTO request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/notificaciones/masiva", request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}