using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 Protección base
    public class VentanasController : ControllerBase
    {
        private readonly IVentanaOperativaRepository _repo;

        public VentanasController(IVentanaOperativaRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VentanaOperativaDTO>))]
        public async Task<ActionResult<IEnumerable<VentanaOperativaDTO>>> GetAll()
        {
            var lista = await _repo.GetAllAsync();

            var dtos = lista.Select(v => new VentanaOperativaDTO
            {
                Id = v.Id,
                IdPeriodo = v.IdPeriodo,
                NombrePeriodo = v.IdPeriodoNavigation?.Nombre ?? "Sin Periodo",
                TipoAccion = v.TipoAccion,
                FechaInicio = v.FechaInicio,
                FechaFin = v.FechaFin,
                IdCarrera = v.IdCarrera,
                NombreCarrera = v.IdCarreraNavigation?.Nombre ?? "Todas",
                IdSede = v.IdSede,
                NombreSede = v.IdSedeNavigation?.Nombre ?? "Todas"
            });

            return Ok(dtos);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒 Solo gestión
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] VentanaOperativaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (dto.FechaInicio == null || dto.FechaFin == null)
                return BadRequest(new { message = "Las fechas de inicio y fin son obligatorias." });

            if (dto.FechaInicio >= dto.FechaFin)
                return BadRequest(new { message = "La fecha de fin debe ser posterior a la fecha de inicio." });

            var nueva = new VentanaOperativa
            {
                IdPeriodo = dto.IdPeriodo,
                TipoAccion = dto.TipoAccion,
                FechaInicio = dto.FechaInicio.Value,
                FechaFin = dto.FechaFin.Value,
                IdCarrera = dto.IdCarrera == 0 ? null : dto.IdCarrera, // 0 significa 'Todas'
                IdSede = dto.IdSede == 0 ? null : dto.IdSede
            };

            await _repo.CreateAsync(nueva);
            return Ok(new { message = "Ventana operativa creada exitosamente." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒 Solo gestión
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _repo.DeleteAsync(id))
                return NoContent();

            return NotFound(new { message = "La ventana operativa no existe o ya fue eliminada." });
        }

        // Endpoint para verificar si se puede inscribir (Usado por el Front)
        [HttpGet("verificar")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<ActionResult<bool>> Verificar(
            [FromQuery] string accion,
            [FromQuery] int periodo,
            [FromQuery] int? carrera = null,
            [FromQuery] int? sede = null)
        {
            bool habilitado = await _repo.IsHabilitadoAsync(accion, periodo, carrera, sede);
            return Ok(habilitado);
        }
    }
}