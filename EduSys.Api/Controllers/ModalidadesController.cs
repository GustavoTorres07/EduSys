using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ModalidadesController : ControllerBase
    {
        private readonly IModalidadRepository _repo;

        public ModalidadesController(IModalidadRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
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
        public async Task<IActionResult> Get(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();

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
        public async Task<IActionResult> Post([FromBody] ModalidadDTO dto)
        {
            if (await _repo.ExisteNombreAsync(dto.Nombre))
            {
                return BadRequest($"Ya existe la modalidad '{dto.Nombre}'.");
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
        public async Task<IActionResult> Put([FromBody] ModalidadDTO dto)
        {
            if (await _repo.ExisteNombreAsync(dto.Nombre, dto.Id))
            {
                return BadRequest($"Ya existe la modalidad '{dto.Nombre}'.");
            }

            var modalidad = new Modalidad
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Codigo = dto.Codigo,
                Activo = dto.Activo
            };

            var resultado = await _repo.UpdateAsync(modalidad);
            if (!resultado) return NotFound();

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var resultado = await _repo.DeleteAsync(id);
            if (!resultado) return NotFound();
            return NoContent();
        }
    }
}