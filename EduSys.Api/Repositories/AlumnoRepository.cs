using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class AlumnoRepository : IAlumnoRepository
    {
        private readonly EduSysDbContext _context;

        public AlumnoRepository(EduSysDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 1. LISTADO (Grilla Principal) - 🚀 OPTIMIZADO: Proyección Directa SQL
        // =========================================================
        public async Task<List<AlumnoListadoDTO>> GetAllAsync()
        {
            return await _context.Alumnos
                .AsNoTracking() // Libera memoria RAM
                .Where(a => a.Activo == true)
                .Select(a => new AlumnoListadoDTO
                {
                    IdAlumno = a.Id,
                    Legajo = a.Legajo,
                    Activo = a.Activo ?? true,

                    Dni = a.IdUsuarioNavigation.Dni,
                    NombreCompleto = a.IdUsuarioNavigation.Apellido + ", " + a.IdUsuarioNavigation.Nombre,
                    Email = a.IdUsuarioNavigation.Email,
                    FotoPerfilUrl = a.IdUsuarioNavigation.FotoPerfilUrl,

                    NombreCarrera = a.IdPlanActualNavigation != null
                                    ? a.IdPlanActualNavigation.IdCarreraNavigation.Nombre
                                    : "Sin Carrera Asignada",

                    NombrePlan = a.IdPlanActualNavigation != null
                                 ? a.IdPlanActualNavigation.Nombre
                                 : "Sin Plan",

                    NombreSede = a.IdSedeNavigation != null
                                 ? a.IdSedeNavigation.Nombre
                                 : "Sin Sede"
                })
                .ToListAsync();
        }

        // =========================================================
        // 2. OBTENER POR ID (Detalle completo + URLs)
        // =========================================================
        public async Task<AlumnoRequestDTO?> GetByIdAsync(int id)
        {
            var alumno = await _context.Alumnos
                .AsNoTracking() // Lectura limpia y rápida
                .Include(a => a.IdUsuarioNavigation)
                .Include(a => a.IdSedeNavigation)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alumno == null) return null;

            return new AlumnoRequestDTO
            {
                IdAlumno = alumno.Id,
                IdSede = alumno.IdSede ?? 0,

                Nombre = alumno.IdUsuarioNavigation.Nombre,
                Apellido = alumno.IdUsuarioNavigation.Apellido,
                Dni = alumno.IdUsuarioNavigation.Dni,
                Email = alumno.IdUsuarioNavigation.Email,
                Telefono = alumno.IdUsuarioNavigation.Telefono,
                Direccion = alumno.IdUsuarioNavigation.Direccion,
                Localidad = alumno.IdUsuarioNavigation.Localidad,
                FechaNacimiento = alumno.IdUsuarioNavigation.FechaNacimiento?.ToDateTime(TimeOnly.MinValue),
                IdNacionalidad = alumno.IdUsuarioNavigation.IdNacionalidad,
                EstadoCivil = alumno.IdUsuarioNavigation.EstadoCivil,
                Sexo = alumno.IdUsuarioNavigation.Sexo,
                LugarNacimiento = alumno.IdUsuarioNavigation.LugarNacimiento,
                FotoPerfilUrl = alumno.IdUsuarioNavigation.FotoPerfilUrl,
                NombreContactoEmergencia = alumno.IdUsuarioNavigation.NombreContactoEmergencia,
                TelefonoContactoEmergencia = alumno.IdUsuarioNavigation.TelefonoContactoEmergencia,

                Legajo = alumno.Legajo,
                IdPlanActual = alumno.IdPlanActual ?? 0,
                Ocupacion = alumno.Ocupacion,
                LugarTrabajo = alumno.LugarTrabajo,
                HorarioLaboral = alumno.HorarioLaboral,
                TituloSecundarioEntregado = alumno.TituloSecundarioEntregado,
                Activo = alumno.Activo ?? true,
                Observaciones = alumno.Observaciones,
                FechaIngreso = alumno.FechaIngreso?.ToDateTime(TimeOnly.MinValue),
                FechaEgreso = alumno.FechaEgreso?.ToDateTime(TimeOnly.MinValue),
                EstaBloqueado = alumno.EstaBloqueado,
                MotivoBloqueo = alumno.MotivoBloqueo,

                UrlDniFrente = alumno.UrlDniFrente,
                UrlDniDorso = alumno.UrlDniDorso,
                UrlTituloSecundario = alumno.UrlTituloSecundario,
                UrlAntecedentesPenales = alumno.UrlAntecedentesPenales,
                UrlValidacionIdentidad = alumno.UrlValidacionIdentidad
            };
        }

        // =========================================================
        // 3. CREAR ALUMNO (Transacción)
        // =========================================================
        public async Task<bool> CrearAsync(AlumnoRequestDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var usuario = new Usuario
                {
                    Nombre = dto.Nombre,
                    Apellido = dto.Apellido,
                    Dni = dto.Dni,
                    Email = dto.Email,
                    ClaveHash = BCrypt.Net.BCrypt.HashPassword(dto.Dni),
                    IdNacionalidad = dto.IdNacionalidad,
                    Activo = dto.Activo,
                    FechaRegistro = DateTime.Now,
                    Telefono = dto.Telefono,
                    Direccion = dto.Direccion,
                    Localidad = dto.Localidad,
                    FotoPerfilUrl = dto.FotoPerfilUrl,
                    NombreContactoEmergencia = dto.NombreContactoEmergencia,
                    TelefonoContactoEmergencia = dto.TelefonoContactoEmergencia,
                    EstadoCivil = dto.EstadoCivil,
                    Sexo = dto.Sexo,
                    LugarNacimiento = dto.LugarNacimiento,
                    FechaNacimiento = dto.FechaNacimiento.HasValue ? DateOnly.FromDateTime(dto.FechaNacimiento.Value) : null
                };

                // 🚀 CORRECCIÓN: Asignamos el rol a la nueva colección IdRols
                var rolAlumno = await _context.Rols.FindAsync(5);
                if (rolAlumno != null)
                {
                    usuario.IdRols.Add(rolAlumno);
                }

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var alumno = new Alumno
                {
                    IdUsuario = usuario.Id,
                    Legajo = string.IsNullOrEmpty(dto.Legajo) ? $"TMP-{dto.Dni}" : dto.Legajo,
                    IdPlanActual = dto.IdPlanActual,
                    IdSede = dto.IdSede > 0 ? dto.IdSede : (int?)null,
                    EstadoAcademico = dto.Activo ? "Activo" : "Inactivo",
                    Activo = dto.Activo,
                    EstaBloqueado = dto.EstaBloqueado ?? false,
                    MotivoBloqueo = dto.MotivoBloqueo,
                    TituloSecundarioEntregado = dto.TituloSecundarioEntregado,
                    Observaciones = dto.Observaciones,
                    Ocupacion = dto.Ocupacion,
                    HorarioLaboral = dto.HorarioLaboral,
                    LugarTrabajo = dto.LugarTrabajo,
                    FechaIngreso = dto.FechaIngreso.HasValue ? DateOnly.FromDateTime(dto.FechaIngreso.Value) : DateOnly.FromDateTime(DateTime.Now),
                    FechaEgreso = dto.FechaEgreso.HasValue ? DateOnly.FromDateTime(dto.FechaEgreso.Value) : null,
                    UrlDniFrente = dto.UrlDniFrente,
                    UrlDniDorso = dto.UrlDniDorso,
                    UrlTituloSecundario = dto.UrlTituloSecundario,
                    UrlAntecedentesPenales = dto.UrlAntecedentesPenales,
                    UrlValidacionIdentidad = dto.UrlValidacionIdentidad
                };

                _context.Alumnos.Add(alumno);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================================
        // 4. EDITAR ALUMNO
        // =========================================================
        public async Task<bool> EditarAsync(AlumnoRequestDTO dto)
        {
            var alumno = await _context.Alumnos
                .Include(a => a.IdUsuarioNavigation)
                .FirstOrDefaultAsync(a => a.Id == dto.IdAlumno);

            if (alumno == null) return false;

            var u = alumno.IdUsuarioNavigation;

            u.Nombre = dto.Nombre;
            u.Apellido = dto.Apellido;
            u.Dni = dto.Dni;
            u.Email = dto.Email;
            u.Telefono = dto.Telefono;
            u.Direccion = dto.Direccion;
            u.Localidad = dto.Localidad;
            u.Activo = dto.Activo;
            u.FotoPerfilUrl = dto.FotoPerfilUrl ?? u.FotoPerfilUrl; // Mantiene el original si llega null
            u.IdNacionalidad = dto.IdNacionalidad;
            u.NombreContactoEmergencia = dto.NombreContactoEmergencia;
            u.TelefonoContactoEmergencia = dto.TelefonoContactoEmergencia;
            u.EstadoCivil = dto.EstadoCivil;
            u.Sexo = dto.Sexo;
            u.LugarNacimiento = dto.LugarNacimiento;
            if (dto.FechaNacimiento.HasValue) u.FechaNacimiento = DateOnly.FromDateTime(dto.FechaNacimiento.Value);

            if (!string.IsNullOrEmpty(dto.Legajo)) alumno.Legajo = dto.Legajo;

            alumno.IdPlanActual = dto.IdPlanActual;
            if (dto.IdSede > 0) alumno.IdSede = dto.IdSede;

            alumno.TituloSecundarioEntregado = dto.TituloSecundarioEntregado;
            alumno.Observaciones = dto.Observaciones;
            alumno.Activo = dto.Activo;
            alumno.EstaBloqueado = dto.EstaBloqueado ?? false;
            alumno.MotivoBloqueo = dto.MotivoBloqueo;
            alumno.Ocupacion = dto.Ocupacion;
            alumno.HorarioLaboral = dto.HorarioLaboral;
            alumno.LugarTrabajo = dto.LugarTrabajo;
            alumno.FechaEgreso = dto.FechaEgreso.HasValue ? DateOnly.FromDateTime(dto.FechaEgreso.Value) : null;

            alumno.UrlDniFrente = dto.UrlDniFrente ?? alumno.UrlDniFrente;
            alumno.UrlDniDorso = dto.UrlDniDorso ?? alumno.UrlDniDorso;
            alumno.UrlTituloSecundario = dto.UrlTituloSecundario ?? alumno.UrlTituloSecundario;
            alumno.UrlAntecedentesPenales = dto.UrlAntecedentesPenales ?? alumno.UrlAntecedentesPenales;
            alumno.UrlValidacionIdentidad = dto.UrlValidacionIdentidad ?? alumno.UrlValidacionIdentidad;

            await _context.SaveChangesAsync();
            return true;
        }

        // =========================================================
        // 5. ELIMINAR (Baja Lógica)
        // =========================================================
        public async Task<bool> EliminarAsync(int id)
        {
            var alumno = await _context.Alumnos
                .Include(a => a.IdUsuarioNavigation)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alumno == null) return false;

            alumno.Activo = false;
            alumno.IdUsuarioNavigation.Activo = false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AlumnoDTO?> GetByUsuarioAsync(int idUsuario)
        {
            var alumno = await _context.Alumnos
                .AsNoTracking()
                .Include(a => a.IdUsuarioNavigation)
                .Include(a => a.IdSedeNavigation)
                .Include(a => a.IdPlanActualNavigation)
                    .ThenInclude(p => p.IdCarreraNavigation)
                .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);

            if (alumno == null) return null;

            return new AlumnoDTO
            {
                Id = alumno.Id,
                IdUsuario = alumno.IdUsuario,
                Nombre = alumno.IdUsuarioNavigation.Nombre,
                Apellido = alumno.IdUsuarioNavigation.Apellido,
                Dni = alumno.IdUsuarioNavigation.Dni,
                Email = alumno.IdUsuarioNavigation.Email,
                Legajo = alumno.Legajo,
                IdPlanActual = alumno.IdPlanActual ?? 0,
                NombrePlan = alumno.IdPlanActualNavigation?.Nombre ?? "Sin Plan",
                IdCarrera = alumno.IdPlanActualNavigation?.IdCarrera ?? 0,
                NombreCarrera = alumno.IdPlanActualNavigation?.IdCarreraNavigation?.Nombre ?? "Sin Carrera",
                IdSede = alumno.IdSede ?? 0,
                NombreSede = alumno.IdSedeNavigation?.Nombre ?? "Sin Sede Asignada"
            };
        }

        public async Task<List<AsistenciaMateriaDTO>> GetMisAsistenciasAsync(int idUsuario)
        {
            var cursadasDb = await _context.InscripcionCursada
                .AsNoTracking()
                .Where(i => i.IdAlumnoNavigation.IdUsuario == idUsuario && i.Estado != "Baja")
                .Select(i => new
                {
                    MateriaNombre = i.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    ComisionCodigo = i.IdComisionNavigation.Codigo,
                    CicloLectivo = i.IdComisionNavigation.IdPeriodoNavigation.FechaInicio.Year,
                    PorcentajeRequerido = i.IdComisionNavigation.IdPlanMateriaNavigation.PorcentajeAsistenciaRegularizar ?? 0,
                    AsistenciasDb = i.Asistencia.Select(a => new
                    {
                        Fecha = a.Fecha,
                        EstaPresente = a.EstaPresente,
                        EsJustificado = a.EsJustificado,
                        Observacion = a.Observacion
                    }).ToList()
                })
                .ToListAsync();

            var resultado = new List<AsistenciaMateriaDTO>();

            foreach (var cursada in cursadasDb)
            {
                var materiaDto = new AsistenciaMateriaDTO
                {
                    Materia = cursada.MateriaNombre ?? "Sin Nombre",
                    Comision = cursada.ComisionCodigo ?? "S/C",
                    CicloLectivo = cursada.CicloLectivo,
                    PorcentajeRequerido = cursada.PorcentajeRequerido,
                    Registros = new List<AsistenciaRegistroDTO>()
                };

                foreach (var asist in cursada.AsistenciasDb)
                {
                    string estadoTexto = "Ausente";
                    if (asist.EsJustificado) estadoTexto = "Justificado";
                    else if (asist.EstaPresente) estadoTexto = "Presente";

                    materiaDto.Registros.Add(new AsistenciaRegistroDTO
                    {
                        Fecha = asist.Fecha.ToDateTime(TimeOnly.MinValue),
                        Estado = estadoTexto,
                        Observacion = asist.Observacion
                    });
                }

                resultado.Add(materiaDto);
            }

            return resultado.OrderByDescending(a => a.CicloLectivo).ThenBy(a => a.Materia).ToList();
        }

        public async Task<Alumno> CrearAsync(Alumno alumno)
        {
            _context.Alumnos.Add(alumno);
            await _context.SaveChangesAsync();
            return alumno;
        }

        // =========================================================
        // 6. VALIDACIONES
        // =========================================================
        public async Task<bool> ExisteLegajoAsync(string legajo)
        {
            return await _context.Alumnos.AsNoTracking().AnyAsync(a => a.Legajo == legajo && a.Activo == true);
        }
    }
}