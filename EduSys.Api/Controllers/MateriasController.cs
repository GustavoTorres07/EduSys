using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 🔓 Candado Base: Lectura permitida para cualquier usuario logueado
    [Authorize]
    public class MateriasController : ControllerBase
    {
        private readonly IMateriaRepository _repo;

        public MateriasController(IMateriaRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MateriaDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<MateriaDTO>>> Get()
        {
            var lista = await _repo.GetAllAsync();
            var dtos = lista.Select(m => new MateriaDTO
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Codigo = m.Codigo,
                Activo = m.Activo ?? true
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MateriaDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<MateriaDTO>> Get(int id)
        {
            var m = await _repo.GetByIdAsync(id);

            if (m == null)
                return NotFound(new { message = "Materia no encontrada." });

            return Ok(new MateriaDTO
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Codigo = m.Codigo,
                Activo = m.Activo ?? true
            });
        }

        [HttpPost]
        // 🔒 CANDADO REAL: Solo personal con permiso de ABM de Materias
        [Authorize(Roles = "ACA_MATERIA_ABM")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MateriaDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] MateriaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _repo.ExisteCodigoAsync(dto.Codigo))
                return BadRequest(new { message = $"El código '{dto.Codigo}' ya se encuentra registrado." });

            var nueva = new Materia { Nombre = dto.Nombre, Codigo = dto.Codigo, Activo = true };
            var creada = await _repo.CreateAsync(nueva);

            // Actualizamos el DTO con el ID generado para devolverlo
            dto.Id = creada.Id;
            dto.Activo = true;

            return CreatedAtAction(nameof(Get), new { id = creada.Id }, dto);
        }

        [HttpPut]
        // 🔒 CANDADO REAL
        [Authorize(Roles = "ACA_MATERIA_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MateriaDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put([FromBody] MateriaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _repo.ExisteCodigoAsync(dto.Codigo, dto.Id))
                return BadRequest(new { message = $"El código '{dto.Codigo}' ya se encuentra registrado en otra materia." });

            var materia = new Materia { Id = dto.Id, Nombre = dto.Nombre, Codigo = dto.Codigo, Activo = dto.Activo };

            if (!await _repo.UpdateAsync(materia))
                return NotFound(new { message = "Materia no encontrada para actualizar." });

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        // 🔒 CANDADO REAL
        [Authorize(Roles = "ACA_MATERIA_ABM")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _repo.DeleteAsync(id))
                return NotFound(new { message = "Materia no encontrada." });

            return NoContent();
        }
    }
}