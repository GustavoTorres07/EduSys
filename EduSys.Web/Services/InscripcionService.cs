using System.Net.Http.Json;
using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;

namespace EduSys.Web.Services
{
    public class InscripcionService : IInscripcionService
    {
        private readonly HttpClient _http;

        public InscripcionService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ResultadoInscripcionDTO> InscribirAlumnoAsync(InscripcionCursadaRequestDTO dto)
        {
            var response = await _http.PostAsJsonAsync("api/inscripciones", dto);
            var result = await response.Content.ReadFromJsonAsync<ResultadoInscripcionDTO>();
            return result ?? new ResultadoInscripcionDTO { Exito = false, Mensaje = "Error desconocido." };
        }

        public async Task<List<ComisionDTO>> GetOfertaParaAlumnoAsync(int idAlumno, int idPeriodo)
        {
            // ✅ Correcto - idPeriodo como query string
            var response = await _http.GetFromJsonAsync<List<ComisionDTO>>(
                $"api/inscripciones/oferta/{idAlumno}?idPeriodo={idPeriodo}");
            return response ?? new List<ComisionDTO>();
        }

        public async Task<bool> CancelarInscripcionAsync(int idInscripcion)
        {
            var response = await _http.DeleteAsync($"api/inscripciones/{idInscripcion}");
            return response.IsSuccessStatusCode;
        }

        public async Task<ResultadoInscripcionDTO> InscribirAdminAsync(InscripcionManualDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/inscripciones/admin/inscribir", dto);

                // Solo intentamos leer el JSON si la respuesta fue Ok o BadRequest
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var result = await response.Content.ReadFromJsonAsync<ResultadoInscripcionDTO>();
                    return result ?? new ResultadoInscripcionDTO { Exito = false, Mensaje = "Respuesta vacía del servidor." };
                }

                // Si es 500 o error de red, evitamos que Blazor crashee
                return new ResultadoInscripcionDTO { Exito = false, Mensaje = $"Error del servidor. Código: {response.StatusCode}" };
            }
            catch (Exception ex)
            {
                return new ResultadoInscripcionDTO { Exito = false, Mensaje = $"Problema de conexión: {ex.Message}" };
            }
        }

        public async Task<List<InscripcionCursadaListadoDTO>> GetInscripcionesAlumnoAsync(int idAlumno, int idPeriodo)
        {
            var response = await _http.GetFromJsonAsync<List<InscripcionCursadaListadoDTO>>($"api/inscripciones/alumno/{idAlumno}/periodo/{idPeriodo}");
            return response ?? new List<InscripcionCursadaListadoDTO>();
        }

        public async Task<List<InscripcionCursadaListadoDTO>> GetInscripcionesByAlumnoAsync(int idAlumno)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<InscripcionCursadaListadoDTO>>($"api/inscripciones/admin/alumno/{idAlumno}");
                return response ?? new List<InscripcionCursadaListadoDTO>();
            }
            catch (Exception)
            {
                // Si la API falla, devolvemos una lista vacía para que no se rompa la UI
                return new List<InscripcionCursadaListadoDTO>();
            }
        }
    }
}