using System.Net.Http.Json;
using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;

namespace EduSys.Web.Services
{
    public class InscripcionFinalService : IInscripcionFinalService
    {
        private readonly HttpClient _http;
        public InscripcionFinalService(HttpClient http) => _http = http;

        public async Task<List<MesaFinalOfertaDTO>> GetOfertaAsync(int idAlumno, int idPeriodo)
        {
            return await _http.GetFromJsonAsync<List<MesaFinalOfertaDTO>>($"api/inscripcionesfinales/oferta/{idAlumno}?idPeriodo={idPeriodo}") ?? new List<MesaFinalOfertaDTO>();
        }

        public async Task<List<MesaFinalOfertaDTO>> GetMisInscripcionesAsync(int idAlumno, int idPeriodo)
        {
            return await _http.GetFromJsonAsync<List<MesaFinalOfertaDTO>>($"api/inscripcionesfinales/mis-inscripciones/{idAlumno}?idPeriodo={idPeriodo}") ?? new List<MesaFinalOfertaDTO>();
        }

        public async Task<ResultadoOperacionDTO> InscribirAsync(InscripcionFinalRequestDTO dto)
        {
            var res = await _http.PostAsJsonAsync("api/inscripcionesfinales/inscribir", dto);
            if (res.IsSuccessStatusCode || res.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return await res.Content.ReadFromJsonAsync<ResultadoOperacionDTO>() ?? new ResultadoOperacionDTO { Exito = false, Mensaje = "Error desconocido." };
            }
            return new ResultadoOperacionDTO { Exito = false, Mensaje = $"Error del servidor: {res.StatusCode}" };
        }

        public async Task<ResultadoOperacionDTO> CancelarInscripcionAsync(int idInscripcion, int idAlumno)
        {
            var res = await _http.DeleteAsync($"api/inscripcionesfinales/cancelar/{idInscripcion}?idAlumno={idAlumno}");
            if (res.IsSuccessStatusCode || res.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return await res.Content.ReadFromJsonAsync<ResultadoOperacionDTO>() ?? new ResultadoOperacionDTO { Exito = false, Mensaje = "Error desconocido." };
            }
            return new ResultadoOperacionDTO { Exito = false, Mensaje = $"Error del servidor: {res.StatusCode}" };
        }
    }
}