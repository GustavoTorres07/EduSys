using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduSys.Api.Helpers
{
    public class FileStorageHelper
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<FileStorageHelper> _logger; // ✅ Agregamos el Logger oficial

        public FileStorageHelper(IConfiguration config, ILogger<FileStorageHelper> logger)
        {
            _logger = logger;

            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];

            // 💡 Validación temprana para detectar problemas de configuración rápido
            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                _logger.LogWarning("⚠️ ATENCIÓN: Faltan credenciales de Cloudinary en el archivo de configuración (appsettings.json).");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string?> GuardarArchivoAsync(
            string? base64String,
            string carpeta,
            string subcarpeta,
            string nombreArchivo)
        {
            // Verificación segura contra nulos o strings vacíos
            if (string.IsNullOrWhiteSpace(base64String)) return null;

            try
            {
                // 1. Limpiar encabezado base64 (ej: "data:image/png;base64,...")
                var partes = base64String.Split(',');
                var header = partes[0];
                var data = partes.Length > 1 ? partes[1] : partes[0];

                // 2. Convertir a bytes
                byte[] bytes = Convert.FromBase64String(data);
                using var stream = new MemoryStream(bytes);

                // 3. Definir ruta pública
                string rutaPublica = $"EduSys/{carpeta}/{subcarpeta}/{nombreArchivo}";

                // 4. PDF → RAW (PÚBLICO)
                if (header.Contains("application/pdf"))
                {
                    var rawParams = new RawUploadParams
                    {
                        File = new FileDescription($"{nombreArchivo}.pdf", stream),
                        PublicId = rutaPublica,
                        Overwrite = true,
                        Type = "upload" // 👈 CLAVE: público para poder visualizarlo en Blazor
                    };

                    var rawResult = await _cloudinary.UploadAsync(rawParams);
                    return rawResult?.SecureUrl?.ToString();
                }

                // 5. Imagen → IMAGE
                var imageParams = new ImageUploadParams
                {
                    File = new FileDescription(nombreArchivo, stream),
                    PublicId = rutaPublica,
                    Overwrite = true
                };

                var imageResult = await _cloudinary.UploadAsync(imageParams);
                return imageResult?.SecureUrl?.ToString();
            }
            catch (FormatException ex)
            {
                // 💡 Capturamos el error si el string Base64 estaba corrupto o mal formado
                _logger.LogError(ex, "Error de formato al intentar convertir a Base64 el archivo: {NombreArchivo}", nombreArchivo);
                return null;
            }
            catch (Exception ex)
            {
                // 💡 Reemplazamos Console.WriteLine por el Logger
                _logger.LogError(ex, "Error inesperado subiendo el archivo {NombreArchivo} a Cloudinary.", nombreArchivo);
                return null;
            }
        }
    }
}