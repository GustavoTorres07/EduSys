using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class EstadoMateriaRepository : IEstadoMateriaRepository
    {
        private readonly EduSysDbContext _context;

        public EstadoMateriaRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<EstadoMateriaDTO>> ObtenerTodosAsync()
        {
            return await _context.EstadoMaterias
                .Select(e => new EstadoMateriaDTO
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    EsAprobatoria = e.EsAprobatoria,
                    HabilitaFinal = e.HabilitaFinal,
                    Activo = e.Activo
                }).ToListAsync();
        }

        public async Task<EstadoMateriaDTO?> ObtenerPorIdAsync(int id)
        {
            var estado = await _context.EstadoMaterias.FindAsync(id);
            if (estado == null) return null;

            return new EstadoMateriaDTO
            {
                Id = estado.Id,
                Nombre = estado.Nombre,
                EsAprobatoria = estado.EsAprobatoria,
                HabilitaFinal = estado.HabilitaFinal,
                Activo = estado.Activo
            };
        }

        public async Task<EstadoMateriaDTO> CrearAsync(EstadoMateriaDTO dto)
        {
            var nuevoEstado = new EstadoMateria
            {
                Nombre = dto.Nombre,
                EsAprobatoria = dto.EsAprobatoria,
                HabilitaFinal = dto.HabilitaFinal,
                Activo = dto.Activo
            };

            _context.EstadoMaterias.Add(nuevoEstado);
            await _context.SaveChangesAsync();

            dto.Id = nuevoEstado.Id;
            return dto;
        }

        public async Task<bool> ActualizarAsync(EstadoMateriaDTO dto)
        {
            var estado = await _context.EstadoMaterias.FindAsync(dto.Id);
            if (estado == null) return false;

            estado.Nombre = dto.Nombre;
            estado.EsAprobatoria = dto.EsAprobatoria;
            estado.HabilitaFinal = dto.HabilitaFinal;
            estado.Activo = dto.Activo;

            _context.EstadoMaterias.Update(estado);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var estado = await _context.EstadoMaterias.FindAsync(id);
            if (estado == null) return false;

            // Baja lógica
            estado.Activo = false;
            _context.EstadoMaterias.Update(estado);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}