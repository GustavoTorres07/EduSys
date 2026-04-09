using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotasController : ControllerBase
    {
        private readonly INotasRepository _notasRepo;

        public NotasController(INotasRepository notasRepo)
        {
            _notasRepo = notasRepo;
        }

        [HttpGet("planilla/{idComision}")]
        [Authorize(Roles = "Docente, EVA_CARGAR_NOTAS, ACTA_VER, EVA_ABM")]
        public async Task<ActionResult<PlanillaNotasDTO>> GetPlanilla(int idComision)
        {
            var planilla = await _notasRepo.GetPlanillaAsync(idComision);
            if (planilla == null) return NotFound(new { message = $"No se encontró la comisión con ID {idComision}" });
            return Ok(planilla);
        }

        [HttpPost("guardar")]
        [Authorize(Roles = "Docente, EVA_CARGAR_NOTAS")]
        public async Task<IActionResult> GuardarNota([FromBody] GuardarNotaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var exito = await _notasRepo.GuardarNotaAsync(dto.IdInscripcion, dto.IdEvaluacion, dto.Valor);
            if (exito) return Ok(new { message = "Nota guardada correctamente." });
            return BadRequest(new { message = "No se pudo guardar la nota. Verifique que el acta no esté cerrada." });
        }

        [HttpPost("nueva-evaluacion/{idComision}")]
        [Authorize(Roles = "Docente, EVA_ABM")]
        public async Task<IActionResult> CrearEvaluacion(int idComision, [FromBody] EvaluacionDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var exito = await _notasRepo.CrearEvaluacionAsync(idComision, dto);
            if (exito) return Ok(new { message = "Evaluación creada correctamente." });
            return BadRequest(new { message = "Error al crear la evaluación." });
        }

        [HttpPut("editar-evaluacion")]
        [Authorize(Roles = "Docente, EVA_ABM")]
        public async Task<IActionResult> EditarEvaluacion([FromBody] EvaluacionDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var exito = await _notasRepo.EditarEvaluacionAsync(dto);
            if (exito) return Ok(new { message = "Evaluación editada correctamente." });
            return BadRequest(new { message = "Error al editar. Puede que el acta ya esté cerrada." });
        }

        [HttpDelete("evaluacion/{id}")]
        [Authorize(Roles = "Docente, EVA_ABM")]
        public async Task<IActionResult> EliminarEvaluacion([FromRoute] int id)
        {
            if (id <= 0) return BadRequest(new { message = "ID de evaluación inválido." });
            var result = await _notasRepo.EliminarEvaluacionAsync(id);
            if (!result) return BadRequest(new { message = "No se puede eliminar la evaluación. Verifique que no esté cerrada." });
            return Ok(new { message = "Evaluación eliminada correctamente." });
        }
    }
}