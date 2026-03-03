using EduSys.Api.Repositories;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Docente, Administrador, Secretaria Academica")]
    public class NotasController : ControllerBase
    {
        // 👇 Aquí la definiste como _notasRepo
        private readonly INotasRepository _notasRepo;

        public NotasController(INotasRepository notasRepo)
        {
            _notasRepo = notasRepo;
        }

        [HttpGet("planilla/{idComision}")]
        public async Task<ActionResult<PlanillaNotasDTO>> GetPlanilla(int idComision)
        {
            var planilla = await _notasRepo.GetPlanillaAsync(idComision);

            if (planilla == null)
                return NotFound($"No se encontró la comisión con ID {idComision}");

            return Ok(planilla);
        }

        [HttpPost("guardar")]
        public async Task<IActionResult> GuardarNota([FromBody] GuardarNotaDTO dto)
        {
            if (dto == null) return BadRequest("Datos inválidos.");

            var exito = await _notasRepo.GuardarNotaAsync(dto.IdInscripcion, dto.IdEvaluacion, dto.Valor);

            if (exito)
                return Ok(new { message = "Nota guardada correctamente." });
            else
                return BadRequest("No se pudo guardar la nota. Verifique que el acta no esté cerrada.");
        }

        // ⚠️ OJO: En tu servicio Front llamabas a "nueva-evaluacion", aquí decía "crear-evaluacion".
        // Lo he unificado a "nueva-evaluacion" para que coincida con el servicio que te pasé antes.
        [HttpPost("nueva-evaluacion/{idComision}")]
        public async Task<IActionResult> CrearEvaluacion(int idComision, [FromBody] EvaluacionDTO dto)
        {
            var exito = await _notasRepo.CrearEvaluacionAsync(idComision, dto);
            if (exito) return Ok();
            return BadRequest("Error al crear evaluación.");
        }

        [HttpPut("editar-evaluacion")]
        public async Task<IActionResult> EditarEvaluacion([FromBody] EvaluacionDTO dto)
        {
            var exito = await _notasRepo.EditarEvaluacionAsync(dto);
            if (exito) return Ok();
            return BadRequest("Error al editar. Puede que el acta esté cerrada.");
        }

        [HttpPost("cerrar-acta")]
        public async Task<IActionResult> CerrarActa([FromBody] CierreActaDTO dto)
        {
            var exito = await _notasRepo.CerrarActaAsync(dto);
            if (exito) return Ok(new { message = "Acta cerrada correctamente." });
            return BadRequest("Error al cerrar el acta.");
        }

        [HttpPost("reabrir-acta/{id}")]
        public async Task<ActionResult> ReabrirActa(int id)
        {
            // ✅ CORREGIDO: Usar _notasRepo (que es como se llama tu variable arriba)
            var exito = await _notasRepo.ReabrirActaAsync(id);

            if (!exito) return BadRequest("No se pudo reabrir el acta o no existe.");
            return Ok();
        }

        [HttpPost("cerrar-cursada")]
        public async Task<IActionResult> CerrarCursada([FromBody] CierreCursadaDTO dto)
        {
            var exito = await _notasRepo.CerrarActaComisionAsync(dto.IdComision, dto.Libro, dto.Folio);
            if (exito) return Ok(new { message = "Cursada cerrada y promedios calculados correctamente." });

            return BadRequest("Error al cerrar el acta de la comisión.");
        }
    }
}