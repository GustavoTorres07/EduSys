using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace EduSys.Web.Services
{
    public class ReportesService : IReportesService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ReportesService> _logger; // ✅ Agregado para trazabilidad

        public ReportesService(HttpClient http, ILogger<ReportesService> logger)
        {
            _http = http;
            _logger = logger;
        }

        // ==========================================
        // CONSULTAS Y LISTADOS
        // ==========================================

        public async Task<List<InscripcionGlobalDTO>> GetInscripcionesGlobalAsync(int idPeriodo, int? idCarrera)
        {
            try
            {
                var url = $"api/reportes/inscripciones-global?idPeriodo={idPeriodo}";
                if (idCarrera.HasValue) url += $"&idCarrera={idCarrera}";

                return await _http.GetFromJsonAsync<List<InscripcionGlobalDTO>>(url) ?? new List<InscripcionGlobalDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las inscripciones globales para el periodo {IdPeriodo}.", idPeriodo);
                return new List<InscripcionGlobalDTO>();
            }
        }

        public async Task<List<AlumnoResumenInscripcionDTO>> GetAlumnosInscriptosAsync(int idPeriodo, int idCarrera, int? idSede)
        {
            try
            {
                var url = $"api/reportes/alumnos-inscriptos?idPeriodo={idPeriodo}&idCarrera={idCarrera}";
                if (idSede.HasValue && idSede.Value > 0) url += $"&idSede={idSede.Value}";

                return await _http.GetFromJsonAsync<List<AlumnoResumenInscripcionDTO>>(url) ?? new List<AlumnoResumenInscripcionDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alumnos inscriptos (Periodo: {IdPeriodo}, Carrera: {IdCarrera}).", idPeriodo, idCarrera);
                return new List<AlumnoResumenInscripcionDTO>();
            }
        }

        public async Task<List<HorarioVisualizacionDTO>> GetHorariosCursandoAsync(int idPeriodo, int idAlumno)
        {
            try
            {
                var url = $"api/reportes/horarios-alumno-cursando?idPeriodo={idPeriodo}&idAlumno={idAlumno}";
                return await _http.GetFromJsonAsync<List<HorarioVisualizacionDTO>>(url) ?? new List<HorarioVisualizacionDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener horarios en curso del alumno {IdAlumno}.", idAlumno);
                return new List<HorarioVisualizacionDTO>();
            }
        }

        public async Task<HistoriaAcademicaDTO?> GetHistoriaAcademicaAsync(int idAlumno)
        {
            try
            {
                var url = $"api/reportes/historia-academica?idAlumno={idAlumno}";
                return await _http.GetFromJsonAsync<HistoriaAcademicaDTO>(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la historia académica del alumno {IdAlumno}.", idAlumno);
                return null;
            }
        }

        // ==========================================
        // DESCARGAS DE PDF
        // ==========================================

        public async Task<byte[]> DescargarConstanciaInscripcionPdfAsync(int idAlumno, int idPeriodo)
        {
            return await ProcesarDescargaPdfAsync($"api/Reportes/constancia-inscripcion?idAlumno={idAlumno}&idPeriodo={idPeriodo}", "la constancia de inscripción");
        }

        public async Task<byte[]> DescargarHorarioPdfAsync(int idPeriodo, int idCarrera, int idSede)
        {
            return await ProcesarDescargaPdfAsync($"api/reportes/horario-descargar?idPeriodo={idPeriodo}&idCarrera={idCarrera}&idSede={idSede}", "el horario");
        }

        public async Task<byte[]> DescargarCertificadoRegularPdfAsync(int idAlumno, int idPeriodo)
        {
            return await ProcesarDescargaPdfAsync($"api/reportes/certificado-alumno-regular-descargar?idAlumno={idAlumno}&idPeriodo={idPeriodo}", "el certificado de alumno regular");
        }

        public async Task<byte[]> DescargarConstanciaFinalAsync(int idInscripcion)
        {
            return await ProcesarDescargaPdfAsync($"api/Reportes/constancia-final/{idInscripcion}", "la constancia de final");
        }

        public async Task<byte[]> DescargarAnaliticoProvisorioAsync()
        {
            // Llamamos a la API sin query string, ya que el Backend usa el Token
            return await ProcesarDescargaPdfAsync("api/Reportes/analitico-provisorio", "el analítico provisorio");
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private async Task<byte[]> ProcesarDescargaPdfAsync(string url, string nombreDocumento)
        {
            try
            {
                var response = await _http.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var mensajeLimpio = ExtraerMensajeError(errorContent);

                _logger.LogWarning("Fallo al descargar {Documento}: {Mensaje}", nombreDocumento, mensajeLimpio);
                throw new ApplicationException(mensajeLimpio);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Fallo de conexión al intentar descargar {Documento}.", nombreDocumento);
                throw new ApplicationException($"Error de conexión al intentar generar {nombreDocumento}.");
            }
        }

        private string ExtraerMensajeError(string errorContent)
        {
            if (string.IsNullOrWhiteSpace(errorContent)) return "Ocurrió un error inesperado en el servidor al generar el documento.";

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
                    // Ignoramos errores de parseo y devolvemos texto por defecto
                }
                return "Error al generar el documento. Verifique los datos.";
            }

            return errorContent;
        }
    }
}