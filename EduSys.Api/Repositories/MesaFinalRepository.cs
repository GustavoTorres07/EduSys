using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class MesaFinalRepository : IMesaFinalRepository
    {
        private readonly EduSysDbContext _context;

        public MesaFinalRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<MesaFinalDTO>> GetAllAsync()
        {
            // 🚀 OPTIMIZADO: Proyección directa para evitar traer datos pesados a memoria
            return await _context.MesaFinals
                .AsNoTracking()
                .OrderByDescending(m => m.FechaHora)
                .Select(m => MapearAProyeccionDTO(m))
                .ToListAsync();
        }

        public async Task<List<MesaFinalDTO>> GetByPeriodoAsync(int idPeriodo)
        {
            return await _context.MesaFinals
                .AsNoTracking()
                .Where(m => m.IdPeriodo == idPeriodo)
                .OrderByDescending(m => m.FechaHora)
                .Select(m => MapearAProyeccionDTO(m))
                .ToListAsync();
        }

        public async Task<MesaFinalDTO?> GetByIdAsync(int id)
        {
            // Aquí sí necesitamos los datos completos
            var mesa = await _context.MesaFinals
                .AsNoTracking()
                .Include(m => m.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(m => m.IdPeriodoNavigation)
                .Include(m => m.IdPresidenteMesaNavigation).ThenInclude(d => d.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mesa == null) return null;

            var dto = MapearAProyeccionDTO(mesa);
            dto.CantidadInscriptos = await _context.InscripcionFinals.CountAsync(i => i.IdMesaFinal == id && i.Estado != "Baja");
            return dto;
        }

        public async Task<ResultadoOperacionDTO> CreateAsync(MesaFinalRequestDTO dto)
        {
            try
            {
                var nueva = new MesaFinal
                {
                    IdPlanMateria = dto.IdPlanMateria,
                    IdPeriodo = dto.IdPeriodo,
                    IdPresidenteMesa = dto.IdPresidenteMesa,
                    IdVocal1 = dto.IdVocal1,
                    IdVocal2 = dto.IdVocal2,
                    FechaHora = dto.FechaHora,
                    Estado = "Abierta"
                };

                _context.MesaFinals.Add(nueva);
                await _context.SaveChangesAsync();
                return new ResultadoOperacionDTO { Exito = true, Mensaje = "Mesa final creada correctamente." };
            }
            catch (Exception ex)
            {
                return new ResultadoOperacionDTO { Exito = false, Mensaje = "Error al crear: " + ex.Message };
            }
        }

        public async Task<ResultadoOperacionDTO> UpdateAsync(MesaFinalRequestDTO dto)
        {
            var mesa = await _context.MesaFinals.FirstOrDefaultAsync(m => m.Id == dto.Id);
            if (mesa == null) return new ResultadoOperacionDTO { Exito = false, Mensaje = "No encontrada." };

            if (mesa.Estado == "Cerrada") return new ResultadoOperacionDTO { Exito = false, Mensaje = "No se puede editar una mesa cerrada." };

            mesa.IdPlanMateria = dto.IdPlanMateria;
            mesa.IdPeriodo = dto.IdPeriodo;
            mesa.IdPresidenteMesa = dto.IdPresidenteMesa;
            mesa.IdVocal1 = dto.IdVocal1;
            mesa.IdVocal2 = dto.IdVocal2;
            mesa.FechaHora = dto.FechaHora;
            mesa.Estado = dto.Estado;

            await _context.SaveChangesAsync();
            return new ResultadoOperacionDTO { Exito = true, Mensaje = "Mesa final actualizada." };
        }

        public async Task<ResultadoOperacionDTO> DeleteAsync(int id)
        {
            var mesa = await _context.MesaFinals.Include(m => m.InscripcionFinals).FirstOrDefaultAsync(m => m.Id == id);
            if (mesa == null) return new ResultadoOperacionDTO { Exito = false, Mensaje = "No encontrada." };

            if (mesa.InscripcionFinals.Any(i => i.Estado != "Baja"))
                return new ResultadoOperacionDTO { Exito = false, Mensaje = "No se puede eliminar una mesa con alumnos inscriptos." };

            _context.MesaFinals.Remove(mesa);
            await _context.SaveChangesAsync();
            return new ResultadoOperacionDTO { Exito = true, Mensaje = "Mesa eliminada." };
        }

        // =======================================================================
        // LÓGICA PARA ACTAS DE EXÁMENES FINALES
        // =======================================================================

        public async Task<ActaMesaFinalDTO?> GetActaMesaFinalAsync(int idMesaFinal)
        {
            return await _context.MesaFinals
                .AsNoTracking()
                .AsSplitQuery() // 🚀 OPTIMIZADO para carga de muchos alumnos
                .Where(m => m.Id == idMesaFinal)
                .Select(mesa => new ActaMesaFinalDTO
                {
                    IdMesaFinal = mesa.Id,
                    MateriaNombre = mesa.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    CarreraNombre = mesa.IdPlanMateriaNavigation.IdPlanNavigation.IdCarreraNavigation.Nombre,
                    FechaHora = mesa.FechaHora,
                    Tribunal = $"{mesa.IdPresidenteMesaNavigation.IdUsuarioNavigation.Apellido} (Presidente)",
                    EstadoMesa = mesa.Estado ?? "Abierta",
                    Libro = mesa.Libro ?? "",
                    Folio = mesa.Folio ?? "",
                    Alumnos = mesa.InscripcionFinals
                        .Where(i => i.Estado != "Baja")
                        .Select(i => new AlumnoActaFinalDTO
                        {
                            IdInscripcion = i.Id,
                            IdAlumno = i.IdAlumno,
                            Legajo = i.IdAlumnoNavigation.Legajo,
                            AlumnoNombre = $"{i.IdAlumnoNavigation.IdUsuarioNavigation.Apellido}, {i.IdAlumnoNavigation.IdUsuarioNavigation.Nombre}",
                            Dni = i.IdAlumnoNavigation.IdUsuarioNavigation.Dni,
                            Condicion = "Regular", // Esto podría venir de la inscripción si lo guardas
                            Nota = i.Nota,
                            EstadoInscripcion = i.Estado ?? "Inscripto"
                        })
                        .OrderBy(a => a.AlumnoNombre)
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> GuardarNotaFinalAsync(int idInscripcion, decimal? nota)
        {
            var inscripcion = await _context.InscripcionFinals.FindAsync(idInscripcion);
            if (inscripcion == null || inscripcion.Estado == "Baja") return false;

            inscripcion.Nota = nota;

            if (nota == null) inscripcion.Estado = "Inscripto";
            else if (nota >= 4) inscripcion.Estado = "Aprobado";
            else inscripcion.Estado = "Reprobado";

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CerrarActaFinalAsync(int idMesaFinal, string libro, string folio)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var mesa = await _context.MesaFinals
                    .Include(m => m.InscripcionFinals)
                    .FirstOrDefaultAsync(m => m.Id == idMesaFinal);

                if (mesa == null || mesa.Estado == "Cerrada") return false;

                mesa.Estado = "Cerrada";
                mesa.Libro = libro;
                mesa.Folio = folio;

                foreach (var inscripcion in mesa.InscripcionFinals.Where(i => i.Estado != "Baja"))
                {
                    if (!inscripcion.Nota.HasValue)
                    {
                        inscripcion.Estado = "Ausente";
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Helper para proyecciones limpias en SQL
        private static MesaFinalDTO MapearAProyeccionDTO(MesaFinal m)
        {
            return new MesaFinalDTO
            {
                Id = m.Id,
                IdPlanMateria = m.IdPlanMateria,
                MateriaNombre = m.IdPlanMateriaNavigation?.IdMateriaNavigation?.Nombre ?? "S/D",
                CarreraNombre = m.IdPlanMateriaNavigation?.IdPlanNavigation?.IdCarreraNavigation?.Nombre ?? "S/D",
                IdPeriodo = m.IdPeriodo,
                PeriodoNombre = m.IdPeriodoNavigation?.Nombre ?? "S/D",
                IdPresidenteMesa = m.IdPresidenteMesa,
                PresidenteNombre = m.IdPresidenteMesaNavigation?.IdUsuarioNavigation != null
                    ? $"{m.IdPresidenteMesaNavigation.IdUsuarioNavigation.Apellido}, {m.IdPresidenteMesaNavigation.IdUsuarioNavigation.Nombre}"
                    : "No asignado",
                IdVocal1 = m.IdVocal1,
                IdVocal2 = m.IdVocal2,
                FechaHora = m.FechaHora,
                Estado = m.Estado,
                Libro = m.Libro,
                Folio = m.Folio,
                CantidadInscriptos = m.InscripcionFinals.Count(i => i.Estado != "Baja")
            };
        }
    }
}