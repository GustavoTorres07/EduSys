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
    public class ModalidadesController : ControllerBase
    {
        private readonly IModalidadRepository _repo;

        public ModalidadesController(IModalidadRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ModalidadDTO>))]
        public async Task<ActionResult<List<ModalidadDTO>>> Get()
        {
            var lista = await _repo.GetAllAsync();
            var dtos = lista.Select(m => new ModalidadDTO
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Codigo = m.Codigo,
                Activo = m.Activo ?? true
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ModalidadDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ModalidadDTO>> Get(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null)
                return NotFound(new { message = "Modalidad no encontrada." });

            var dto = new ModalidadDTO
            {
                Id = item.Id,
                Nombre = item.Nombre,
                Codigo = item.Codigo,
                Activo = item.Activo ?? true
            };
            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒 Restringido a Gestión
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ModalidadDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] ModalidadDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _repo.ExisteNombreAsync(dto.Nombre))
            {
                return BadRequest(new { message = $"Ya existe la modalidad '{dto.Nombre}'." });
            }

            var nueva = new Modalidad
            {
                Nombre = dto.Nombre,
                Codigo = dto.Codigo,
                Activo = true
            };

            var resultado = await _repo.CreateAsync(nueva);
            dto.Id = resultado.Id;
            dto.Activo = true;

            return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
        }

        [HttpPut]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒 Restringido a Gestión
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ModalidadDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put([FromBody] ModalidadDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _repo.ExisteNombreAsync(dto.Nombre, dto.Id))
            {
                return BadRequest(new { message = $"Ya existe la modalidad '{dto.Nombre}'." });
            }

            var modalidad = new Modalidad
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Codigo = dto.Codigo,
                Activo = dto.Activo
            };

            var resultado = await _repo.UpdateAsync(modalidad);
            if (!resultado)
                return NotFound(new { message = "No se encontró la modalidad para actualizar." });

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒 Restringido a Gestión
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var resultado = await _repo.DeleteAsync(id);
            if (!resultado)
                return NotFound(new { message = "Modalidad no encontrada." });

            return NoContent();
        }
    }
}