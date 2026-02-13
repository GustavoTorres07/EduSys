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
    public class VentanasController : ControllerBase
    {
        private readonly IVentanaOperativaRepository _repo;

        public VentanasController(IVentanaOperativaRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lista = await _repo.GetAllAsync();
            var dtos = lista.Select(v => new VentanaOperativaDTO
            {
                Id = v.Id,
                IdPeriodo = v.IdPeriodo,
                NombrePeriodo = v.IdPeriodoNavigation.Nombre,
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
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<IActionResult> Create(VentanaOperativaDTO dto)
        {
            if (dto.FechaInicio == null || dto.FechaFin == null)
                return BadRequest("Fechas requeridas");

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
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _repo.DeleteAsync(id)) return Ok();
            return BadRequest();
        }

        // Endpoint para verificar si se puede inscribir (Usado por el Front)
        [HttpGet("verificar")]
        public async Task<IActionResult> Verificar(string accion, int periodo, int? carrera = null, int? sede = null)
        {
            bool habilitado = await _repo.IsHabilitadoAsync(accion, periodo, carrera, sede);
            return Ok(habilitado);
        }
    }
}