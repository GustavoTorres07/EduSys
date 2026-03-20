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
    public class CarrerasController : ControllerBase
    {
        private readonly ICarreraRepository _repo;

        public CarrerasController(ICarreraRepository repo)
        {
            _repo = repo;
        }

        // GET: api/carreras
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CarreraDTO>))]
        public async Task<ActionResult<List<CarreraDTO>>> Get()
        {
            var lista = await _repo.GetAllAsync();

            var listaDto = lista.Select(c => new CarreraDTO
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Titulo = c.Titulo,
                DuracionAnios = c.DuracionAnios,
                Activo = c.Activo ?? true,
                Descripcion = c.Descripcion,
                ResolucionMinisterial = c.ResolucionMinisterial,

                // Modalidades
                Modalidades = c.CarreraModalidads
                               .Where(cm => cm.IdModalidadNavigation.Activo == true)
                               .Select(cm => cm.IdModalidadNavigation.Nombre)
                               .ToList(),

                // Sedes
                NombresSedes = c.CarreraSedes
                                .Where(cs => cs.Activo == true && cs.IdSedeNavigation.Activo == true)
                                .Select(cs => cs.IdSedeNavigation.Nombre)
                                .ToList()
            }).ToList();

            return Ok(listaDto);
        }

        // GET: api/carreras/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CarreraDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CarreraDTO>> Get(int id)
        {
            var carrera = await _repo.GetByIdAsync(id);
            if (carrera == null) return NotFound(new { message = "Carrera no encontrada." });

            var dto = new CarreraDTO
            {
                Id = carrera.Id,
                Nombre = carrera.Nombre,
                Titulo = carrera.Titulo,
                DuracionAnios = carrera.DuracionAnios,
                Activo = carrera.Activo ?? true,
                Descripcion = carrera.Descripcion,
                ResolucionMinisterial = carrera.ResolucionMinisterial,

                Modalidades = carrera.CarreraModalidads
                                     .Where(cm => cm.IdModalidadNavigation.Activo == true)
                                     .Select(cm => cm.IdModalidadNavigation.Nombre)
                                     .ToList()
            };
            return Ok(dto);
        }

        // POST: api/carreras
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CarreraDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] CarreraDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _repo.ExisteNombreAsync(dto.Nombre))
            {
                return BadRequest(new { message = $"Ya existe una carrera llamada '{dto.Nombre}'." });
            }

            var nuevaCarrera = new Carrera
            {
                Nombre = dto.Nombre,
                Titulo = dto.Titulo,
                DuracionAnios = dto.DuracionAnios,
                Activo = true,
                Descripcion = dto.Descripcion,
                ResolucionMinisterial = dto.ResolucionMinisterial
            };

            var resultado = await _repo.CreateAsync(nuevaCarrera);

            var resultadoDto = new CarreraDTO
            {
                Id = resultado.Id,
                Nombre = resultado.Nombre,
                Activo = true
            };

            return CreatedAtAction(nameof(Get), new { id = resultadoDto.Id }, resultadoDto);
        }

        // PUT: api/carreras
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CarreraDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put([FromBody] CarreraDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _repo.ExisteNombreAsync(dto.Nombre, dto.Id))
            {
                return BadRequest(new { message = $"La carrera '{dto.Nombre}' ya existe." });
            }

            var carrera = new Carrera
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Titulo = dto.Titulo,
                DuracionAnios = dto.DuracionAnios,
                Activo = dto.Activo,
                Descripcion = dto.Descripcion,
                ResolucionMinisterial = dto.ResolucionMinisterial
            };

            var resultado = await _repo.UpdateAsync(carrera);
            if (!resultado) return NotFound(new { message = "Carrera no encontrada para actualizar." });

            return Ok(dto);
        }

        // DELETE: api/carreras/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var resultado = await _repo.DeleteAsync(id);
            if (!resultado) return NotFound(new { message = "Carrera no encontrada." });

            return NoContent();
        }

        // ---------------------------------------------------------
        // ENDPOINTS DE RELACIONES (SEDES Y MODALIDADES)
        // ---------------------------------------------------------

        [HttpGet("{id}/sedes")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
        public async Task<ActionResult<List<int>>> GetSedes(int id)
        {
            var listaIds = await _repo.GetSedesIdsByCarreraAsync(id);
            return Ok(listaIds);
        }

        [HttpPost("{id}/sedes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSedes(int id, [FromBody] List<int> idsSedes)
        {
            var resultado = await _repo.ActualizarSedesAsync(id, idsSedes);
            if (!resultado) return BadRequest(new { message = "No se pudo actualizar las sedes." });
            return Ok(new { message = "Sedes actualizadas correctamente." });
        }

        [HttpGet("{id}/modalidades")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
        public async Task<ActionResult<List<int>>> GetModalidades(int id)
        {
            var listaIds = await _repo.GetModalidadesIdsByCarreraAsync(id);
            return Ok(listaIds);
        }

        [HttpPost("{id}/modalidades")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateModalidades(int id, [FromBody] List<int> idsModalidades)
        {
            var resultado = await _repo.ActualizarModalidadesAsync(id, idsModalidades);
            if (!resultado) return BadRequest(new { message = "No se pudo actualizar las modalidades." });
            return Ok(new { message = "Modalidades actualizadas correctamente." });
        }

        // GET: api/carreras/por-sede/5
        [HttpGet("por-sede/{idSede}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CarreraDTO>))]
        public async Task<ActionResult<List<CarreraDTO>>> GetPorSede(int idSede)
        {
            var lista = await _repo.GetCarrerasPorSedeAsync(idSede);

            var listaDto = lista.Select(c => new CarreraDTO
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Titulo = c.Titulo,
                DuracionAnios = c.DuracionAnios,
                Activo = true
            }).ToList();

            return Ok(listaDto);
        }
    }
}