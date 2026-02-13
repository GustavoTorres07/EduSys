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
            // Retorna solo las aulas de esa sede que estén activas
            return await _context.Aulas
                .Where(a => a.IdSede == idSede && (a.Activo == true))
                .ToListAsync();
        }
    }
}