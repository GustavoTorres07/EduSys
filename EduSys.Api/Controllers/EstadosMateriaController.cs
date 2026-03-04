using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protegemos el controlador para que solo usuarios logueados accedan
    public class EstadosMateriaController : ControllerBase
    {
        private readonly IEstadoMateriaRepository _repository;

        public EstadosMateriaController(IEstadoMateriaRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await _repository.ObtenerTodosAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var estado = await _repository.ObtenerPorIdAsync(id);
            if (estado == null) return NotFound("Estado no encontrado");
            return Ok(estado);
        }

        [HttpPost]
        public async Task<IActionResult> Post(EstadoMateriaDTO dto)
        {
            var resultado = await _repository.CrearAsync(dto);
            return Ok(resultado);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, EstadoMateriaDTO dto)
        {
            if (id != dto.Id) return BadRequest("El ID no coincide");

            var exito = await _repository.ActualizarAsync(dto);
            if (!exito) return NotFound("Estado no encontrado");

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var exito = await _repository.EliminarAsync(id);
            if (!exito) return NotFound("Estado no encontrado");

            return Ok();
        }
    }
}