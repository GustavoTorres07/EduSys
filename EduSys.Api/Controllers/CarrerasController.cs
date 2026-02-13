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
        public async Task<IActionResult> Get()
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

                // --- MAPEO DE MODALIDADES (Relación Tabla -> Lista Nombres) ---
                // Ahora traemos los nombres desde la tabla relacionada para mostrarlos en los Chips
                Modalidades = c.CarreraModalidads
                               .Where(cm => cm.IdModalidadNavigation.Activo == true)
                               .Select(cm => cm.IdModalidadNavigation.Nombre)
                               .ToList(),

                // --- CARGA DE SEDES ---
                NombresSedes = c.CarreraSedes
                                .Where(cs => cs.Activo == true && (cs.IdSedeNavigation.Activo == true))
                                .Select(cs => cs.IdSedeNavigation.Nombre)
                                .ToList()
            }).ToList();

            return Ok(listaDto);
        }

        // GET: api/carreras/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var carrera = await _repo.GetByIdAsync(id);
            if (carrera == null) return NotFound();

            var dto = new CarreraDTO
            {
                Id = carrera.Id,
                Nombre = carrera.Nombre,
                Titulo = carrera.Titulo,
                DuracionAnios = carrera.DuracionAnios,
                Activo = carrera.Activo ?? true,
                Descripcion = carrera.Descripcion,
                ResolucionMinisterial = carrera.ResolucionMinisterial,

                // --- MAPEO DE MODALIDADES ---
                Modalidades = carrera.CarreraModalidads
                                     .Where(cm => cm.IdModalidadNavigation.Activo == true)
                                     .Select(cm => cm.IdModalidadNavigation.Nombre)
                                     .ToList()
            };
            return Ok(dto);
        }

        // POST: api/carreras
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CarreraDTO dto)
        {
            // 1. VALIDACIÓN DE DUPLICADOS
            if (await _repo.ExisteNombreAsync(dto.Nombre))
            {
                return BadRequest($"Ya existe una carrera llamada '{dto.Nombre}'.");
            }

            // 2. CREACIÓN (La Carrera limpia, sin relaciones aún)
            var nuevaCarrera = new Carrera
            {
                Nombre = dto.Nombre,
                Titulo = dto.Titulo,
                DuracionAnios = dto.DuracionAnios,
                Activo = true,
                Descripcion = dto.Descripcion,
                ResolucionMinisterial = dto.ResolucionMinisterial
                // Nota: Ya no asignamos 'Modalidad' string aquí.
            };

            var resultado = await _repo.CreateAsync(nuevaCarrera);

            // 3. RESPUESTA
            // Nota: Al crear, la lista de modalidades vuelve vacía inicialmente. 
            // El frontend deberá llamar a UpdateModalidades si quiere asignarlas en el mismo paso,
            // o hacerlo en un segundo paso como con Sedes.
            var resultadoDto = new CarreraDTO
            {
                Id = resultado.Id,
                Nombre = resultado.Nombre,
                // ... resto de campos ...
                Activo = true
            };

            return CreatedAtAction(nameof(Get), new { id = resultadoDto.Id }, resultadoDto);
        }

        // PUT: api/carreras
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CarreraDTO dto)
        {
            if (await _repo.ExisteNombreAsync(dto.Nombre, dto.Id))
            {
                return BadRequest($"La carrera '{dto.Nombre}' ya existe.");
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
                // Nota: No tocamos relaciones aquí
            };

            var resultado = await _repo.UpdateAsync(carrera);
            if (!resultado) return NotFound();

            return Ok(dto);
        }

        // DELETE: api/carreras/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var resultado = await _repo.DeleteAsync(id);
            if (!resultado) return NotFound();

            return NoContent();
        }

        // ---------------------------------------------------------
        // ENDPOINTS DE RELACIONES (SEDES Y MODALIDADES)
        // ---------------------------------------------------------

        [HttpGet("{id}/sedes")]
        public async Task<IActionResult> GetSedes(int id)
        {
            var listaIds = await _repo.GetSedesIdsByCarreraAsync(id);
            return Ok(listaIds);
        }

        [HttpPost("{id}/sedes")]
        public async Task<IActionResult> UpdateSedes(int id, [FromBody] List<int> idsSedes)
        {
            var resultado = await _repo.ActualizarSedesAsync(id, idsSedes);
            if (!resultado) return BadRequest("No se pudo actualizar las sedes");
            return Ok();
        }

        // --- NUEVOS: IGUAL QUE SEDES PERO PARA MODALIDADES ---

        [HttpGet("{id}/modalidades")]
        public async Task<IActionResult> GetModalidades(int id)
        {
            // Necesitas agregar este método en tu ICarreraRepository
            var listaIds = await _repo.GetModalidadesIdsByCarreraAsync(id);
            return Ok(listaIds);
        }

        [HttpPost("{id}/modalidades")]
        public async Task<IActionResult> UpdateModalidades(int id, [FromBody] List<int> idsModalidades)
        {
            // Necesitas agregar este método en tu ICarreraRepository
            var resultado = await _repo.ActualizarModalidadesAsync(id, idsModalidades);
            if (!resultado) return BadRequest("No se pudo actualizar las modalidades");
            return Ok();
        }
        // GET: api/carreras/por-sede/5
        [HttpGet("por-sede/{idSede}")]
        [AllowAnonymous] // Permitimos anónimo porque es para la solicitud de ingreso pública
        public async Task<IActionResult> GetPorSede(int idSede)
        {
            var lista = await _repo.GetCarrerasPorSedeAsync(idSede);

            // Mapeamos a DTO para no devolver ciclos o datos innecesarios
            var listaDto = lista.Select(c => new CarreraDTO
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Titulo = c.Titulo,
                DuracionAnios = c.DuracionAnios,
                Activo = true,
                // Solo llenamos lo necesario para el combo
            }).ToList();

            return Ok(listaDto);
        }

    }
}