using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSys.Api.Repositories
{
    public class DocenteRepository : IDocenteRepository
    {
        private readonly EduSysDbContext _context;

        public DocenteRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<DocenteListadoDTO>> GetAllAsync()
        {
            // 🚀 OPTIMIZADO: Proyección directa y AsNoTracking implícito
            return await _context.Docentes
                .Where(d => d.Activo == true)
                .Select(d => new DocenteListadoDTO
                {
                    IdDocente = d.Id,
                    Legajo = d.Legajo,
                    NombreCompleto = $"{d.IdUsuarioNavigation.Apellido}, {d.IdUsuarioNavigation.Nombre}",
                    Dni = d.IdUsuarioNavigation.Dni,
                    Email = d.IdUsuarioNavigation.Email,
                    TituloAcademico = d.TituloAcademico ?? "Sin Título",
                    Activo = d.Activo ?? false
                })
                .ToListAsync();
        }

        public async Task<DocenteRequestDTO?> GetByIdAsync(int id)
        {
            // 🚀 OPTIMIZADO: AsNoTracking para lectura
            var docente = await _context.Docentes
                .AsNoTracking()
                .Include(d => d.IdUsuarioNavigation)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (docente == null) return null;

            return new DocenteRequestDTO
            {
                IdDocente = docente.Id,
                Nombre = docente.IdUsuarioNavigation.Nombre,
                Apellido = docente.IdUsuarioNavigation.Apellido,
                Dni = docente.IdUsuarioNavigation.Dni,
                Email = docente.IdUsuarioNavigation.Email,
                Telefono = docente.IdUsuarioNavigation.Telefono,
                Direccion = docente.IdUsuarioNavigation.Direccion,
                Localidad = docente.IdUsuarioNavigation.Localidad,
                FechaNacimiento = docente.IdUsuarioNavigation.FechaNacimiento?.ToDateTime(TimeOnly.MinValue),
                Sexo = docente.IdUsuarioNavigation.Sexo,
                EstadoCivil = docente.IdUsuarioNavigation.EstadoCivil,
                IdNacionalidad = docente.IdUsuarioNavigation.IdNacionalidad,
                LugarNacimiento = docente.IdUsuarioNavigation.LugarNacimiento,
                NombreContactoEmergencia = docente.IdUsuarioNavigation.NombreContactoEmergencia,
                TelefonoContactoEmergencia = docente.IdUsuarioNavigation.TelefonoContactoEmergencia,
                Legajo = docente.Legajo,
                TituloAcademico = docente.TituloAcademico ?? "",
                Activo = docente.Activo ?? true
            };
        }

        // ✅ NUEVA IMPLEMENTACIÓN: Dashboard Docente - 🚀 OPTIMIZACIÓN EXTREMA DE CONSULTA (Proyección en BD)
        public async Task<List<ComisionDocenteDTO>> GetMisComisionesAsync(int idUsuario)
        {
            var docenteId = await _context.Docentes
                .AsNoTracking()
                .Where(d => d.IdUsuario == idUsuario && d.Activo == true)
                .Select(d => d.Id)
                .FirstOrDefaultAsync();

            if (docenteId == 0) return new List<ComisionDocenteDTO>();

            // Al usar Select directo, evitamos los 7 .Include() pesados en memoria
            var comisionesDocente = await _context.DocenteComisions
                .AsNoTracking()
                .Where(dc => dc.IdDocente == docenteId && dc.Activo)
                .Select(dc => new ComisionDocenteDTO
                {
                    IdComision = dc.IdComision,
                    CodigoComision = dc.IdComisionNavigation.Codigo,
                    Materia = dc.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    Carrera = dc.IdComisionNavigation.IdPlanMateriaNavigation.IdPlanNavigation.IdCarreraNavigation.Nombre,
                    Sede = dc.IdComisionNavigation.IdSedeNavigation.Nombre,
                    Rol = dc.RolDocente,

                    // Contamos directo en BD
                    CantidadAlumnos = dc.IdComisionNavigation.InscripcionCursada.Count(i => i.Estado != "Baja"),

                    // Evaluamos los horarios seguros
                    Horario = dc.IdComisionNavigation.HorarioComisions.Any()
                        ? string.Join(", ", dc.IdComisionNavigation.HorarioComisions.Select(h => h.DiaSemana + " " + h.HoraInicio.ToString(@"hh\:mm") + "-" + h.HoraFin.ToString(@"hh\:mm")))
                        : "Sin asignar",

                    Aula = dc.IdComisionNavigation.HorarioComisions.FirstOrDefault() != null &&
                           dc.IdComisionNavigation.HorarioComisions.FirstOrDefault()!.IdAulaNavigation != null
                           ? dc.IdComisionNavigation.HorarioComisions.FirstOrDefault()!.IdAulaNavigation!.Nombre
                           : "Sin asignar",

                    Estado = dc.IdComisionNavigation.Estado ?? "Desconocido"
                })
                .OrderBy(c => c.Carrera)
                .ThenBy(c => c.Materia)
                .ToListAsync();

            return comisionesDocente;
        }

        public async Task<bool> CrearAsync(DocenteRequestDTO dto)
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
                    IdRol = 4, // Docente
                    IdNacionalidad = dto.IdNacionalidad,
                    Activo = dto.Activo,
                    FechaRegistro = DateTime.Now,
                    Telefono = dto.Telefono,
                    Direccion = dto.Direccion,
                    Localidad = dto.Localidad,
                    Sexo = dto.Sexo,
                    EstadoCivil = dto.EstadoCivil,
                    DebeCambiarPass = true,
                    LugarNacimiento = dto.LugarNacimiento,
                    NombreContactoEmergencia = dto.NombreContactoEmergencia,
                    TelefonoContactoEmergencia = dto.TelefonoContactoEmergencia,
                    FechaNacimiento = dto.FechaNacimiento.HasValue ? DateOnly.FromDateTime(dto.FechaNacimiento.Value) : null
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var docente = new Docente
                {
                    IdUsuario = usuario.Id,
                    Legajo = string.IsNullOrEmpty(dto.Legajo) ? $"DOC-{dto.Dni}" : dto.Legajo,
                    TituloAcademico = dto.TituloAcademico,
                    Activo = dto.Activo
                };

                _context.Docentes.Add(docente);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> EditarAsync(DocenteRequestDTO dto)
        {
            var docente = await _context.Docentes
                .Include(d => d.IdUsuarioNavigation)
                .FirstOrDefaultAsync(d => d.Id == dto.IdDocente);

            if (docente == null) return false;

            var u = docente.IdUsuarioNavigation;
            u.Nombre = dto.Nombre;
            u.Apellido = dto.Apellido;
            u.Dni = dto.Dni;
            u.Email = dto.Email;
            u.Telefono = dto.Telefono;
            u.Direccion = dto.Direccion;
            u.Localidad = dto.Localidad;
            u.Sexo = dto.Sexo;
            u.EstadoCivil = dto.EstadoCivil;
            u.Activo = dto.Activo;
            u.IdNacionalidad = dto.IdNacionalidad;
            u.LugarNacimiento = dto.LugarNacimiento;
            u.NombreContactoEmergencia = dto.NombreContactoEmergencia;
            u.TelefonoContactoEmergencia = dto.TelefonoContactoEmergencia;

            if (dto.FechaNacimiento.HasValue)
                u.FechaNacimiento = DateOnly.FromDateTime(dto.FechaNacimiento.Value);

            if (!string.IsNullOrEmpty(dto.Legajo))
                docente.Legajo = dto.Legajo;

            docente.TituloAcademico = dto.TituloAcademico;
            docente.Activo = dto.Activo;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var docente = await _context.Docentes
                .Include(d => d.IdUsuarioNavigation)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (docente == null) return false;

            docente.Activo = false;
            docente.IdUsuarioNavigation.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteLegajoAsync(string legajo)
        {
            return await _context.Docentes
                .AsNoTracking()
                .AnyAsync(d => d.Legajo == legajo && d.Activo == true);
        }

        public async Task<DocenteRequestDTO?> GetMiPerfilAsync(string emailUsuario)
        {
            // Buscamos al docente cuyo usuario asociado tenga el email del Token JWT
            var docente = await _context.Docentes
                .Include(d => d.IdUsuarioNavigation) // Incluimos la tabla de Usuario para traer Nombre, Email, etc.
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdUsuarioNavigation.Email == emailUsuario);

            if (docente == null) return null;

            // Mapeamos a DocenteRequestDTO (el mismo que usamos en la vista)
            return new DocenteRequestDTO
            {
                IdDocente = docente.Id,
                Nombre = docente.IdUsuarioNavigation.Nombre,
                Apellido = docente.IdUsuarioNavigation.Apellido,
                Dni = docente.IdUsuarioNavigation.Dni,
                Legajo = docente.Legajo,
                TituloAcademico = docente.TituloAcademico,
                Email = docente.IdUsuarioNavigation.Email,
                Telefono = docente.IdUsuarioNavigation.Telefono,
                // ✅ CORRECCIÓN AQUÍ: Si es null, por defecto lo ponemos en false (o true, según prefieras)
                Activo = docente.Activo ?? false
            };
        }
    }
}