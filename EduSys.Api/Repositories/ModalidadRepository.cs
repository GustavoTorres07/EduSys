using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class ModalidadRepository : IModalidadRepository
    {
        private readonly EduSysDbContext _context;

        public ModalidadRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<Modalidad>> GetAllAsync()
        {
            // Retorna todas. Si quisieras solo las activas por defecto:
            // return await _context.Modalidads.Where(m => m.Activo == true).ToListAsync();
            return await _context.Modalidads.ToListAsync();
        }

        public async Task<Modalidad?> GetByIdAsync(int id)
        {
            return await _context.Modalidads.FindAsync(id);
        }

        public async Task<Modalidad> CreateAsync(Modalidad modalidad)
        {
            _context.Modalidads.Add(modalidad);
            await _context.SaveChangesAsync();
            return modalidad;
        }

        public async Task<bool> UpdateAsync(Modalidad modalidad)
        {
            var existe = await _context.Modalidads.AnyAsync(m => m.Id == modalidad.Id);
            if (!existe) return false;

            _context.Entry(modalidad).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var modalidad = await _context.Modalidads.FindAsync(id);
            if (modalidad == null) return false;

            // Baja Lógica (Soft Delete): Solo marcamos Activo como false
            modalidad.Activo = false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int idExcluir = 0)
        {
            // Verifica si existe alguna OTRA modalidad con ese nombre
            return await _context.Modalidads
                                 .AnyAsync(m => m.Nombre == nombre && m.Id != idExcluir);
        }
    }
}