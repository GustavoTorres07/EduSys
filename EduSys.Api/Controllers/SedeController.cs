using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 Protección base: Requiere login para leer
    public class SedesController : ControllerBase
    {
        private readonly IInfrastructureRepository _repo;

        public SedesController(IInfrastructureRepository repo)
        {
            _repo = repo;
        }


        [AllowAnonymous] 
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SedeDTO>))]
        public async Task<ActionResult<IEnumerable<SedeDTO>>> GetAll()
        {
            var list = await _repo.GetAllSedesAsync();

            return Ok(list.Select(s => new SedeDTO
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Direccion = s.Direccion,
                CodigoPostal = s.CodigoPostal,
                CantidadAulas = s.Aulas.Count(a => a.Activo == true),
                Activo = s.Activo ?? true
            }));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SedeDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SedeDTO>> GetById(int id)
        {
            var s = await _repo.GetSedeByIdAsync(id);

            if (s == null)
                return NotFound(new { message = "Sede no encontrada." });

            return Ok(new SedeDTO
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Direccion = s.Direccion,
                CodigoPostal = s.CodigoPostal,
                Activo = s.Activo ?? true
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] SedeDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _repo.CreateSedeAsync(new Sede
            {
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                CodigoPostal = dto.CodigoPostal,
                Activo = true
            });

            return Ok(new { message = "Sede creada exitosamente." });
        }

        [HttpPut]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] SedeDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var sede = new Sede
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                CodigoPostal = dto.CodigoPostal,
                Activo = dto.Activo
            };

            if (await _repo.UpdateSedeAsync(sede))
                return Ok(new { message = "Sede actualizada exitosamente." });

            return NotFound(new { message = "No se encontró la sede para actualizar." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _repo.DeleteSedeAsync(id))
                return NoContent();

            return NotFound(new { message = "La sede no existe o ya fue eliminada." });
        }

        // =========================================================
        // --- SUB-RECURSO: AULAS ---
        // =========================================================

        [HttpGet("{idSede}/aulas")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AulaDTO>))]
        public async Task<ActionResult<IEnumerable<AulaDTO>>> GetAulas(int idSede)
        {
            var list = await _repo.GetAulasBySedeAsync(idSede);

            return Ok(list.Select(a => new AulaDTO
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Capacidad = a.Capacidad,
                IdSede = a.IdSede,
                Activo = a.Activo ?? true
            }));
        }

        [HttpPost("aulas")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAula([FromBody] AulaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _repo.CreateAulaAsync(new Aula
            {
                Nombre = dto.Nombre,
                Capacidad = dto.Capacidad,
                IdSede = dto.IdSede,
                Activo = true
            });

            return Ok(new { message = "Aula creada exitosamente." });
        }

        [HttpPut("aulas")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAula([FromBody] AulaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var aula = new Aula
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Capacidad = dto.Capacidad,
                Activo = dto.Activo
            };

            if (await _repo.UpdateAulaAsync(aula))
                return Ok(new { message = "Aula actualizada exitosamente." });

            return NotFound(new { message = "No se encontró el aula para actualizar." });
        }

        [HttpDelete("aulas/{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAula(int id)
        {
            if (await _repo.DeleteAulaAsync(id))
                return NoContent();

            return NotFound(new { message = "El aula no existe o ya fue eliminada." });
        }
    }
}