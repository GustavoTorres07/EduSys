using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace EduSys.Api.Helpers
{
    public class FileStorageHelper
    {
        private readonly Cloudinary _cloudinary;

        public FileStorageHelper(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> GuardarArchivoAsync(
            string base64String,
            string carpeta,
            string subcarpeta,
            string nombreArchivo)
        {
            if (string.IsNullOrEmpty(base64String)) return null;

            try
            {
                // 1. Limpiar encabezado base64
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
                        Type = "upload" // 👈 CLAVE: público
                    };

                    var rawResult = await _cloudinary.UploadAsync(rawParams);
                    return rawResult.SecureUrl.ToString();
                }

                // 5. Imagen → IMAGE (sin cambios)
                var imageParams = new ImageUploadParams
                {
                    File = new FileDescription(nombreArchivo, stream),
                    PublicId = rutaPublica,
                    Overwrite = true
                };

                var imageResult = await _cloudinary.UploadAsync(imageParams);
                return imageResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error subiendo a Cloudinary: {ex.Message}");
                return null;
            }
        }
    }
}
