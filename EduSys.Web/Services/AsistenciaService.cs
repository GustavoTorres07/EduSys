using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace EduSys.Web.Services
{
    public class AsistenciaService : IAsistenciaService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AsistenciaService> _logger;

        public AsistenciaService(HttpClient http, ILogger<AsistenciaService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<AsistenciaGrillaDTO> GetGrillaByComisionAsync(int idComision)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<AsistenciaGrillaDTO>($"api/asistencias/grilla/comision/{idComision}");
                return response ?? new AsistenciaGrillaDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la grilla de asistencia del servidor para la comisión {IdComision}", idComision);
                return new AsistenciaGrillaDTO();
            }
        }

        public async Task<bool> GuardarGrillaAsync(GuardarAsistenciaRequestDTO request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/asistencias/guardar", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error del servidor al guardar asistencia: {ErrorMsg}", errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción crítica al intentar guardar la asistencia de la comisión {IdComision}", request.IdComision);
                return false;
            }
        }

        // 🚀 AQUÍ SÍ VA EL MÉTODO PARA SUBIR EL ARCHIVO
        public async Task<string?> SubirCertificadoAsync(string base64Content, string fileName)
        {
            try
            {
                var request = new { Base64Content = base64Content, FileName = fileName };
                var response = await _http.PostAsJsonAsync("api/files/upload-certificado", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<FileUploadResponse>();
                    return result?.Url;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir el certificado.");
                return null;
            }
        }

        private class FileUploadResponse
        {
            public string Url { get; set; } = string.Empty;
        }
    }
}