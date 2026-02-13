using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeriodosController : ControllerBase
    {
        private readonly IPeriodoRepository _repo;
        public PeriodosController(IPeriodoRepository repo) { _repo = repo; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _repo.GetAllAsync();
            return Ok(list.Select(p => new PeriodoAcademicoDTO
            {
                Id = p.Id,
                Nombre = p.Nombre,
                // CONVERSIÓN: De DateOnly (BD) a DateTime (DTO)
                FechaInicio = p.FechaInicio.ToDateTime(TimeOnly.MinValue),
                FechaFin = p.FechaFin.ToDateTime(TimeOnly.MinValue),
                Estado = p.Estado,
                Activo = p.Activo ?? true
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) return NotFound();

            return Ok(new PeriodoAcademicoDTO
            {
                Id = p.Id,
                Nombre = p.Nombre,
                // CONVERSIÓN: De DateOnly (BD) a DateTime (DTO)
                FechaInicio = p.FechaInicio.ToDateTime(TimeOnly.MinValue),
                FechaFin = p.FechaFin.ToDateTime(TimeOnly.MinValue),
                Estado = p.Estado,
                Activo = p.Activo ?? true
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(PeriodoAcademicoDTO dto)
        {
            if (dto.FechaInicio >= dto.FechaFin)
                return BadRequest("La fecha de fin debe ser posterior a la de inicio.");

            var nuevo = new PeriodoAcademico
            {
                Nombre = dto.Nombre,
                // CONVERSIÓN: De DateTime (DTO) a DateOnly (BD)
                FechaInicio = DateOnly.FromDateTime(dto.FechaInicio!.Value),
                FechaFin = DateOnly.FromDateTime(dto.FechaFin!.Value),
                Estado = "Abierto",
                Activo = true
            };

            await _repo.CreateAsync(nuevo);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update(PeriodoAcademicoDTO dto)
        {
            if (dto.FechaInicio >= dto.FechaFin)
                return BadRequest("La fecha de fin debe ser posterior a la de inicio.");

            var p = new PeriodoAcademico
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                // CONVERSIÓN: De DateTime (DTO) a DateOnly (BD)
                FechaInicio = DateOnly.FromDateTime(dto.FechaInicio!.Value),
                FechaFin = DateOnly.FromDateTime(dto.FechaFin!.Value),
                Estado = dto.Estado,
                Activo = dto.Activo
            };

            if (await _repo.UpdateAsync(p)) return Ok();
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _repo.DeleteAsync(id)) return Ok();
            return BadRequest();
        }
    }
}