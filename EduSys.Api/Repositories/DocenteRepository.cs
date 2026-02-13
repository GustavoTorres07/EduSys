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

        // ... (Los métodos GetAllAsync, GetByIdAsync, CrearAsync, EditarAsync, EliminarAsync, ExisteLegajoAsync SE MANTIENEN IGUAL) ...
        // ... (Solo copia y pega tus métodos existentes aquí si no los quieres reescribir) ...

        public async Task<List<DocenteListadoDTO>> GetAllAsync()
        {
            return await _context.Docentes
                .Include(d => d.IdUsuarioNavigation)
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

        // ... (Mantén tus métodos GetByIdAsync, CrearAsync, EditarAsync, EliminarAsync, ExisteLegajoAsync aquí) ...
        // Para ahorrar espacio en la respuesta asumo que mantienes tu código original arriba

        // ✅ NUEVA IMPLEMENTACIÓN: Dashboard Docente
        public async Task<List<ComisionDocenteDTO>> GetMisComisionesAsync(int idUsuario)
        {
            // 1. Identificar al Docente por su ID de Usuario
            var docente = await _context.Docentes
                .FirstOrDefaultAsync(d => d.IdUsuario == idUsuario && d.Activo == true);

            if (docente == null) return new List<ComisionDocenteDTO>();

            // 2. Buscar sus comisiones ACTIVAS (Estado 'Abierta')
            var comisiones = await _context.DocenteComisions
                .Include(dc => dc.IdComisionNavigation)
                    .ThenInclude(c => c.IdPlanMateriaNavigation)
                        .ThenInclude(pm => pm.IdMateriaNavigation) // Materia
                .Include(dc => dc.IdComisionNavigation)
                    .ThenInclude(c => c.IdPlanMateriaNavigation)
                        .ThenInclude(pm => pm.IdPlanNavigation)
                            .ThenInclude(p => p.IdCarreraNavigation) // Carrera
                .Include(dc => dc.IdComisionNavigation)
                    .ThenInclude(c => c.IdSedeNavigation) // Sede
                .Include(dc => dc.IdComisionNavigation)
                    .ThenInclude(c => c.HorarioComisions) // Horarios
                        .ThenInclude(h => h.IdAulaNavigation) // Aulas
                .Include(dc => dc.IdComisionNavigation)
                    .ThenInclude(c => c.InscripcionCursada) // Inscripciones para contar alumnos
                .Where(dc => dc.IdDocente == docente.Id
                             && dc.Activo == true
                             && dc.IdComisionNavigation.Estado == "Abierta")
                .ToListAsync();

            // 3. Mapear a DTO
            var resultado = comisiones.Select(dc => new ComisionDocenteDTO
            {
                IdComision = dc.IdComision,
                Materia = dc.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                Carrera = dc.IdComisionNavigation.IdPlanMateriaNavigation.IdPlanNavigation.IdCarreraNavigation.Nombre,
                CodigoComision = $"{dc.IdComisionNavigation.Codigo} ({dc.IdComisionNavigation.Turno})",
                Sede = dc.IdComisionNavigation.IdSedeNavigation.Nombre,
                Rol = dc.RolDocente,
                Estado = dc.IdComisionNavigation.Estado ?? "Desconocido",

                // Contar solo inscripciones activas (no bajas)
                CantidadAlumnos = dc.IdComisionNavigation.InscripcionCursada.Count(i => i.Estado != "Baja"),

                // Formatear horarios
                Horario = string.Join(" / ", dc.IdComisionNavigation.HorarioComisions.Select(h =>
                    $"{h.DiaSemana.Substring(0, 3)} {h.HoraInicio:hh\\:mm}-{h.HoraFin:hh\\:mm}")),

                // Aula
                Aula = dc.IdComisionNavigation.HorarioComisions.FirstOrDefault()?.IdAulaNavigation?.Nombre ?? "Sin Aula"
            }).ToList();

            return resultado;
        }

        // ... (Aquí sigue el resto de tus métodos: GetByIdAsync, CrearAsync, etc.) ...

        // --- AGREGO TUS MÉTODOS ORIGINALES PARA QUE EL ARCHIVO TE QUEDE COMPLETO SI COPIAS TODO ---
        public async Task<DocenteRequestDTO?> GetByIdAsync(int id)
        {
            var docente = await _context.Docentes
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
            var docente = await _context.Docentes.Include(d => d.IdUsuarioNavigation).FirstOrDefaultAsync(d => d.Id == id);
            if (docente == null) return false;
            docente.Activo = false;
            docente.IdUsuarioNavigation.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteLegajoAsync(string legajo)
        {
            return await _context.Docentes.AnyAsync(d => d.Legajo == legajo && d.Activo == true);
        }
    }
}