using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlumnosController : ControllerBase
    {
        private readonly IAlumnoRepository _alumnoRepo;
        private readonly EduSysDbContext _context;

        public AlumnosController(IAlumnoRepository alumnoRepo, EduSysDbContext context)
        {
            _alumnoRepo = alumnoRepo;
            _context = context;
        }

        // GET: api/alumnos
        [HttpGet]
        public async Task<ActionResult<List<AlumnoListadoDTO>>> GetAlumnos()
        {
            var lista = await _alumnoRepo.GetAllAsync();
            return Ok(lista);
        }

        // GET: api/alumnos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AlumnoRequestDTO>> GetAlumnoById(int id)
        {
            var alumno = await _alumnoRepo.GetByIdAsync(id);
            if (alumno == null) return NotFound("Alumno no encontrado.");
            return Ok(alumno);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] AlumnoRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _alumnoRepo.CrearAsync(dto);
            if (result) return Ok();
            return BadRequest("No se pudo crear el alumno.");
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] AlumnoRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _alumnoRepo.EditarAsync(dto);
            if (result) return Ok();
            return BadRequest("No se pudo actualizar.");
        }

        // ====================================================================
        // ✅ MÉTODO CRÍTICO PARA INSCRIPCIONES (ACTUALIZADO CON SEDE)
        // ====================================================================
        [HttpGet("usuario/{idUsuario}")]
        public async Task<ActionResult<AlumnoDTO>> GetByUsuario(int idUsuario)
        {
            // Buscamos al alumno por su ID de Usuario (Identity)
            var alumno = await _context.Alumnos
                .Include(a => a.IdUsuarioNavigation)
                .Include(a => a.IdSedeNavigation) // ✅ NUEVO: Traemos la Sede
                .Include(a => a.IdPlanActualNavigation)
                    .ThenInclude(p => p.IdCarreraNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);

            if (alumno == null)
                return NotFound("No se encontró un perfil de alumno asociado a este usuario.");

            // Mapeamos manualmente para asegurar que todos los datos lleguen al front
            var dto = new AlumnoDTO
            {
                Id = alumno.Id,
                IdUsuario = alumno.IdUsuario,

                // Datos Personales
                Nombre = alumno.IdUsuarioNavigation.Nombre,
                Apellido = alumno.IdUsuarioNavigation.Apellido,
                Dni = alumno.IdUsuarioNavigation.Dni,
                Email = alumno.IdUsuarioNavigation.Email,
                Legajo = alumno.Legajo,

                // Datos Académicos
                IdPlanActual = alumno.IdPlanActual ?? 0,
                NombrePlan = alumno.IdPlanActualNavigation?.Nombre ?? "Sin Plan",

                IdCarrera = alumno.IdPlanActualNavigation?.IdCarrera ?? 0,
                NombreCarrera = alumno.IdPlanActualNavigation?.IdCarreraNavigation?.Nombre ?? "Sin Carrera",

                // ✅ NUEVOS DATOS DE SEDE (Para filtrar inscripción)
                IdSede = alumno.IdSede ?? 0,
                NombreSede = alumno.IdSedeNavigation?.Nombre ?? "Sin Sede Asignada"
            };

            return Ok(dto);
        }
    }
}