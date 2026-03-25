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
    public class InscripcionesController : ControllerBase
    {
        private readonly IInscripcionRepository _inscripcionRepository;

        public InscripcionesController(IInscripcionRepository inscripcionRepository)
        {
            _inscripcionRepository = inscripcionRepository;
        }

        // =========================================================================
        // 1. INSCRIBIRSE (Acción principal de Autogestión)
        // =========================================================================
        [HttpPost("inscribir")]
        // 🔒 CANDADO ESTRUCTURAL: Solo los alumnos pueden usar la autogestión
        [Authorize(Roles = "Alumno")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoInscripcionDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultadoInscripcionDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResultadoInscripcionDTO>> Inscribir([FromBody] InscripcionCursadaRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var resultado = await _inscripcionRepository.InscribirAlumnoAsync(dto);

                if (resultado.Exito)
                    return Ok(resultado);
                else
                    return BadRequest(resultado); // Devuelve error de validación (ej: cupo, correlativa)
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInscripcionDTO
                {
                    Exito = false,
                    Mensaje = "Error interno al procesar la inscripción: " + ex.Message
                });
            }
        }

        // =========================================================================
        // 2. VER OFERTA (Para que el alumno elija o el admin consulte)
        // =========================================================================
        [HttpGet("oferta/{idAlumno}")]
        // 🔒 CANDADO MIXTO: Entra el Alumno (para inscribirse) o el Administrativo (para forzar inscripción)
        [Authorize(Roles = "Alumno, INS_CURSADA_GESTION")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ComisionDTO>))]
        public async Task<ActionResult<List<ComisionDTO>>> GetOferta(int idAlumno, [FromQuery] int idPeriodo)
        {
            var oferta = await _inscripcionRepository.GetOfertaParaAlumnoAsync(idAlumno, idPeriodo);
            return Ok(oferta);
        }

        // =========================================================================
        // 3. VER MIS INSCRIPCIONES (Autogestión)
        // =========================================================================
        [HttpGet("alumno/{idAlumno}/periodo/{idPeriodo}")]
        // 🔒 CANDADO ESTRUCTURAL
        [Authorize(Roles = "Alumno")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<InscripcionCursadaListadoDTO>))]
        public async Task<ActionResult<List<InscripcionCursadaListadoDTO>>> GetPorAlumno(int idAlumno, int idPeriodo)
        {
            var lista = await _inscripcionRepository.GetInscripcionesPorAlumnoAsync(idAlumno, idPeriodo);

            var result = lista.Select(i => new InscripcionCursadaListadoDTO
            {
                IdInscripcion = i.Id,
                Materia = i.IdComisionNavigation?.IdPlanMateriaNavigation?.IdMateriaNavigation?.Nombre ?? "Materia sin nombre",
                ComisionCodigo = i.IdComisionNavigation?.Codigo ?? "S/C",
                Estado = i.Estado,
                Fecha = i.FechaInscripcion ?? DateTime.Now
            }).ToList();

            return Ok(result);
        }

        // =========================================================================
        // 4. DARSE DE BAJA
        // =========================================================================
        [HttpDelete("{id}")]
        // 🔒 CANDADO MIXTO: El alumno se puede dar de baja a sí mismo, o el administrativo puede darle la baja
        [Authorize(Roles = "Alumno, INS_CURSADA_GESTION")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Cancelar(int id)
        {
            // ⚠️ NOTA DE SEGURIDAD: A futuro, validar que el ID del token JWT coincida 
            // con el dueño de esta inscripción, o que el usuario tenga rol de Admin.
            try
            {
                var exito = await _inscripcionRepository.CancelarInscripcionAsync(id);

                if (!exito)
                    return NotFound(new { message = "Inscripción no encontrada o ya dada de baja." });

                return Ok(new { message = "Inscripción cancelada correctamente." });
            }
            catch (Exception ex)
            {
                // Captura excepciones de negocio (ej: ventana de periodo cerrada)
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================================================================
        // 5. INSCRIBIR ADMIN (Con Overrides / Excepciones)
        // =========================================================================
        [HttpPost("admin/inscribir")]
        // 🔒 CANDADO REAL: Exclusivo para gestión administrativa
        [Authorize(Roles = "INS_CURSADA_GESTION")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoInscripcionDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultadoInscripcionDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResultadoInscripcionDTO>> InscribirManual([FromBody] InscripcionManualDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var resultado = await _inscripcionRepository.InscribirAdminAsync(dto);

                if (resultado.Exito)
                    return Ok(resultado);
                else
                    return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInscripcionDTO
                {
                    Exito = false,
                    Mensaje = "Error interno al forzar inscripción: " + ex.Message
                });
            }
        }

        // =========================================================================
        // 6. VER INSCRIPCIONES (ADMIN)
        // =========================================================================
        [HttpGet("admin/alumno/{idAlumno}")]
        // 🔒 CANDADO REAL: Exclusivo para gestión administrativa
        [Authorize(Roles = "INS_CURSADA_GESTION")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<InscripcionCursadaListadoDTO>))]
        public async Task<ActionResult<List<InscripcionCursadaListadoDTO>>> GetInscripcionesAlumno(int idAlumno)
        {
            var inscripciones = await _inscripcionRepository.GetInscripcionesByAlumnoAsync(idAlumno);
            return Ok(inscripciones);
        }
    }
}