using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 🔓 Candado Base: Requiere estar logueado
    [Authorize]
    public class AsistenciasController : ControllerBase
    {
        private readonly IAsistenciaRepository _asistenciaRepo;

        public AsistenciasController(IAsistenciaRepository asistenciaRepo)
        {
            _asistenciaRepo = asistenciaRepo;
        }

        [HttpGet("grilla/comision/{idComision}")]
        // 🔒 CANDADO REAL: Puede entrar quien tenga permiso para VER o CARGAR asistencias
        [Authorize(Roles = "ASIS_VER, ASIS_CARGAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AsistenciaGrillaDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AsistenciaGrillaDTO>> GetGrillaComision(int idComision)
        {
            var grilla = await _asistenciaRepo.GetGrillaByComisionAsync(idComision);
            return Ok(grilla);
        }

        [HttpPost("guardar")]
        // 🔒 CANDADO REAL: Solo puede guardar quien tenga explícitamente el permiso de CARGAR
        [Authorize(Roles = "ASIS_CARGAR")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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