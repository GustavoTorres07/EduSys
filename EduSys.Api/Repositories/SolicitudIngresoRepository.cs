using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class SolicitudIngresoRepository : ISolicitudIngresoRepository
    {
        private readonly EduSysDbContext _context;

        public SolicitudIngresoRepository(EduSysDbContext context)
        {
            _context = context;
        }

        // 1. Listar Pendientes (Con Sede incluida)
        public async Task<List<SolicitudIngreso>> GetPendientesAsync()
        {
            // 🚀 OPTIMIZADO: AsNoTracking para consultas de gestión
            return await _context.SolicitudIngresos
                .AsNoTracking()
                .Include(s => s.IdCarreraInteresNavigation)
                .Include(s => s.IdSedeNavigation)
                .Where(s => s.Estado == "Pendiente")
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
        }

        // 2. Crear Solicitud
        public async Task<SolicitudIngreso> CrearAsync(SolicitudIngreso solicitud)
        {
            _context.SolicitudIngresos.Add(solicitud);
            await _context.SaveChangesAsync();
            return solicitud;
        }

        // 3. Validar Duplicados
        public async Task<bool> ExistePendienteAsync(string dni, int idCarrera)
        {
            // 🚀 OPTIMIZADO: Consulta rápida sin tracking
            return await _context.SolicitudIngresos
                .AsNoTracking()
                .AnyAsync(s => s.Dni == dni
                            && s.IdCarreraInteres == idCarrera
                            && s.Estado == "Pendiente");
        }

        // 4. Listar Todas
        public async Task<List<SolicitudIngreso>> GetAllAsync()
        {
            return await _context.SolicitudIngresos
                .AsNoTracking()
                .Include(s => s.IdCarreraInteresNavigation)
                .Include(s => s.IdSedeNavigation)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
        }

        public async Task UpdateAsync(SolicitudIngreso solicitud)
        {
            var existing = await _context.SolicitudIngresos.FirstOrDefaultAsync(s => s.Id == solicitud.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(solicitud);
                await _context.SaveChangesAsync();
            }
        }

        // 5. Obtener por ID (Detalle completo)
        public async Task<SolicitudIngreso?> GetByIdAsync(int id)
        {
            return await _context.SolicitudIngresos
                .AsNoTracking()
                .Include(s => s.IdCarreraInteresNavigation)
                .Include(s => s.IdSedeNavigation)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // 6. Aprobar o Rechazar
        public async Task<bool> ActualizarEstadoAsync(int id, string nuevoEstado, string? observacion)
        {
            var solicitud = await _context.SolicitudIngresos.FindAsync(id);
            if (solicitud == null) return false;

            solicitud.Estado = nuevoEstado;
            solicitud.FechaProcesado = DateTime.Now;

            if (!string.IsNullOrEmpty(observacion))
            {
                solicitud.ObservacionAdmin = observacion;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        // 7. Historial
        public async Task<List<SolicitudIngreso>> GetHistorialAsync()
        {
            return await _context.SolicitudIngresos
                .AsNoTracking()
                .Include(x => x.IdCarreraInteresNavigation)
                .Include(x => x.IdSedeNavigation)
                .Where(x => x.Estado != "Pendiente")
                .OrderByDescending(x => x.FechaProcesado)
                .ToListAsync();
        }
    }
}