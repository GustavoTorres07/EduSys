using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 Protección base: Requiere login para leer periodos
    public class PeriodosController : ControllerBase
    {
        private readonly IPeriodoRepository _repo;
        public PeriodosController(IPeriodoRepository repo) { _repo = repo; }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PeriodoAcademicoDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<PeriodoAcademicoDTO>>> GetAll()
        {
            var list = await _repo.GetAllAsync();
            return Ok(list.Select(p => new PeriodoAcademicoDTO
            {
                Id = p.Id,
                Nombre = p.Nombre,
                FechaInicio = p.FechaInicio.ToDateTime(TimeOnly.MinValue),
                FechaFin = p.FechaFin.ToDateTime(TimeOnly.MinValue),
                Estado = p.Estado,
                Activo = p.Activo ?? true
            }));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PeriodoAcademicoDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PeriodoAcademicoDTO>> GetById(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) return NotFound(new { message = "Periodo académico no encontrado." });

            return Ok(new PeriodoAcademicoDTO
            {
                Id = p.Id,
                Nombre = p.Nombre,
                FechaInicio = p.FechaInicio.ToDateTime(TimeOnly.MinValue),
                FechaFin = p.FechaFin.ToDateTime(TimeOnly.MinValue),
                Estado = p.Estado,
                Activo = p.Activo ?? true
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒 Solo gestión crea periodos
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] PeriodoAcademicoDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!dto.FechaInicio.HasValue || !dto.FechaFin.HasValue)
                return BadRequest(new { message = "Las fechas de inicio y fin son obligatorias." });

            if (dto.FechaInicio >= dto.FechaFin)
                return BadRequest(new { message = "La fecha de fin debe ser posterior a la de inicio." });

            var nuevo = new PeriodoAcademico
            {
                Nombre = dto.Nombre,
                FechaInicio = DateOnly.FromDateTime(dto.FechaInicio.Value),
                FechaFin = DateOnly.FromDateTime(dto.FechaFin.Value),
                Estado = "Abierto",
                Activo = true
            };

            await _repo.CreateAsync(nuevo);
            return Ok(new { message = "Periodo académico creado correctamente." });
        }

        [HttpPut]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒 Solo gestión modifica periodos
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromBody] PeriodoAcademicoDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!dto.FechaInicio.HasValue || !dto.FechaFin.HasValue)
                return BadRequest(new { message = "Las fechas de inicio y fin son obligatorias." });

            if (dto.FechaInicio >= dto.FechaFin)
                return BadRequest(new { message = "La fecha de fin debe ser posterior a la de inicio." });

            var p = new PeriodoAcademico
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                FechaInicio = DateOnly.FromDateTime(dto.FechaInicio.Value),
                FechaFin = DateOnly.FromDateTime(dto.FechaFin.Value),
                Estado = dto.Estado,
                Activo = dto.Activo
            };

            if (await _repo.UpdateAsync(p))
                return Ok(new { message = "Periodo académico actualizado correctamente." });

            return BadRequest(new { message = "No se pudo actualizar el periodo académico." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒 Solo gestión elimina periodos
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _repo.DeleteAsync(id))
                return NoContent();

            return NotFound(new { message = "El periodo académico no existe o ya fue eliminado." });
        }
    }
}