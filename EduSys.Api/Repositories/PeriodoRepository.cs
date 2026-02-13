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
            // Ordenamos por fecha de inicio descendente (lo más nuevo primero)
            return await _context.PeriodoAcademicos
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();
        }

        public async Task<PeriodoAcademico?> GetByIdAsync(int id)
            => await _context.PeriodoAcademicos.FindAsync(id);

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

            existing.Activo = false;
            existing.Estado = "Cerrado"; // Al dar de baja, cerramos el período
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ValidarSuperposicionAsync(DateTime inicio, DateTime fin, int idExcluir = 0)
        {
            // Opcional: Lógica para avisar si se superponen fechas, por ahora retornamos false
            // para no bloquear, ya que a veces hay cursos de verano que se solapan con mesas de examen.
            return await Task.FromResult(false);
        }
    }
}