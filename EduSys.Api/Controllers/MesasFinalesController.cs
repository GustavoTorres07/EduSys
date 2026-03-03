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
    }
}