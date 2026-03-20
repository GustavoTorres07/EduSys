using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class AulaRepository : IAulaRepository
    {
        private readonly EduSysDbContext _context;

        public AulaRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<Aula>> GetBySedeAsync(int idSede)
        {
            // 🚀 OPTIMIZADO: AsNoTracking para velocidad y OrderBy para UX
            return await _context.Aulas
                .AsNoTracking()
                .Where(a => a.IdSede == idSede && a.Activo == true)
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        }
    }
}