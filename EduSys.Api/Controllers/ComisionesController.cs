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
    public class ComisionesController : ControllerBase
    {
        private readonly IComisionRepository _comisionRepository;
        private readonly IInscripcionRepository _inscripcionRepository;

        public ComisionesController(IComisionRepository comisionRepository, IInscripcionRepository inscripcionRepository)
        {
            _comisionRepository = comisionRepository;
            _inscripcionRepository = inscripcionRepository;
        }

        // --- LECTURA ---

        [HttpGet]
        public async Task<ActionResult<List<ComisionDTO>>> Get()
        {
            var lista = await _comisionRepository.GetAllAsync();
            var dtos = lista.Select(c => MapToDTO(c)).ToList();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ComisionDTO>> Get(int id)
        {
            var comision = await _comisionRepository.GetByIdAsync(id);
            if (comision == null) return NotFound("Comisión no encontrada");
            return Ok(MapToDTO(comision));
        }

        [HttpGet("periodo/{idPeriodo}")]
        public async Task<ActionResult<List<ComisionDTO>>> GetByPeriodo(int idPeriodo)
        {
            var lista = await _comisionRepository.GetByPeriodoAsync(idPeriodo);
            return Ok(lista.Select(c => MapToDTO(c)).ToList());
        }

        // GET: api/Comisiones/periodo/{id}/carrera/{id}
        [HttpGet("periodo/{idPeriodo}/carrera/{idCarrera}")]
        public async Task<ActionResult<List<ComisionDTO>>> GetByPeriodoAndCarrera(int idPeriodo, int idCarrera, [FromQuery] int? idAlumno = null)
        {
            // 1. Traemos todo de la base de datos (aquí vienen los duplicados)
            var listaRaw = await _comisionRepository.GetByPeriodoAndCarreraAsync(idPeriodo, idCarrera);

            var dtos = new List<ComisionDTO>();

            // 2. ¡LA SOLUCIÓN! Agrupamos por Materia + Código (Nombre de comisión)
            // Esto hace que todas las "1º B" de "Programación I" se junten en un solo grupo.
            var grupos = listaRaw
                .GroupBy(c => new { c.IdPlanMateria, c.Codigo })
                .ToList();

            foreach (var grupo in grupos)
            {
                // Tomamos la primera comisión del grupo como referencia (para el ID, Cupos, etc)
                var principal = grupo.First();

                // 3. FUSIONAMOS HORARIOS: Recolectamos los horarios de TODAS las filas del grupo
                var horariosFusionados = grupo
                    .SelectMany(x => x.HorarioComisions)
                    .OrderBy(h => h.DiaSemana) // Opcional: ordenar
                    .ToList();

                // 4. FUSIONAMOS DOCENTES: Buscamos si ALGUNA de las filas tiene docente asignado
                var docenteFusionado = grupo
                    .SelectMany(x => x.DocenteComisions)
                    .FirstOrDefault(d => d.Activo == true);

                // Preparamos el texto de horarios
                string horariosTexto = horariosFusionados.Any()
                    ? string.Join(" / ", horariosFusionados.Select(h => $"{h.DiaSemana[..3]} {h.HoraInicio:hh\\:mm}-{h.HoraFin:hh\\:mm}"))
                    : "Sin horario";

                // Preparamos el nombre del profesor
                string nombreProfesor = "Profesor aún no asignado";
                if (docenteFusionado?.IdDocenteNavigation?.IdUsuarioNavigation != null)
                {
                    nombreProfesor = $"{docenteFusionado.IdDocenteNavigation.IdUsuarioNavigation.Apellido} {docenteFusionado.IdDocenteNavigation.IdUsuarioNavigation.Nombre}";
                }

                // 5. Creamos el DTO ÚNICO
                var dto = new ComisionDTO
                {
                    Id = principal.Id, // Usamos el ID de la primera (el alumno se inscribirá a esta)
                    Codigo = principal.Codigo,
                    IdPlanMateria = principal.IdPlanMateria,
                    MateriaNombre = principal.IdPlanMateriaNavigation?.IdMateriaNavigation?.Nombre ?? "Desconocida",
                    IdPeriodo = principal.IdPeriodo,
                    PeriodoNombre = principal.IdPeriodoNavigation?.Nombre ?? "",
                    IdSede = principal.IdSede,
                    SedeNombre = principal.IdSedeNavigation?.Nombre ?? "",
                    CupoMaximo = principal.CupoMaximo,

                    // Usamos los datos fusionados:
                    Turno = $"{principal.Turno} ({horariosTexto})",
                    Profesor = nombreProfesor,

                    Estado = principal.Estado,
                    AnioCursada = principal.IdPlanMateriaNavigation?.AnioCursada ?? 1
                };

                // Lógica de Correlativas (Igual que antes)
                if (idAlumno.HasValue)
                {
                    bool cumple = await _inscripcionRepository.ValidarCorrelativasAsync(idAlumno.Value, principal.IdPlanMateria);
                    dto.CumpleCorrelativas = cumple;
                    dto.MensajeError = cumple ? null : "No cumples con las correlativas requeridas.";
                }

                dtos.Add(dto);
            }

            return Ok(dtos);
        }

        [HttpGet("{id}/docentes")]
        public async Task<ActionResult<List<DocenteComisionListadoDTO>>> GetDocentes(int id)
        {
            var docentes = await _comisionRepository.GetDocentesPorComisionAsync(id);
            return Ok(docentes);
        }

        // --- ESCRITURA ---

        [HttpPost]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<ActionResult> Post(ComisionDTO dto)
        {
            var comision = new Comision
            {
                Codigo = dto.Codigo,
                IdPlanMateria = dto.IdPlanMateria,
                IdPeriodo = dto.IdPeriodo,
                IdSede = dto.IdSede,
                CupoMaximo = dto.CupoMaximo,
                Turno = dto.Turno,
                Estado = dto.Estado
            };
            var result = await _comisionRepository.CreateAsync(comision);
            return result ? Ok() : BadRequest("Error al crear");
        }

        [HttpPut]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<ActionResult> Put(ComisionDTO dto)
        {
            var comision = new Comision
            {
                Id = dto.Id,
                Codigo = dto.Codigo,
                IdPlanMateria = dto.IdPlanMateria,
                IdPeriodo = dto.IdPeriodo,
                IdSede = dto.IdSede,
                CupoMaximo = dto.CupoMaximo,
                Turno = dto.Turno,
                Estado = dto.Estado
            };
            var result = await _comisionRepository.UpdateAsync(comision);
            return result ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _comisionRepository.DeleteAsync(id);
            return result ? Ok() : NotFound();
        }

        [HttpPost("asignar-docente")]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<ActionResult<ResultadoOperacionDTO>> AsignarDocente(DocenteComisionRequestDTO dto)
        {
            try
            {
                await _comisionRepository.AsignarDocenteAsync(dto);
                // Éxito
                return Ok(new ResultadoOperacionDTO { Exito = true, Mensaje = "Docente asignado correctamente." });
            }
            catch (Exception ex)
            {
                // Error de Negocio (Conflicto Horario, Ya asignado, etc)
                // Devolvemos Ok con Exito = false para que el front lea el mensaje fácilmente
                return Ok(new ResultadoOperacionDTO
                {
                    Exito = false,
                    Mensaje = ex.Message // <--- AQUÍ VA TU MENSAJE DE CONFLICTO
                });
            }
        }



        [HttpDelete("docentes/{idAsignacion}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<ActionResult> DesasignarDocente(int idAsignacion)
        {
            return await _comisionRepository.DesasignarDocenteAsync(idAsignacion) ? Ok(new { message = "Eliminado" }) : NotFound();
        }

        // --- Helper Mapeo ---
        private static ComisionDTO MapToDTO(Comision c)
        {
            // Ordenar y formatear horarios
            var horariosOrdenados = c.HorarioComisions.OrderBy(h => h.DiaSemana).ToList();
            string horariosTexto = horariosOrdenados.Any()
                ? string.Join(" / ", horariosOrdenados.Select(h => $"{h.DiaSemana[..3]} {h.HoraInicio:hh\\:mm}-{h.HoraFin:hh\\:mm}"))
                : "Sin horario";

            // Lógica segura para obtener el nombre del profesor
            // ⚠️ CORRECCIÓN AQUÍ: Usamos "d.Activo == true" en lugar de "d.Activo"
            var docenteAsignado = c.DocenteComisions?.FirstOrDefault(d => d.Activo == true);

            string nombreProfesor = "Profesor aún no asignado";
            if (docenteAsignado?.IdDocenteNavigation?.IdUsuarioNavigation != null)
            {
                nombreProfesor = $"{docenteAsignado.IdDocenteNavigation.IdUsuarioNavigation.Apellido} {docenteAsignado.IdDocenteNavigation.IdUsuarioNavigation.Nombre}";
            }

            return new ComisionDTO
            {
                Id = c.Id,
                Codigo = c.Codigo,
                IdPlanMateria = c.IdPlanMateria,
                MateriaNombre = c.IdPlanMateriaNavigation?.IdMateriaNavigation?.Nombre ?? "Desconocida",
                IdPeriodo = c.IdPeriodo,
                PeriodoNombre = c.IdPeriodoNavigation?.Nombre ?? "",
                IdSede = c.IdSede,
                SedeNombre = c.IdSedeNavigation?.Nombre ?? "",
                CupoMaximo = c.CupoMaximo,
                Turno = $"{c.Turno} ({horariosTexto})",
                Estado = c.Estado,
                AnioCursada = c.IdPlanMateriaNavigation?.AnioCursada ?? 1,
                Profesor = nombreProfesor // Asignamos el nombre calculado
            };
        }
    }
}