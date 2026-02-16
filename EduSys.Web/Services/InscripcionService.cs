using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class InscripcionService : IInscripcionService
    {
        private readonly HttpClient _http;

        public InscripcionService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ResultadoInscripcionDTO> InscribirAsync(InscripcionCursadaRequestDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/inscripciones/inscribir", dto);

                // Leemos el resultado sea cual sea el status code (porque el BadRequest también trae el mensaje de error)
                var resultado = await response.Content.ReadFromJsonAsync<ResultadoInscripcionDTO>();
                return resultado ?? new ResultadoInscripcionDTO { Exito = false, Mensaje = "Error desconocido." };
            }
            catch (Exception ex)
            {
                return new ResultadoInscripcionDTO { Exito = false, Mensaje = "Error de conexión: " + ex.Message };
            }
        }

        public async Task<List<InscripcionCursadaListadoDTO>> GetInscripcionesAlumnoAsync(int idAlumno, int idPeriodo)
        {
            return await _http.GetFromJsonAsync<List<InscripcionCursadaListadoDTO>>($"api/inscripciones/alumno/{idAlumno}/periodo/{idPeriodo}")
                   ?? new List<InscripcionCursadaListadoDTO>();
        }


        public async Task<bool> CancelarInscripcionAsync(int idInscripcion)
        {
            var response = await _http.DeleteAsync($"api/inscripciones/{idInscripcion}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ComisionDTO>> GetOfertaInscripcionAsync(int idAlumno, int idPeriodo)
        {
            return await _http.GetFromJsonAsync<List<ComisionDTO>>($"api/inscripciones/oferta/{idAlumno}?idPeriodo={idPeriodo}")
                   ?? new List<ComisionDTO>();
        }

        // Implementación en InscripcionService
        // En InscripcionService.cs
        public async Task<ResultadoInscripcionDTO> InscribirAdminAsync(InscripcionManualDTO dto)
        {
            var response = await _http.PostAsJsonAsync("api/inscripciones/admin/inscribir", dto);
            // Manejar errores de conexión si es necesario
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ResultadoInscripcionDTO>();
                return error ?? new ResultadoInscripcionDTO { Exito = false, Mensaje = "Error en el servidor" };
            }
            return await response.Content.ReadFromJsonAsync<ResultadoInscripcionDTO>();
        }
    }
}