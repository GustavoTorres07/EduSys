using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Api.Services.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class NotasRepository : INotasRepository
    {
        private readonly EduSysDbContext _context;

        public NotasRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<PlanillaNotasDTO> GetPlanillaAsync(int idComision)
        {
            var comision = await _context.Comisions
                .AsNoTracking()
                .Include(c => c.IdPlanMateriaNavigation)
                    .ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(c => c.Evaluacions)
                .Include(c => c.InscripcionCursada.Where(i => i.Estado != "Baja"))
                    .ThenInclude(i => i.IdAlumnoNavigation)
                        .ThenInclude(a => a.IdUsuarioNavigation)
                .Include(c => c.InscripcionCursada)
                    .ThenInclude(i => i.Nota)
                .Include(c => c.InscripcionCursada)
                    .ThenInclude(i => i.IdEstadoMateriaNavigation)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.Id == idComision);

            if (comision == null) return null;

            var evaluacionesDto = comision.Evaluacions.Select(e => new EvaluacionDTO
            {
                IdEvaluacion = e.Id,
                Nombre = e.Nombre,
                Fecha = e.Fecha.ToDateTime(TimeOnly.MinValue),
                EsRecuperatorio = e.EsRecuperatorio == true,
                EsIntegrador = e.EsIntegrador == true,
                IdEvaluacionPadre = e.IdEvaluacionPadre,
                EstadoActa = e.EstadoActa ?? "Abierta",
                Libro = e.Libro,
                Folio = e.Folio,
                FechaCierre = e.FechaCierre
            }).OrderBy(e => e.Fecha).ToList();

            var planilla = new PlanillaNotasDTO
            {
                IdComision = idComision,
                MateriaNombre = comision.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                ComisionCodigo = comision.Codigo,
                ComisionEstado = comision.Estado ?? "Abierta",
                Evaluaciones = evaluacionesDto,
                Alumnos = comision.InscripcionCursada.Select(ins => new NotaAlumnoDTO
                {
                    IdInscripcion = ins.Id,
                    AlumnoNombre = $"{ins.IdAlumnoNavigation.IdUsuarioNavigation.Apellido}, {ins.IdAlumnoNavigation.IdUsuarioNavigation.Nombre}",
                    Legajo = ins.IdAlumnoNavigation.Legajo,
                    Estado = ins.IdEstadoMateriaNavigation?.Nombre ?? "Cursando",
                    CursadaCerrada = ins.CursadaCerrada,
                    Notas = evaluacionesDto.ToDictionary(
                        e => e.IdEvaluacion,
                        e => ins.Nota.FirstOrDefault(n => n.IdEvaluacion == e.IdEvaluacion)?.Valor
                    ),
                    Promedio = ins.Nota.Any() ? Math.Round(ins.Nota.Average(n => n.Valor), 2) : (decimal?)null
                }).OrderBy(a => a.AlumnoNombre).ToList()
            };

            return planilla;
        }

        public async Task<bool> GuardarNotaAsync(int idInscripcion, int idEvaluacion, decimal? valor)
        {
            try
            {
                var evaluacion = await _context.Evaluacions.AsNoTracking().FirstOrDefaultAsync(e => e.Id == idEvaluacion);
                if (evaluacion == null || evaluacion.EstadoActa == "Cerrada") return false;

                var nota = await _context.Nota
                    .FirstOrDefaultAsync(n => n.IdInscripcionCursada == idInscripcion && n.IdEvaluacion == idEvaluacion);

                if (valor.HasValue)
                {
                    if (nota == null)
                    {
                        await _context.Nota.AddAsync(new Nota
                        {
                            IdInscripcionCursada = idInscripcion,
                            IdEvaluacion = idEvaluacion,
                            Valor = valor.Value,
                            FechaCarga = DateTime.Now
                        });
                    }
                    else
                    {
                        nota.Valor = valor.Value;
                        nota.FechaCarga = DateTime.Now;
                    }
                }
                else
                {
                    if (nota != null) _context.Nota.Remove(nota);
                }

                await _context.SaveChangesAsync();
                await RecalcularEstadoAlumnoAsync(idInscripcion);
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> CrearEvaluacionAsync(int idComision, EvaluacionDTO dto)
        {
            _context.Evaluacions.Add(new Evaluacion
            {
                IdComision = idComision,
                Nombre = dto.Nombre,
                Fecha = DateOnly.FromDateTime(dto.Fecha),
                EsRecuperatorio = dto.EsRecuperatorio,
                EsIntegrador = dto.EsIntegrador,
                IdEvaluacionPadre = dto.IdEvaluacionPadre,
                EstadoActa = "Abierta"
            });
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EditarEvaluacionAsync(EvaluacionDTO dto)
        {
            var eval = await _context.Evaluacions.FindAsync(dto.IdEvaluacion);
            if (eval == null || eval.EstadoActa == "Cerrada") return false;
            eval.Nombre = dto.Nombre;
            eval.Fecha = DateOnly.FromDateTime(dto.Fecha);
            eval.EsRecuperatorio = dto.EsRecuperatorio;
            eval.EsIntegrador = dto.EsIntegrador;
            eval.IdEvaluacionPadre = dto.IdEvaluacionPadre;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarEvaluacionAsync(int idEvaluacion)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var eval = await _context.Evaluacions.FirstOrDefaultAsync(e => e.Id == idEvaluacion);
                if (eval == null || eval.EstadoActa == "Cerrada") return false;

                var hijos = await _context.Evaluacions.Where(e => e.IdEvaluacionPadre == idEvaluacion).ToListAsync();
                foreach (var h in hijos) { h.IdEvaluacionPadre = null; h.EsRecuperatorio = false; }
                await _context.SaveChangesAsync();

                var notas = await _context.Nota.Where(n => n.IdEvaluacion == idEvaluacion).ToListAsync();
                if (notas.Any()) _context.Nota.RemoveRange(notas);
                await _context.SaveChangesAsync();

                _context.Evaluacions.Remove(eval);
                await _context.SaveChangesAsync();

                var inscs = await _context.InscripcionCursada.Where(i => i.IdComision == eval.IdComision && i.Estado != "Baja").Select(i => i.Id).ToListAsync();
                foreach (var id in inscs) await RecalcularEstadoAlumnoAsync(id);

                await trans.CommitAsync();
                return true;
            }
            catch { await trans.RollbackAsync(); return false; }
        }

        private (int IdEstado, decimal? NotaFinal) CalcularEstadoYNotaDefinitiva(PlanMateria plan, List<Nota> notasAlumno, List<Evaluacion> evaluacionesComision, int idEstadoActual)
        {
            if (!notasAlumno.Any()) return (idEstadoActual, null);

            int estadoDesaprobado = plan.IdEstadoSiDesaprueba ?? 7;
            int estadoLibre = plan.IdEstadoSiFaltaAsistencia ?? 4;
            int estadoRegular = plan.IdEstadoRegular ?? 2;

            if (plan.NotaEliminatoria.HasValue && notasAlumno.Any(n => n.Valor < plan.NotaEliminatoria.Value))
            {
                decimal prom = notasAlumno.Any() ? Math.Round(notasAlumno.Average(n => n.Valor), 2) : 0m;
                return (estadoLibre, prom);
            }

            var integrador = evaluacionesComision.FirstOrDefault(e => e.EsIntegrador == true);
            var parcialesPrincipales = evaluacionesComision.Where(e => e.IdEvaluacionPadre == null && e.EsIntegrador != true).ToList();
            var notasPorInstancia = new List<decimal>();

            foreach (var parcial in parcialesPrincipales)
            {
                decimal? notaParcial = notasAlumno.FirstOrDefault(n => n.IdEvaluacion == parcial.Id)?.Valor;
                var recuperatoriosIds = evaluacionesComision.Where(e => e.IdEvaluacionPadre == parcial.Id).Select(r => r.Id).ToList();
                bool rindioInstancia = notaParcial.HasValue || notasAlumno.Any(n => recuperatoriosIds.Contains(n.IdEvaluacion));

                if (rindioInstancia)
                {
                    decimal notaDefinitiva = notaParcial ?? 0m;
                    var notasRecus = notasAlumno.Where(n => recuperatoriosIds.Contains(n.IdEvaluacion)).ToList();

                    if (notasRecus.Any())
                    {
                        notaDefinitiva = (plan.ModoNotaRecuperatorio == 0)
                            ? Math.Max(notaDefinitiva, notasRecus.Max(n => n.Valor))
                            : notasRecus.OrderByDescending(n => n.FechaCarga).First().Valor;
                    }
                    notasPorInstancia.Add(notaDefinitiva);
                }
            }

            if (!notasPorInstancia.Any()) return (idEstadoActual, null);

            decimal promedioInstancias = Math.Round(notasPorInstancia.Average(), 2);
            decimal notaDeCorte = plan.PromedioMinimoAprobacion ?? plan.NotaMinimaRegularizar ?? 4m;

            if (plan.CantidadAplazosParaLibre.HasValue && notasPorInstancia.Count(n => n < notaDeCorte) > plan.CantidadAplazosParaLibre.Value)
                return (estadoLibre, promedioInstancias);

            if (plan.CantidadParciales.HasValue && notasPorInstancia.Count >= plan.CantidadParciales.Value)
            {
                bool aproboBase = plan.ModoAprobacionCursada == 1 ? notasPorInstancia.All(n => n >= notaDeCorte) : promedioInstancias >= notaDeCorte;

                if (aproboBase)
                {
                    bool cumplePromo = plan.NotaMinimaPromocion.HasValue &&
                        (plan.ModoAprobacionCursada == 1 ? notasPorInstancia.All(n => n >= plan.NotaMinimaPromocion.Value) : promedioInstancias >= plan.NotaMinimaPromocion.Value);

                    if (plan.EsPromocionable == true && cumplePromo && plan.IdEstadoPromocion.HasValue)
                        return (plan.IdEstadoPromocion.Value, promedioInstancias);

                    return (estadoRegular, null);
                }
                else if (plan.TieneIntegrador && integrador != null)
                {
                    decimal? notaInt = notasAlumno.FirstOrDefault(n => n.IdEvaluacion == integrador.Id)?.Valor;
                    if (notaInt.HasValue)
                    {
                        decimal minInt = plan.NotaAprobacionIntegrador ?? notaDeCorte;
                        if (notaInt.Value >= minInt)
                        {
                            if (plan.IntegradorPermitePromocion && plan.NotaPromocionIntegrador.HasValue && notaInt.Value >= plan.NotaPromocionIntegrador.Value && plan.IdEstadoPromocion.HasValue)
                                return (plan.IdEstadoPromocion.Value, notaInt.Value);
                            return (estadoRegular, null);
                        }
                        return (estadoDesaprobado, notaInt.Value);
                    }
                }
                return (estadoDesaprobado, promedioInstancias);
            }
            return (idEstadoActual, promedioInstancias);
        }

        private async Task RecalcularEstadoAlumnoAsync(int idInscripcionCursada)
        {
            var inscripcion = await _context.InscripcionCursada
                .Include(i => i.Nota)
                .Include(i => i.IdComisionNavigation.IdPlanMateriaNavigation)
                .FirstOrDefaultAsync(i => i.Id == idInscripcionCursada);

            if (inscripcion == null || inscripcion.CursadaCerrada) return;

            var evaluaciones = await _context.Evaluacions
                .AsNoTracking()
                .Where(e => e.IdComision == inscripcion.IdComision)
                .ToListAsync();

            var res = CalcularEstadoYNotaDefinitiva(inscripcion.IdComisionNavigation.IdPlanMateriaNavigation, inscripcion.Nota.ToList(), evaluaciones, inscripcion.IdEstadoMateria ?? 1);
            inscripcion.IdEstadoMateria = res.IdEstado;
            inscripcion.NotaFinalCursada = res.NotaFinal;

            await _context.SaveChangesAsync();
        }
    }
}