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
            return await _context.Regimenes.ToListAsync();
        }

        public async Task<Regimen?> GetByIdAsync(int id)
        {
            return await _context.Regimenes.FindAsync(id);
        }

        public async Task<Regimen> CreateAsync(Regimen regimen)
        {
            _context.Regimenes.Add(regimen);
            await _context.SaveChangesAsync();
            return regimen;
        }

        public async Task<bool> UpdateAsync(Regimen regimen)
        {
            var existe = await _context.Regimenes.AnyAsync(r => r.Id == regimen.Id);
            if (!existe) return false;

            _context.Entry(regimen).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var regimen = await _context.Regimenes.FindAsync(id);
            if (regimen == null) return false;

            regimen.Activo = false; // Baja Lógica
            await _context.SaveChangesAsync();
            return true;
        }
    }
}