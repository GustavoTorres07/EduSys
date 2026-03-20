using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Lectura permitida para cualquier usuario logueado
    public class EstadosMateriaController : ControllerBase
    {
        private readonly IEstadoMateriaRepository _repository;

        public EstadosMateriaController(IEstadoMateriaRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<EstadoMateriaDTO>))]
        public async Task<ActionResult<List<EstadoMateriaDTO>>> Get()
        {
            var lista = await _repository.ObtenerTodosAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EstadoMateriaDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EstadoMateriaDTO>> Get(int id)
        {
            var estado = await _repository.ObtenerPorIdAsync(id);
            if (estado == null) return NotFound(new { message = "Estado de materia no encontrado." });

            return Ok(estado);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒 Solo gestión puede crear
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(EstadoMateriaDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] EstadoMateriaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var resultado = await _repository.CrearAsync(dto);

            // Retorna 201 Created apuntando al método Get con el nuevo ID
            return CreatedAtAction(nameof(Get), new { id = resultado.Id }, resultado);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒 Solo gestión puede editar
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EstadoMateriaDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(int id, [FromBody] EstadoMateriaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest(new { message = "El ID de la ruta no coincide con el del objeto." });

            var exito = await _repository.ActualizarAsync(dto);
            if (!exito) return NotFound(new { message = "Estado de materia no encontrado para actualizar." });

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒 Solo gestión puede eliminar
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var exito = await _repository.EliminarAsync(id);
            if (!exito) return NotFound(new { message = "Estado de materia no encontrado." });

            return NoContent();
        }
    }
}