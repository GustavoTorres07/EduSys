using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere estar logueado
    public class MesasFinalesController : ControllerBase
    {
        private readonly IMesaFinalRepository _repo;

        public MesasFinalesController(IMesaFinalRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador, Secretaria Academica, Coordinador")]
        public async Task<ActionResult<List<MesaFinalDTO>>> Get()
        {
            return Ok(await _repo.GetAllAsync());
        }

        [HttpGet("periodo/{idPeriodo}")]
        public async Task<ActionResult<List<MesaFinalDTO>>> GetByPeriodo(int idPeriodo)
        {
            return Ok(await _repo.GetByPeriodoAsync(idPeriodo));
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<ActionResult<ResultadoOperacionDTO>> Post(MesaFinalRequestDTO dto)
        {
            var result = await _repo.CreateAsync(dto);
            if (result.Exito) return Ok(result);
            return BadRequest(result);
        }

        [HttpPut]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<ActionResult<ResultadoOperacionDTO>> Put(MesaFinalRequestDTO dto)
        {
            var result = await _repo.UpdateAsync(dto);
            if (result.Exito) return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<ActionResult<ResultadoOperacionDTO>> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);
            if (result.Exito) return Ok(result);
            return BadRequest(result);
        }

        // ==================================================================
        // ENDPOINTS DE CALIFICACIONES Y ACTAS
        // ==================================================================

        [HttpGet("{idMesa}/acta")]
        public async Task<IActionResult> GetActaMesaFinal(int idMesa)
        {
            var acta = await _repo.GetActaMesaFinalAsync(idMesa);
            if (acta == null) return NotFound("La mesa no existe.");
            return Ok(acta);
        }

        [HttpPut("inscripcion/{idInscripcion}/nota")]
        public async Task<IActionResult> GuardarNotaFinal(int idInscripcion, [FromBody] decimal? nota)
        {
            var exito = await _repo.GuardarNotaFinalAsync(idInscripcion, nota);
            if (exito) return Ok();
            return BadRequest("No se pudo guardar la nota.");
        }

        [HttpPost("{idMesa}/cerrar-acta")]
        public async Task<IActionResult> CerrarActaFinal(int idMesa, [FromBody] CierreActaDTO dto) // Reutilizamos tu CierreActaDTO
        {
            var exito = await _repo.CerrarActaFinalAsync(idMesa, dto.Libro, dto.Folio);
            if (exito) return Ok(new { message = "Acta cerrada exitosamente." });
            return BadRequest("No se pudo cerrar el acta. Verifica que no esté cerrada previamente.");
        }
    }
}