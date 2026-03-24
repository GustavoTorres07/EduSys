using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Asegura que solo usuarios autenticados accedan
    public class AsistenciasController : ControllerBase
    {
        private readonly IAsistenciaRepository _asistenciaRepo;

        public AsistenciasController(IAsistenciaRepository asistenciaRepo)
        {
            _asistenciaRepo = asistenciaRepo;
        }

        [HttpGet("grilla/comision/{idComision}")]
        public async Task<ActionResult<AsistenciaGrillaDTO>> GetGrillaComision(int idComision)
        {
            var grilla = await _asistenciaRepo.GetGrillaByComisionAsync(idComision);
            return Ok(grilla);
        }

        [HttpPost("guardar")]
        public async Task<ActionResult> GuardarAsistencia([FromBody] GuardarAsistenciaRequestDTO request)
        {
            if (request == null || request.IdComision <= 0)
                return BadRequest("Datos de asistencia inválidos.");

            var resultado = await _asistenciaRepo.GuardarGrillaAsync(request);

            if (resultado)
                return Ok(new { Mensaje = "Asistencia guardada correctamente." });
            else
                return StatusCode(500, "Ocurrió un error al guardar la asistencia.");
        }
    }
}