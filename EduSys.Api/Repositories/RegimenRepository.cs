using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class RegimenRepository : IRegimenRepository
    {
        private readonly EduSysDbContext _context;

        public RegimenRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<Regimen>> GetAllAsync()
        {
            // 🚀 OPTIMIZADO: AsNoTracking para evitar uso de caché y OrderBy para la UI
            return await _context.Regimenes
                .AsNoTracking()
                .OrderBy(r => r.Nombre)
                .ToListAsync();
        }

        public async Task<Regimen?> GetByIdAsync(int id)
        {
            // 🚀 OPTIMIZADO: Lectura directa sin tracking
            return await _context.Regimenes
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Regimen> CreateAsync(Regimen regimen)
        {
            _context.Regimenes.Add(regimen);
            await _context.SaveChangesAsync();
            return regimen;
        }

        public async Task<bool> UpdateAsync(Regimen regimen)
        {
            // Buscamos la entidad para que EF inicie el seguimiento
            var existing = await _context.Regimenes.FirstOrDefaultAsync(r => r.Id == regimen.Id);
            if (existing == null) return false;

            // 🚀 Actualización inteligente: solo se enviarán los cambios reales a la BD
            existing.Nombre = regimen.Nombre;
            existing.Activo = regimen.Activo;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var regimen = await _context.Regimenes.FindAsync(id);
            if (regimen == null) return false;

            // Baja Lógica: Mantenemos la integridad histórica de los planes de estudio
            regimen.Activo = false;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}