using EduSys.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly FileStorageHelper _fileHelper;

        public FilesController(FileStorageHelper fileHelper)
        {
            _fileHelper = fileHelper;
        }

        [HttpPost("upload-certificado")]
        public async Task<IActionResult> UploadCertificado([FromBody] FileUploadRequest request)
        {
            if (string.IsNullOrEmpty(request.Base64Content))
                return BadRequest("El contenido del archivo no puede estar vacío.");

            // Usamos FileStorageHelper para subir el archivo a Cloudinary
            var url = await _fileHelper.GuardarArchivoAsync(
                request.Base64Content,
                "Asistencias",
                "Certificados",
                $"Certificado_{Guid.NewGuid()}_{request.FileName}"
            );

            if (url != null)
                return Ok(new { Url = url });

            return StatusCode(500, "Error al subir el archivo a Cloudinary.");
        }
    }

    public class FileUploadRequest
    {
        public string Base64Content { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}