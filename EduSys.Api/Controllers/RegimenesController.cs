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
    public class RegimenesController : ControllerBase
    {
        private readonly IRegimenRepository _repo;

        public RegimenesController(IRegimenRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
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
        public async Task<IActionResult> Post([FromBody] RegimenDTO dto)
        {
            var nuevo = new Regimen { Nombre = dto.Nombre, Activo = true };
            await _repo.CreateAsync(nuevo);
            return Ok(nuevo.Id);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] RegimenDTO dto)
        {
            var regimen = new Regimen { Id = dto.Id, Nombre = dto.Nombre, Activo = dto.Activo };
            if (!await _repo.UpdateAsync(regimen)) return NotFound();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _repo.DeleteAsync(id)) return NotFound();
            return NoContent();
        }
    }
}