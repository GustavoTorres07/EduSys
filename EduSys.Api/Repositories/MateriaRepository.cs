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
            // 🚀 OPTIMIZADO: AsNoTracking y OrderBy
            return await _context.Materia
                .AsNoTracking()
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        public async Task<Materia?> GetByIdAsync(int id)
        {
            // 🚀 OPTIMIZADO: AsNoTracking para lectura rápida
            return await _context.Materia
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Materia> CreateAsync(Materia materia)
        {
            _context.Materia.Add(materia);
            await _context.SaveChangesAsync();
            return materia;
        }

        public async Task<bool> UpdateAsync(Materia materia)
        {
            // Buscamos la materia para que EF la rastree
            var existing = await _context.Materia.FirstOrDefaultAsync(m => m.Id == materia.Id);
            if (existing == null) return false;

            // 🚀 EF Core detectará solo los campos cambiados al hacer SaveChanges
            existing.Nombre = materia.Nombre;
            existing.Codigo = materia.Codigo;
            existing.Activo = materia.Activo;
            existing.Descripcion = materia.Descripcion;

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
            // 🚀 OPTIMIZADO: AsNoTracking y comparación insensible a mayúsculas
            return await _context.Materia
                .AsNoTracking()
                .AnyAsync(m => m.Codigo.ToLower() == codigo.ToLower() && m.Id != idExcluir);
        }
    }
}