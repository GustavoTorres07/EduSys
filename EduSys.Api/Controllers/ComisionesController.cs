using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 🔓 Candado Base: Lectura permitida para usuarios autenticados
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

        // --- LECTURA (Heredan [Authorize]) ---

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ComisionDTO>))]
        public async Task<ActionResult<List<ComisionDTO>>> Get()
        {
            var lista = await _comisionRepository.GetAllAsync();
            var dtos = lista.Select(c => MapToDTO(c)).ToList();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ComisionDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ComisionDTO>> Get(int id)
        {
            var comision = await _comisionRepository.GetByIdAsync(id);
            if (comision == null) return NotFound(new { message = "Comisión no encontrada" });
            return Ok(MapToDTO(comision));
        }

        [HttpGet("periodo/{idPeriodo}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ComisionDTO>))]
        public async Task<ActionResult<List<ComisionDTO>>> GetByPeriodo(int idPeriodo)
        {
            var lista = await _comisionRepository.GetByPeriodoAsync(idPeriodo);
            return Ok(lista.Select(c => MapToDTO(c)).ToList());
        }

        [HttpGet("periodo/{idPeriodo}/carrera/{idCarrera}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ComisionDTO>))]
        public async Task<ActionResult<List<ComisionDTO>>> GetByPeriodoAndCarrera(int idPeriodo, int idCarrera, [FromQuery] int? idAlumno = null)
        {
            var listaRaw = await _comisionRepository.GetByPeriodoAndCarreraAsync(idPeriodo, idCarrera);
            var dtos = new List<ComisionDTO>();

            var grupos = listaRaw
                .GroupBy(c => new { c.IdPlanMateria, c.Codigo })
                .ToList();

            foreach (var grupo in grupos)
            {
                var principal = grupo.First();

                var horariosFusionados = grupo
                    .SelectMany(x => x.HorarioComisions ?? new List<HorarioComision>())
                    .OrderBy(h => h.DiaSemana)
                    .ToList();

                var docenteFusionado = grupo
                    .SelectMany(x => x.DocenteComisions ?? new List<DocenteComision>())
                    .FirstOrDefault(d => d.Activo == true);

                string horariosTexto = horariosFusionados.Any()
                    ? string.Join(" / ", horariosFusionados.Select(h => $"{h.DiaSemana[..3]} {h.HoraInicio:hh\\:mm}-{h.HoraFin:hh\\:mm}"))
                    : "Sin horario";

                string nombreProfesor = docenteFusionado?.IdDocenteNavigation?.IdUsuarioNavigation != null
                    ? $"{docenteFusionado.IdDocenteNavigation.IdUsuarioNavigation.Apellido} {docenteFusionado.IdDocenteNavigation.IdUsuarioNavigation.Nombre}"
                    : "Profesor aún no asignado";

                var dto = new ComisionDTO
                {
                    Id = principal.Id,
                    Codigo = principal.Codigo,
                    IdPlanMateria = principal.IdPlanMateria,
                    MateriaNombre = principal.IdPlanMateriaNavigation?.IdMateriaNavigation?.Nombre ?? "Desconocida",
                    IdPeriodo = principal.IdPeriodo,
                    PeriodoNombre = principal.IdPeriodoNavigation?.Nombre ?? "",
                    IdSede = principal.IdSede,
                    SedeNombre = principal.IdSedeNavigation?.Nombre ?? "",
                    CupoMaximo = principal.CupoMaximo,
                    Turno = principal.Turno ?? "",
                    Horario = horariosTexto,
                    Profesor = nombreProfesor,
                    Estado = principal.Estado,
                    AnioCursada = principal.IdPlanMateriaNavigation?.AnioCursada ?? 1
                };

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

        [HttpGet("sede/{idSede}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ComisionDTO>))]
        public async Task<ActionResult<List<ComisionDTO>>> GetPorSede(int idSede)
        {
            var comisiones = await _comisionRepository.GetPorSedeAsync(idSede);
            return Ok(comisiones);
        }

        [HttpGet("{id}/docentes")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DocenteComisionListadoDTO>))]
        public async Task<ActionResult<List<DocenteComisionListadoDTO>>> GetDocentes(int id)
        {
            var docentes = await _comisionRepository.GetDocentesPorComisionAsync(id);
            return Ok(docentes);
        }

        // --- ESCRITURA ---

        [HttpPost]
        // 🔒 CANDADO REAL: ABM Comisiones
        [Authorize(Roles = "COM_COMISION_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoOperacionDTO))]
        public async Task<ActionResult<ResultadoOperacionDTO>> Post(ComisionDTO dto)
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

            return result
                ? Ok(new ResultadoOperacionDTO { Exito = true, Mensaje = "Comisión creada correctamente." })
                : BadRequest(new ResultadoOperacionDTO { Exito = false, Mensaje = "Error al crear la comisión." });
        }

        [HttpPut]
        // 🔒 CANDADO REAL
        [Authorize(Roles = "COM_COMISION_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoOperacionDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResultadoOperacionDTO>> Put(ComisionDTO dto)
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

            return result
                ? Ok(new ResultadoOperacionDTO { Exito = true, Mensaje = "Comisión actualizada." })
                : NotFound(new ResultadoOperacionDTO { Exito = false, Mensaje = "Comisión no encontrada." });
        }

        [HttpDelete("{id}")]
        // 🔒 CANDADO REAL
        [Authorize(Roles = "COM_COMISION_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoOperacionDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResultadoOperacionDTO>> Delete(int id)
        {
            var result = await _comisionRepository.DeleteAsync(id);

            return result
                ? Ok(new ResultadoOperacionDTO { Exito = true, Mensaje = "Comisión eliminada lógicamente." })
                : NotFound(new ResultadoOperacionDTO { Exito = false, Mensaje = "Comisión no encontrada." });
        }

        // --- ASIGNACIÓN DE DOCENTES ---

        [HttpPost("asignar-docente")]
        // 🔒 CANDADO REAL: Asignar docentes a comisiones
        [Authorize(Roles = "COM_COMISION_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoOperacionDTO))]
        public async Task<ActionResult<ResultadoOperacionDTO>> AsignarDocente(DocenteComisionRequestDTO dto)
        {
            try
            {
                await _comisionRepository.AsignarDocenteAsync(dto);
                return Ok(new ResultadoOperacionDTO { Exito = true, Mensaje = "Docente asignado correctamente." });
            }
            catch (Exception ex)
            {
                return Ok(new ResultadoOperacionDTO { Exito = false, Mensaje = ex.Message });
            }
        }

        [HttpDelete("docentes/{idAsignacion}")]
        // 🔒 CANDADO REAL
        [Authorize(Roles = "COM_COMISION_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoOperacionDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResultadoOperacionDTO>> DesasignarDocente(int idAsignacion)
        {
            var result = await _comisionRepository.DesasignarDocenteAsync(idAsignacion);

            return result
                ? Ok(new ResultadoOperacionDTO { Exito = true, Mensaje = "Docente desasignado." })
                : NotFound(new ResultadoOperacionDTO { Exito = false, Mensaje = "Asignación no encontrada." });
        }

        // --- Helper Mapeo ---
        private static ComisionDTO MapToDTO(Comision c)
        {
            var horariosOrdenados = c.HorarioComisions?.OrderBy(h => h.DiaSemana).ToList() ?? new List<HorarioComision>();

            string horariosTexto = horariosOrdenados.Any()
                ? string.Join(" / ", horariosOrdenados.Select(h => $"{h.DiaSemana[..3]} {h.HoraInicio:hh\\:mm}-{h.HoraFin:hh\\:mm}"))
                : "Sin horario";

            var docenteAsignado = c.DocenteComisions?.FirstOrDefault(d => d.Activo == true);

            string nombreProfesor = docenteAsignado?.IdDocenteNavigation?.IdUsuarioNavigation != null
                ? $"{docenteAsignado.IdDocenteNavigation.IdUsuarioNavigation.Apellido} {docenteAsignado.IdDocenteNavigation.IdUsuarioNavigation.Nombre}"
                : "Profesor aún no asignado";

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

                Turno = c.Turno ?? "",
                Horario = horariosTexto,

                Estado = c.Estado,
                AnioCursada = c.IdPlanMateriaNavigation?.AnioCursada ?? 1,
                Profesor = nombreProfesor
            };
        }
    }
}