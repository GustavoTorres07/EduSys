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
            return await _context.SolicitudIngresos
                .Include(s => s.IdCarreraInteresNavigation) // Nombre Carrera
                .Include(s => s.IdSedeNavigation)           // ✅ NUEVO: Trae el nombre de la Sede
                .Where(s => s.Estado == "Pendiente")
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
        }

        // 2. Crear Solicitud (Guarda IdSede automáticamente si viene en el objeto)
        public async Task<SolicitudIngreso> CrearAsync(SolicitudIngreso solicitud)
        {
            _context.SolicitudIngresos.Add(solicitud);
            await _context.SaveChangesAsync();
            return solicitud;
        }

        // 3. Validar Duplicados
        // Nota: Validamos por DNI y Carrera. Si intenta misma carrera en otra sede,
        // usualmente también se bloquea hasta resolver la anterior, pero podrías agregar && s.IdSede == idSede si quisieras permitirlo.
        public async Task<bool> ExistePendienteAsync(string dni, int idCarrera)
        {
            return await _context.SolicitudIngresos
                .AnyAsync(s => s.Dni == dni
                            && s.IdCarreraInteres == idCarrera
                            && s.Estado == "Pendiente");
        }

        // 4. Listar Todas
        public async Task<List<SolicitudIngreso>> GetAllAsync()
        {
            return await _context.SolicitudIngresos
                .Include(s => s.IdCarreraInteresNavigation)
                .Include(s => s.IdSedeNavigation) // ✅ NUEVO
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
        }

        public async Task UpdateAsync(SolicitudIngreso solicitud)
        {
            _context.SolicitudIngresos.Update(solicitud);
            await _context.SaveChangesAsync();
        }

        // 5. Obtener por ID (Detalle completo)
        public async Task<SolicitudIngreso?> GetByIdAsync(int id)
        {
            return await _context.SolicitudIngresos
                .Include(s => s.IdCarreraInteresNavigation)
                .Include(s => s.IdSedeNavigation) // ✅ NUEVO: Para ver la sede en el detalle
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

            await _context.SaveChangesAsync();
            return true;
        }

        // 7. Historial
        public async Task<List<SolicitudIngreso>> GetHistorialAsync()
        {
            return await _context.SolicitudIngresos
                .Include(x => x.IdCarreraInteresNavigation)
                .Include(x => x.IdSedeNavigation) // ✅ NUEVO
                .Where(x => x.Estado != "Pendiente")
                .OrderByDescending(x => x.FechaProcesado)
                .ToListAsync();
        }
    }
}