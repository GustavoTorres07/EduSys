using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly EduSysDbContext _context;

        public DashboardRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDTO> GetResumenAsync()
        {
            var dto = new DashboardDTO();

            // Ejecutamos las consultas paso a paso para respetar la regla de 
            // no-concurrencia del DbContext (Excelente decisión).
            dto.CantidadAlumnos = await _context.Alumnos.CountAsync(x => x.Activo == true);
            dto.CantidadDocentes = await _context.Docentes.CountAsync(x => x.Activo == true);
            dto.CantidadCarreras = await _context.Carreras.CountAsync(x => x.Activo == true);
            dto.CantidadSedes = await _context.Sedes.CountAsync(x => x.Activo == true);

            // Cargar últimos usuarios registrados (Simulando eventos recientes)
            dto.UltimosEventos = await _context.Usuarios
                .AsNoTracking() // 🚀 OPTIMIZADO: Lectura rápida
                .OrderByDescending(u => u.FechaRegistro)
                .Take(4)
                .Select(u => new EventoRecienteDTO
                {
                    Titulo = "Nuevo Usuario",
                    Descripcion = $"{u.Nombre} {u.Apellido} se registró.",
                    Fecha = u.FechaRegistro ?? DateTime.Now,
                    Tipo = "Success"
                })
                .ToListAsync();

            return dto;
        }
    }
}