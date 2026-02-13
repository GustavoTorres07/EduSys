using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class MateriaRepository : IMateriaRepository
    {
        private readonly EduSysDbContext _context;

        public MateriaRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<Materia>> GetAllAsync()
        {
            return await _context.Materia.ToListAsync();
        }

        public async Task<Materia?> GetByIdAsync(int id)
        {
            return await _context.Materia.FindAsync(id);
        }

        public async Task<Materia> CreateAsync(Materia materia)
        {
            _context.Materia.Add(materia);
            await _context.SaveChangesAsync();
            return materia;
        }

        public async Task<bool> UpdateAsync(Materia materia)
        {
            var existe = await _context.Materia.AnyAsync(m => m.Id == materia.Id);
            if (!existe) return false;

            _context.Entry(materia).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var materia = await _context.Materia.FindAsync(id);
            if (materia == null) return false;

            materia.Activo = false; // Baja Lógica
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteCodigoAsync(string codigo, int idExcluir = 0)
        {
            return await _context.Materia.AnyAsync(m => m.Codigo == codigo && m.Id != idExcluir);
        }
    }
}