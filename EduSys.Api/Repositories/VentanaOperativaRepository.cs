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
            return await _context.VentanaOperativas
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

        // ✅ LÓGICA DE CASCADA
        public async Task<bool> IsHabilitadoAsync(string tipoAccion, int idPeriodo, int? idCarrera, int? idSede)
        {
            var hoy = DateTime.Now;

            // 1. Buscamos todas las ventanas VIGENTES para este periodo y acción
            var ventanas = await _context.VentanaOperativas
                .Where(v => v.IdPeriodo == idPeriodo &&
                            v.TipoAccion == tipoAccion &&
                            v.FechaInicio <= hoy &&
                            v.FechaFin >= hoy)
                .ToListAsync();

            if (!ventanas.Any()) return false;

            // 2. NIVEL 1: Prioridad Máxima (Carrera Y Sede específicas)
            // Ej: Solo para Software en Toay
            if (idCarrera.HasValue && idSede.HasValue)
            {
                if (ventanas.Any(v => v.IdCarrera == idCarrera && v.IdSede == idSede)) return true;
            }

            // 3. NIVEL 2: Prioridad Media (Solo Carrera, cualquier Sede)
            // Ej: Para Software en general
            if (idCarrera.HasValue)
            {
                if (ventanas.Any(v => v.IdCarrera == idCarrera && v.IdSede == null)) return true;
            }

            // 4. NIVEL 3: Prioridad Baja (Global)
            // Ej: Para toda la universidad
            if (ventanas.Any(v => v.IdCarrera == null && v.IdSede == null)) return true;

            return false;
        }
    }
}