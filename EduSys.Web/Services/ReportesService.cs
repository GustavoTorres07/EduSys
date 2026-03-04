using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;

namespace EduSys.Web.Services
{
    public class ReportesService : IReportesService
    {
        private readonly HttpClient _http;

        public ReportesService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<InscripcionGlobalDTO>> GetInscripcionesGlobalAsync(int idPeriodo, int? idCarrera)
        {
            var url = $"api/reportes/inscripciones-global?idPeriodo={idPeriodo}";
            if (idCarrera.HasValue) url += $"&idCarrera={idCarrera}";

            return await _http.GetFromJsonAsync<List<InscripcionGlobalDTO>>(url)
                   ?? new List<InscripcionGlobalDTO>();
        }

        public async Task<List<AlumnoResumenInscripcionDTO>> GetAlumnosInscriptosAsync(int idPeriodo, int idCarrera, int? idSede)
        {
            var url = $"api/reportes/alumnos-inscriptos?idPeriodo={idPeriodo}&idCarrera={idCarrera}";
            if (idSede.HasValue && idSede.Value > 0) url += $"&idSede={idSede.Value}";

            return await _http.GetFromJsonAsync<List<AlumnoResumenInscripcionDTO>>(url)
                   ?? new List<AlumnoResumenInscripcionDTO>();
        }

        public async Task<byte[]> DescargarConstanciaInscripcionPdfAsync(int idAlumno, int idPeriodo)
        {
            var response = await _http.GetAsync($"api/Reportes/constancia-inscripcion?idAlumno={idAlumno}&idPeriodo={idPeriodo}");

            if (!response.IsSuccessStatusCode)
            {
                // Leemos el motivo del rechazo desde la API
                var errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMsg);
            }

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<byte[]> DescargarHorarioPdfAsync(int idPeriodo, int idCarrera, int idSede)
        {
            var url = $"api/reportes/horario-descargar?idPeriodo={idPeriodo}&idCarrera={idCarrera}&idSede={idSede}";
            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsByteArrayAsync();

            throw new Exception("Error al descargar el horario.");
        }

        public async Task<byte[]> DescargarCertificadoRegularPdfAsync(int idAlumno, int idPeriodo)
        {
            var url = $"api/reportes/certificado-alumno-regular-descargar?idAlumno={idAlumno}&idPeriodo={idPeriodo}";
            var response = await _http.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsByteArrayAsync();
            throw new Exception("Error al descargar el certificado.");
        }

        public async Task<List<HorarioVisualizacionDTO>> GetHorariosCursandoAsync(int idPeriodo, int idAlumno)
        {
            var url = $"api/reportes/horarios-alumno-cursando?idPeriodo={idPeriodo}&idAlumno={idAlumno}";
            return await _http.GetFromJsonAsync<List<HorarioVisualizacionDTO>>(url)
                   ?? new List<HorarioVisualizacionDTO>();
        }

        public async Task<HistoriaAcademicaDTO> GetHistoriaAcademicaAsync(int idAlumno)
        {
            var url = $"api/reportes/historia-academica?idAlumno={idAlumno}";
            return await _http.GetFromJsonAsync<HistoriaAcademicaDTO>(url);
        }

        public async Task<byte[]> DescargarConstanciaFinalAsync(int idInscripcion)
        {
            return await _http.GetByteArrayAsync($"api/Reportes/constancia-final/{idInscripcion}");
        }

        public async Task<byte[]> DescargarAnaliticoProvisorioAsync()
        {
            var response = await _http.GetAsync("api/Reportes/analitico-provisorio");

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMsg);
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}