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
    public class MateriasController : ControllerBase
    {
        private readonly IMateriaRepository _repo;

        public MateriasController(IMateriaRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
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
        public async Task<IActionResult> Get(int id)
        {
            var m = await _repo.GetByIdAsync(id);
            if (m == null) return NotFound();
            return Ok(new MateriaDTO { Id = m.Id, Nombre = m.Nombre, Codigo = m.Codigo, Activo = m.Activo ?? true });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] MateriaDTO dto)
        {
            if (await _repo.ExisteCodigoAsync(dto.Codigo))
                return BadRequest($"El código '{dto.Codigo}' ya existe.");

            var nueva = new Materia { Nombre = dto.Nombre, Codigo = dto.Codigo, Activo = true };
            var creada = await _repo.CreateAsync(nueva);

            return CreatedAtAction(nameof(Get), new { id = creada.Id }, dto);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] MateriaDTO dto)
        {
            if (await _repo.ExisteCodigoAsync(dto.Codigo, dto.Id))
                return BadRequest($"El código '{dto.Codigo}' ya existe.");

            var materia = new Materia { Id = dto.Id, Nombre = dto.Nombre, Codigo = dto.Codigo, Activo = dto.Activo };
            if (!await _repo.UpdateAsync(materia)) return NotFound();

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _repo.DeleteAsync(id)) return NotFound();
            return NoContent();
        }
    }
}