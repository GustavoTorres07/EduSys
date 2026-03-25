using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 🔓 Candado Base: Requiere estar logueado (Cualquier rol).
    // Fundamental para que el Alumno pueda consultar su propio Id en GetByUsuario.
    [Authorize]
    public class AlumnosController : ControllerBase
    {
        private readonly IAlumnoRepository _alumnoRepo;

        public AlumnosController(IAlumnoRepository alumnoRepo)
        {
            _alumnoRepo = alumnoRepo;
        }

        // GET: api/alumnos
        [HttpGet]
        // 🔒 CANDADO REAL: Solo usuarios con permiso para gestionar alumnos ven la lista
        [Authorize(Roles = "ALU_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AlumnoListadoDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<AlumnoListadoDTO>>> GetAlumnos()
        {
            var lista = await _alumnoRepo.GetAllAsync();
            return Ok(lista);
        }

        // GET: api/alumnos/{id}
        [HttpGet("{id}")]
        // 🔒 CANDADO REAL: Solo usuarios con permiso pueden abrir el detalle administrativo
        [Authorize(Roles = "ALU_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AlumnoRequestDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AlumnoRequestDTO>> GetAlumnoById(int id)
        {
            var alumno = await _alumnoRepo.GetByIdAsync(id);
            if (alumno == null) return NotFound(new { message = "Alumno no encontrado." });

            return Ok(alumno);
        }

        // POST: api/alumnos
        [HttpPost]
        // 🔒 CANDADO REAL
        [Authorize(Roles = "ALU_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Crear([FromBody] AlumnoRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _alumnoRepo.CrearAsync(dto);
            if (result) return Ok(new { message = "Alumno creado correctamente." });

            return BadRequest(new { message = "No se pudo crear el alumno." });
        }

        // PUT: api/alumnos
        [HttpPut]
        // 🔒 CANDADO REAL
        [Authorize(Roles = "ALU_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Editar([FromBody] AlumnoRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _alumnoRepo.EditarAsync(dto);
            if (result) return Ok(new { message = "Alumno actualizado correctamente." });

            return BadRequest(new { message = "No se pudo actualizar el alumno." });
        }

        // ====================================================================
        // ✅ MÉTODO CRÍTICO PARA INSCRIPCIONES (ACTUALIZADO CON SEDE)
        // ====================================================================

        // GET: api/alumnos/usuario/{idUsuario}
        [HttpGet("usuario/{idUsuario}")]
        // 🔓 NO SE PONE ALU_ABM AQUÍ. Hereda el [Authorize] de la clase.
        // Permite que el alumno consulte sus propios datos para operar en la web.
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AlumnoDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AlumnoDTO>> GetByUsuario(int idUsuario)
        {
            // Delegamos la consulta al Repositorio
            var alumnoDto = await _alumnoRepo.GetByUsuarioAsync(idUsuario);

            if (alumnoDto == null)
                return NotFound(new { message = "No se encontró un perfil de alumno asociado a este usuario." });

            return Ok(alumnoDto);
        }
    }
}