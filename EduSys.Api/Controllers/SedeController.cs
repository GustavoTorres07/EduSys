using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SedesController : ControllerBase
    {
        private readonly IInfrastructureRepository _repo;
        public SedesController(IInfrastructureRepository repo) { _repo = repo; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _repo.GetAllSedesAsync();

            // CORRECCIÓN AQUÍ:
            return Ok(list.Select(s => new SedeDTO
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Direccion = s.Direccion,
                CodigoPostal = s.CodigoPostal,
                CantidadAulas = s.Aulas.Count(a => a.Activo == true),

                // ¡ESTA LÍNEA FALTABA! Sin ella, siempre asume True
                Activo = s.Activo ?? true
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _repo.GetSedeByIdAsync(id);
            if (s == null) return NotFound();

            return Ok(new SedeDTO
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Direccion = s.Direccion,
                CodigoPostal = s.CodigoPostal,
                // AQUÍ TAMBIÉN FALTABA
                Activo = s.Activo ?? true
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(SedeDTO dto)
        {
            await _repo.CreateSedeAsync(new Sede
            {
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                CodigoPostal = dto.CodigoPostal,
                Activo = true
            });
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update(SedeDTO dto)
        {
            // IMPORTANTE: Pasamos el estado Activo que viene del DTO
            if (await _repo.UpdateSedeAsync(new Sede
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                CodigoPostal = dto.CodigoPostal,
                Activo = dto.Activo // <--- Asegurarnos de enviar el estado actual
            })) return Ok();

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _repo.DeleteSedeAsync(id)) return Ok();
            return BadRequest();
        }

        // --- SUB-RECURSO: AULAS ---
        [HttpGet("{idSede}/aulas")]
        public async Task<IActionResult> GetAulas(int idSede)
        {
            var list = await _repo.GetAulasBySedeAsync(idSede);
            return Ok(list.Select(a => new AulaDTO
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Capacidad = a.Capacidad,
                IdSede = a.IdSede,
                Activo = a.Activo ?? true // Mapeamos también el estado del aula
            }));
        }

        [HttpPost("aulas")]
        public async Task<IActionResult> CreateAula(AulaDTO dto)
        {
            await _repo.CreateAulaAsync(new Aula
            {
                Nombre = dto.Nombre,
                Capacidad = dto.Capacidad,
                IdSede = dto.IdSede,
                Activo = true
            });
            return Ok();
        }

        [HttpPut("aulas")]
        public async Task<IActionResult> UpdateAula(AulaDTO dto)
        {
            if (await _repo.UpdateAulaAsync(new Aula
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Capacidad = dto.Capacidad,
                Activo = dto.Activo // Permitimos actualizar estado desde edición
            })) return Ok();

            return BadRequest();
        }

        [HttpDelete("aulas/{id}")]
        public async Task<IActionResult> DeleteAula(int id)
        {
            if (await _repo.DeleteAulaAsync(id)) return Ok();
            return BadRequest();
        }
    }
}