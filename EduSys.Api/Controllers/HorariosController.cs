using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 🔓 Candado Base: Aseguramos que solo usuarios del sistema vean los horarios
    [Authorize]
    public class HorariosController : ControllerBase
    {
        private readonly IHorarioRepository _repo;

        public HorariosController(IHorarioRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("comision/{idComision}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<HorarioComisionDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<HorarioComisionDTO>>> GetByComision(int idComision)
        {
            var lista = await _repo.GetByComisionAsync(idComision);

            var dtos = lista.Select(h => new HorarioComisionDTO
            {
                Id = h.Id,
                IdComision = h.IdComision,
                DiaSemana = h.DiaSemana,
                HoraInicio = h.HoraInicio,
                HoraFin = h.HoraFin,
                IdAula = h.IdAula ?? 0,
                AulaNombre = h.IdAulaNavigation != null ? h.IdAulaNavigation.Nombre : "Sin Aula",
                SedeNombre = h.IdAulaNavigation?.IdSedeNavigation?.Nombre ?? ""
            });

            return Ok(dtos);
        }

        [HttpPost]
        // 🔒 CANDADO REAL: Solo personal con permiso para editar horarios
        [Authorize(Roles = "COM_HORARIOS_EDIT")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // Para cuando el aula está ocupada
        public async Task<IActionResult> Create([FromBody] HorarioComisionDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                if (dto.HoraInicio >= dto.HoraFin)
                    return BadRequest(new { message = "La hora de inicio debe ser menor a la de fin." });

                var nuevo = new HorarioComision
                {
                    IdComision = dto.IdComision,
                    DiaSemana = dto.DiaSemana,
                    HoraInicio = dto.HoraInicio,
                    HoraFin = dto.HoraFin,
                    IdAula = dto.IdAula
                };

                await _repo.CreateAsync(nuevo);
                return Ok(new { message = "Horario asignado correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                // Devolvemos el mensaje de validación (Ej: Choque de aulas)
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        // 🔒 CANDADO REAL
        [Authorize(Roles = "COM_HORARIOS_EDIT")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _repo.DeleteAsync(id))
                return NoContent();

            return NotFound(new { message = "El horario no existe o ya fue eliminado." });
        }

        // ✅ ESTE ES EL MÉTODO CORRECTO (Con Sede)
        [HttpGet("visualizacion/periodo/{idPeriodo}/carrera/{idCarrera}/sede/{idSede}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVisualizacion(int idPeriodo, int idCarrera, int idSede)
        {
            var list = await _repo.GetHorariosByCarreraAndPeriodoAsync(idPeriodo, idCarrera, idSede);
            return Ok(list);
        }
    }
}