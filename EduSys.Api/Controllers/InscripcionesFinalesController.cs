using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 🔓 Candado Base: Requiere estar logueado
    [Authorize]
    public class InscripcionesFinalesController : ControllerBase
    {
        private readonly IInscripcionFinalRepository _repo;

        public InscripcionesFinalesController(IInscripcionFinalRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("oferta/{idAlumno}")]
        // 🔒 CANDADO MIXTO: El alumno puede ver su oferta, o un administrativo con permisos
        [Authorize(Roles = "Alumno, INS_FINAL_MESAS_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MesaFinalOfertaDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<MesaFinalOfertaDTO>>> GetOferta(int idAlumno, [FromQuery] int idPeriodo)
        {
            return Ok(await _repo.GetOfertaParaAlumnoAsync(idAlumno, idPeriodo));
        }

        [HttpGet("mis-inscripciones/{idAlumno}")]
        // 🔒 CANDADO MIXTO
        [Authorize(Roles = "Alumno, INS_FINAL_MESAS_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MesaFinalOfertaDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<MesaFinalOfertaDTO>>> GetMisInscripciones(int idAlumno, [FromQuery] int idPeriodo)
        {
            return Ok(await _repo.GetMisInscripcionesAsync(idAlumno, idPeriodo));
        }

        [HttpPost("inscribir")]
        // 🔒 CANDADO ESTRUCTURAL: Acción exclusiva de autogestión del alumno
        [Authorize(Roles = "Alumno")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoOperacionDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultadoOperacionDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ResultadoOperacionDTO>> Inscribir([FromBody] InscripcionFinalRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var res = await _repo.InscribirAlumnoAsync(dto);

            if (res.Exito) return Ok(res);

            // Rebota si está fuera de término, rompe reglas o no hay cupo
            return BadRequest(res);
        }

        [HttpDelete("cancelar/{idInscripcion}")]
        // 🔒 CANDADO MIXTO: El alumno puede bajarse, o un administrativo puede darlo de baja
        [Authorize(Roles = "Alumno, INS_FINAL_MESAS_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoOperacionDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultadoOperacionDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ResultadoOperacionDTO>> Cancelar(int idInscripcion, [FromQuery] int idAlumno)
        {
            // ⚠️ NOTA DE SEGURIDAD: Lo ideal a futuro es que idAlumno 
            // se extraiga del Token JWT en lugar de recibirlo por Query String.
            var res = await _repo.CancelarInscripcionAsync(idInscripcion, idAlumno);

            if (res.Exito) return Ok(res);

            return BadRequest(res);
        }
    }
}