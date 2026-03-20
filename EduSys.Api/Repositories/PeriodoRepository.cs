using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class PeriodoRepository : IPeriodoRepository
    {
        private readonly EduSysDbContext _context;
        public PeriodoRepository(EduSysDbContext context) { _context = context; }

        public async Task<List<PeriodoAcademico>> GetAllAsync()
        {
            // 🚀 OPTIMIZADO: AsNoTracking para una lista de configuración que no se editará aquí
            return await _context.PeriodoAcademicos
                .AsNoTracking()
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();
        }

        public async Task<PeriodoAcademico?> GetByIdAsync(int id)
            => await _context.PeriodoAcademicos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<bool> CreateAsync(PeriodoAcademico periodo)
        {
            _context.PeriodoAcademicos.Add(periodo);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(PeriodoAcademico periodo)
        {
            var existing = await _context.PeriodoAcademicos.FindAsync(periodo.Id);
            if (existing == null) return false;

            existing.Nombre = periodo.Nombre;
            existing.FechaInicio = periodo.FechaInicio;
            existing.FechaFin = periodo.FechaFin;
            existing.Estado = periodo.Estado;
            existing.Activo = periodo.Activo;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.PeriodoAcademicos.FindAsync(id);
            if (existing == null) return false;

            // Baja Lógica y cierre de seguridad
            existing.Activo = false;
            existing.Estado = "Cerrado";

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ValidarSuperposicionAsync(DateOnly inicio, DateOnly fin, int idExcluir = 0)
        {
            // 🚀 Implementación de lógica de traslape de fechas
            return await _context.PeriodoAcademicos
                .AsNoTracking()
                .AnyAsync(p => p.Id != idExcluir &&
                               p.Activo == true &&
                               inicio < p.FechaFin &&
                               p.FechaInicio < fin);
        }
    }
}