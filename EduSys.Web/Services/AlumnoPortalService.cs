using System.Net.Http.Json;
using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;

namespace EduSys.Web.Services
{
    public class AlumnoPortalService : IAlumnoPortalService
    {
        private readonly HttpClient _http;

        public AlumnoPortalService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<NotificacionDTO>> GetNotificacionesAsync()
        {
            try
            {
                // Intentamos obtener las notificaciones
                var resultado = await _http.GetFromJsonAsync<List<NotificacionDTO>>("api/AlumnoPortal/notificaciones");
                return resultado ?? new List<NotificacionDTO>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // ✅ CORRECCIÓN CLAVE:
                // Si la API responde 401 (No autorizado), asumimos que el usuario no está logueado o el token venció.
                // Retornamos lista vacía para no romper la interfaz visual.
                return new List<NotificacionDTO>();
            }
            catch (Exception)
            {
                // Cualquier otro error de conexión, retornamos vacío para que la app siga funcionando
                return new List<NotificacionDTO>();
            }
        }

        public async Task<AlumnoDTO> GetPerfilAsync()
        {
            try
            {
                // Llama al endpoint de tu backend que devuelve el perfil del alumno logueado
                var response = await _http.GetFromJsonAsync<AlumnoDTO>("api/alumnoportal/perfil");
                return response ?? new AlumnoDTO();
            }
            catch (Exception)
            {
                return new AlumnoDTO(); // Si hay error, devuelve un objeto vacío en lugar de romper
            }
        }
        public async Task MarcarLeidaAsync(int id)
        {
            await _http.PostAsync($"api/AlumnoPortal/notificaciones/leer/{id}", null);
        }

        public async Task<List<CursadaAlumnoDTO>> GetMisCursadasAsync()
        {
            var resultado = await _http.GetFromJsonAsync<List<CursadaAlumnoDTO>>("api/AlumnoPortal/mis-cursadas");
            return resultado ?? new List<CursadaAlumnoDTO>();
        }
    }
}