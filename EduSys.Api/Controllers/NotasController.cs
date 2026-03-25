using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 🔓 Candado Base: Exige autenticación, pero los permisos se delegan método por método.
    [Authorize]
    public class NotasController : ControllerBase
    {
        private readonly INotasRepository _notasRepo;

        public NotasController(INotasRepository notasRepo)
        {
            _notasRepo = notasRepo;
        }

        [HttpGet("planilla/{idComision}")]
        // 🔒 CANDADO MIXTO: El Docente ve su planilla, y los administrativos de Notas/Actas/Evaluaciones también
        [Authorize(Roles = "Docente, EVA_CARGAR_NOTAS, ACTA_VER, EVA_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlanillaNotasDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlanillaNotasDTO>> GetPlanilla(int idComision)
        {
            var planilla = await _notasRepo.GetPlanillaAsync(idComision);

            if (planilla == null)
                return NotFound(new { message = $"No se encontró la comisión con ID {idComision}" });

            return Ok(planilla);
        }

        [HttpPost("guardar")]
        // 🔒 CANDADO MIXTO
        [Authorize(Roles = "Docente, EVA_CARGAR_NOTAS")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GuardarNota([FromBody] GuardarNotaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exito = await _notasRepo.GuardarNotaAsync(dto.IdInscripcion, dto.IdEvaluacion, dto.Valor);

            if (exito)
                return Ok(new { message = "Nota guardada correctamente." });
            else
                return BadRequest(new { message = "No se pudo guardar la nota. Verifique que el acta no esté cerrada." });
        }

        [HttpPost("nueva-evaluacion/{idComision}")]
        // 🔒 CANDADO MIXTO
        [Authorize(Roles = "Docente, EVA_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CrearEvaluacion(int idComision, [FromBody] EvaluacionDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exito = await _notasRepo.CrearEvaluacionAsync(idComision, dto);
            if (exito) return Ok(new { message = "Evaluación creada correctamente." });

            return BadRequest(new { message = "Error al crear la evaluación." });
        }

        [HttpPut("editar-evaluacion")]
        // 🔒 CANDADO MIXTO
        [Authorize(Roles = "Docente, EVA_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditarEvaluacion([FromBody] EvaluacionDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exito = await _notasRepo.EditarEvaluacionAsync(dto);
            if (exito) return Ok(new { message = "Evaluación editada correctamente." });

            return BadRequest(new { message = "Error al editar. Puede que el acta ya esté cerrada." });
        }

        [HttpDelete("evaluacion/{id}")]
        // 🔒 CANDADO MIXTO
        [Authorize(Roles = "Docente, EVA_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EliminarEvaluacion([FromRoute] int id)
        {
            if (id <= 0) return BadRequest(new { message = "ID de evaluación inválido." });

            var result = await _notasRepo.EliminarEvaluacionAsync(id);

            if (!result)
                return BadRequest(new { message = "No se puede eliminar la evaluación. Verifique que no esté cerrada." });

            return Ok(new { message = "Evaluación eliminada correctamente." });
        }

        [HttpPost("cerrar-acta")]
        // 🔒 CANDADO MIXTO: Ambos pueden cerrar el acta normal de una evaluación
        [Authorize(Roles = "Docente, EVA_CARGAR_NOTAS")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CerrarActa([FromBody] CierreActaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exito = await _notasRepo.CerrarActaAsync(dto);
            if (exito) return Ok(new { message = "Acta cerrada correctamente." });

            return BadRequest(new { message = "Error al cerrar el acta. Verifique los datos ingresados." });
        }

        [HttpPost("cerrar-cursada")]
        // 🔒 CANDADO MIXTO: Cierre final de la materia
        [Authorize(Roles = "Docente, EVA_CARGAR_NOTAS")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CerrarCursada([FromBody] CierreCursadaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exito = await _notasRepo.CerrarActaComisionAsync(dto.IdComision, dto.Libro, dto.Folio);
            if (exito) return Ok(new { message = "Cursada cerrada y promedios calculados correctamente." });

            return BadRequest(new { message = "Error al cerrar el acta de la comisión." });
        }

        // ===================================================================================
        // ⚠️ ZONA ADMINISTRATIVA RESTRINGIDA (Sin acceso para el Rol Docente)
        // ===================================================================================

        [HttpPost("reabrir-acta/{id}")]
        // 🔒 CANDADO ESTRICTO: Solo Secretaría / Admin pueden deshacer cierres de actas
        [Authorize(Roles = "EVA_CARGAR_NOTAS, ACTA_VER")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ReabrirActa(int id)
        {
            var exito = await _notasRepo.ReabrirActaAsync(id);

            if (!exito) return BadRequest(new { message = "No se pudo reabrir el acta o no existe." });
            return Ok(new { message = "Acta reabierta exitosamente." });
        }

        [HttpPost("inscripcion/{id}/toggle-cierre")]
        // 🔒 CANDADO ESTRICTO
        [Authorize(Roles = "EVA_CARGAR_NOTAS, ACTA_VER")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleCierreIndividual(int id)
        {
            var result = await _notasRepo.ToggleCierreCursadaIndividualAsync(id);
            if (!result) return NotFound(new { message = "Inscripción no encontrada." });

            return Ok(new { message = "Estado de cierre alternado exitosamente." });
        }

        [HttpPost("comision/{idComision}/reabrir")]
        // 🔒 CANDADO ESTRICTO
        [Authorize(Roles = "EVA_CARGAR_NOTAS, ACTA_VER")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReabrirComision(int idComision)
        {
            var result = await _notasRepo.ReabrirActaComisionAsync(idComision);
            if (!result) return NotFound(new { message = "Comisión no encontrada o no se pudo reabrir." });

            return Ok(new { message = "Comisión reabierta exitosamente." });
        }
    }
}