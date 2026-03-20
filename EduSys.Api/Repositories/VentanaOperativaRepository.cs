using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class VentanaOperativaRepository : IVentanaOperativaRepository
    {
        private readonly EduSysDbContext _context;

        public VentanaOperativaRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<VentanaOperativa>> GetAllAsync()
        {
            // 🚀 OPTIMIZADO: Lectura sin tracking para el administrador
            return await _context.VentanaOperativas
                .AsNoTracking()
                .Include(v => v.IdPeriodoNavigation)
                .Include(v => v.IdCarreraNavigation)
                .Include(v => v.IdSedeNavigation)
                .OrderByDescending(v => v.FechaInicio)
                .ToListAsync();
        }

        public async Task<bool> CreateAsync(VentanaOperativa ventana)
        {
            _context.VentanaOperativas.Add(ventana);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var v = await _context.VentanaOperativas.FindAsync(id);
            if (v == null) return false;

            _context.VentanaOperativas.Remove(v);
            return await _context.SaveChangesAsync() > 0;
        }

        // ✅ LÓGICA DE CASCADA - 🚀 OPTIMIZACIÓN EXTREMA
        public async Task<bool> IsHabilitadoAsync(string tipoAccion, int idPeriodo, int? idCarrera, int? idSede)
        {
            var hoy = DateTime.Now;

            // Al agrupar la lógica booleana dentro de AnyAsync, obligamos a SQL Server
            // a evaluar la cascada internamente, devolviendo solo True o False a la RAM.
            return await _context.VentanaOperativas
                .AsNoTracking()
                .AnyAsync(v =>
                    v.IdPeriodo == idPeriodo &&
                    v.TipoAccion == tipoAccion &&
                    v.FechaInicio <= hoy &&
                    v.FechaFin >= hoy &&
                    (
                        // Nivel 1: Prioridad Máxima (Carrera Y Sede específicas)
                        (idCarrera.HasValue && idSede.HasValue && v.IdCarrera == idCarrera && v.IdSede == idSede)
                        ||
                        // Nivel 2: Prioridad Media (Solo Carrera, cualquier Sede)
                        (idCarrera.HasValue && v.IdCarrera == idCarrera && v.IdSede == null)
                        ||
                        // Nivel 3: Prioridad Baja (Global)
                        (v.IdCarrera == null && v.IdSede == null)
                    )
                );
        }
    }
}