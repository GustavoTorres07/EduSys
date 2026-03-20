using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Lectura permitida para cualquier usuario autenticado
    public class RegimenesController : ControllerBase
    {
        private readonly IRegimenRepository _repo;

        public RegimenesController(IRegimenRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RegimenDTO>))]
        public async Task<ActionResult<List<RegimenDTO>>> Get()
        {
            var lista = await _repo.GetAllAsync();
            var dtos = lista.Select(r => new RegimenDTO
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Activo = r.Activo
            }).ToList();

            return Ok(dtos);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RegimenDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] RegimenDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var nuevo = new Regimen { Nombre = dto.Nombre, Activo = true };
            await _repo.CreateAsync(nuevo);

            // Completamos el DTO con el ID generado en la base de datos
            dto.Id = nuevo.Id;
            dto.Activo = true;

            return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
        }

        [HttpPut]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegimenDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put([FromBody] RegimenDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var regimen = new Regimen { Id = dto.Id, Nombre = dto.Nombre, Activo = dto.Activo };

            if (!await _repo.UpdateAsync(regimen))
                return NotFound(new { message = "Régimen no encontrado para actualizar." });

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _repo.DeleteAsync(id))
                return NotFound(new { message = "Régimen no encontrado." });

            return NoContent();
        }
    }
}