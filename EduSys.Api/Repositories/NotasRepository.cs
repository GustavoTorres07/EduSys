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
        private readonly INotificationService _notificationService;

        public NotasRepository(EduSysDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<PlanillaNotasDTO> GetPlanillaAsync(int idComision)
        {
            var comision = await _context.Comisions
                .Include(c => c.IdPlanMateriaNavigation.IdMateriaNavigation)
                .Include(c => c.Evaluacions)
                .Include(c => c.InscripcionCursada).ThenInclude(i => i.IdAlumnoNavigation.IdUsuarioNavigation)
                .Include(c => c.InscripcionCursada).ThenInclude(i => i.Nota)
                .Include(c => c.InscripcionCursada).ThenInclude(i => i.IdEstadoMateriaNavigation)
                .FirstOrDefaultAsync(c => c.Id == idComision);

            if (comision == null) return null;

            var planilla = new PlanillaNotasDTO
            {
                IdComision = idComision,
                MateriaNombre = comision.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                ComisionCodigo = comision.Codigo,
                ComisionEstado = comision.Estado ?? "Abierta",
                Evaluaciones = comision.Evaluacions.Select(e => new EvaluacionDTO
                {
                    IdEvaluacion = e.Id,
                    Nombre = e.Nombre,
                    Fecha = e.Fecha.ToDateTime(TimeOnly.MinValue),
                    EsRecuperatorio = e.EsRecuperatorio ?? false,
                    IdEvaluacionPadre = e.IdEvaluacionPadre,
                    EstadoActa = e.EstadoActa ?? "Abierta",
                    Libro = e.Libro,
                    Folio = e.Folio,
                    FechaCierre = e.FechaCierre
                }).OrderBy(e => e.Fecha).ToList()
            };

            foreach (var ins in comision.InscripcionCursada.Where(i => i.Estado != "Baja"))
            {
                var fila = new NotaAlumnoDTO
                {
                    IdInscripcion = ins.Id,
                    AlumnoNombre = $"{ins.IdAlumnoNavigation.IdUsuarioNavigation.Apellido}, {ins.IdAlumnoNavigation.IdUsuarioNavigation.Nombre}",
                    Legajo = ins.IdAlumnoNavigation.Legajo,
                    Estado = ins.IdEstadoMateriaNavigation?.Nombre ?? "Cursando",
                    CursadaCerrada = ins.CursadaCerrada
                };

                foreach (var eval in planilla.Evaluaciones)
                {
                    var notaExistente = ins.Nota.FirstOrDefault(n => n.IdEvaluacion == eval.IdEvaluacion);
                    fila.Notas[eval.IdEvaluacion] = notaExistente?.Valor;
                }

                // Cálculo visual rápido (puede diferir de las instancias agrupadas, pero sirve para la tabla)
                var notasValidas = fila.Notas.Values.Where(v => v.HasValue).Select(v => v!.Value);
                if (notasValidas.Any())
                    fila.Promedio = Math.Round(notasValidas.Average(), 2);

                planilla.Alumnos.Add(fila);
            }

            return planilla;
        }

        public async Task<bool> GuardarNotaAsync(int idInscripcion, int idEvaluacion, decimal? valor)
        {
            var evaluacion = await _context.Evaluacions.FindAsync(idEvaluacion);
            if (evaluacion == null) return false;
            if (evaluacion.EstadoActa == "Cerrada") return false;

            var nota = await _context.Nota
                .FirstOrDefaultAsync(n => n.IdInscripcionCursada == idInscripcion && n.IdEvaluacion == idEvaluacion);

            if (valor.HasValue)
            {
                if (nota == null)
                {
                    nota = new Nota { IdInscripcionCursada = idInscripcion, IdEvaluacion = idEvaluacion, Valor = valor.Value, FechaCarga = DateTime.Now };
                    _context.Nota.Add(nota);
                }
                else
                {
                    nota.Valor = valor.Value;
                    nota.FechaCarga = DateTime.Now;
                    _context.Nota.Update(nota);
                }
            }
            else
            {
                if (nota != null) _context.Nota.Remove(nota);
            }

            await _context.SaveChangesAsync();

            // Llamamos al recálculo
            await RecalcularEstadoAlumnoAsync(idInscripcion);

            return true;
        }

        // =======================================================================
        // MOTOR DE REGLAS ACADÉMICAS - ENCAPSULADO Y REUTILIZABLE
        // =======================================================================
        private (int IdEstado, decimal? NotaFinal) CalcularEstadoYNotaDefinitiva(PlanMateria plan, List<Nota> notasAlumno, List<Evaluacion> evaluacionesComision)
        {
            if (!notasAlumno.Any()) return (1, null); // 1 = Cursando

            // 1. MUERTE SÚBITA (Nota Eliminatoria)
            if (plan.NotaEliminatoria.HasValue && notasAlumno.Any(n => n.Valor < plan.NotaEliminatoria.Value))
            {
                // Un aplazo grave lo deja libre inmediatamente (IdEstadoLibre)
                return (plan.IdEstadoLibre ?? 4, Math.Round(notasAlumno.Average(n => n.Valor), 2));
            }

            // 2. AGRUPAR EN INSTANCIAS (Parcial + sus respectivos Recuperatorios)
            var parcialesPrincipales = evaluacionesComision.Where(e => e.IdEvaluacionPadre == null).ToList();
            var notasPorInstancia = new List<decimal>();

            foreach (var parcial in parcialesPrincipales)
            {
                var notaParcial = notasAlumno.FirstOrDefault(n => n.IdEvaluacion == parcial.Id)?.Valor;

                // Buscar si rindió recuperatorios de este parcial específico
                var recuperatorios = evaluacionesComision.Where(e => e.IdEvaluacionPadre == parcial.Id).ToList();
                decimal notaMaxRecu = 0;

                foreach (var r in recuperatorios)
                {
                    var nr = notasAlumno.FirstOrDefault(n => n.IdEvaluacion == r.Id)?.Valor ?? 0;
                    if (nr > notaMaxRecu) notaMaxRecu = nr;
                }

                // La nota de la instancia es la máxima entre el parcial y su(s) recuperatorio(s)
                if (notaParcial.HasValue || notasAlumno.Any(n => recuperatorios.Select(r => r.Id).Contains(n.IdEvaluacion)))
                {
                    notasPorInstancia.Add(Math.Max(notaParcial ?? 0, notaMaxRecu));
                }
            }

            if (!notasPorInstancia.Any()) return (1, null);

            decimal promedioInstancias = notasPorInstancia.Average();
            decimal notaDeCorte = plan.PromedioMinimoAprobacion ?? plan.NotaMinimaRegularizar ?? 4;

            // Un aplazo es una INSTANCIA desaprobada (ya falló parcial y todos sus recuperatorios)
            int cantidadInstanciasDesaprobadas = notasPorInstancia.Count(n => n < notaDeCorte);

            decimal porcentajeAsistencia = 100; // Simulado hasta tener módulo Asistencia

            // 3. REGLA DE ASISTENCIA (Usamos IdEstadoLibre)
            // 3. REGLA DE ASISTENCIA 
            // 3. REGLA DE ASISTENCIA
            if (plan.PorcentajeAsistenciaRegularizar.HasValue && porcentajeAsistencia < plan.PorcentajeAsistenciaRegularizar.Value)
            {
                return (plan.IdEstadoSiFaltaAsistencia ?? 4, Math.Round(promedioInstancias, 2));
            }

            // 4. LÍMITE DE APLAZOS 
            if (plan.CantidadAplazosParaLibre.HasValue && cantidadInstanciasDesaprobadas >= plan.CantidadAplazosParaLibre.Value)
            {
                return (plan.IdEstadoDesaprobado ?? 5, Math.Round(promedioInstancias, 2)); // <-- CORREGIDO
            }

            // 5. RENDIMIENTO FINAL (Solo si ya completó la cantidad de parciales exigidos)
            if (plan.CantidadParciales.HasValue && notasPorInstancia.Count >= plan.CantidadParciales.Value)
            {
                // LÓGICA DE MODO DE APROBACIÓN:
                // Si Modo = 1 (Instancia a Instancia): TODAS las instancias deben ser >= nota de corte
                // Si Modo = 0 (Promedio): El promedio de las instancias debe ser >= nota de corte
                bool aproboCursada = plan.ModoAprobacionCursada == 1
                    ? notasPorInstancia.All(n => n >= notaDeCorte)
                    : promedioInstancias >= notaDeCorte;

                if (aproboCursada)
                {
                    // Promoción directa
                    bool cumpleNotaPromo = plan.NotaMinimaPromocion.HasValue &&
                        (plan.ModoAprobacionCursada == 1 ? notasPorInstancia.All(n => n >= plan.NotaMinimaPromocion.Value) : promedioInstancias >= plan.NotaMinimaPromocion.Value);

                    bool cumpleAsisPromo = plan.PorcentajeAsistenciaPromocion.HasValue ? (porcentajeAsistencia >= plan.PorcentajeAsistenciaPromocion.Value) : true;

                    if (plan.EsPromocionable == true && cumpleNotaPromo && cumpleAsisPromo && plan.IdEstadoPromocion.HasValue)
                    {
                        return (plan.IdEstadoPromocion.Value, Math.Round(promedioInstancias, 2));
                    }

                    // Regular
                    if (plan.IdEstadoRegular.HasValue)
                    {
                        return (plan.IdEstadoRegular.Value, null); // Null porque va a Mesa Final
                    }
                }

                // Si llegó aquí, no le alcanzó ni para promo ni para regular (Usamos IdEstadoDesaprobado)
                return (plan.IdEstadoDesaprobado ?? 5, Math.Round(promedioInstancias, 2));
            }

            return (1, null); // Aún "Cursando"
        }

        private async Task RecalcularEstadoAlumnoAsync(int idInscripcionCursada)
        {
            var inscripcion = await _context.InscripcionCursada
                .Include(i => i.Nota)
                .Include(i => i.IdComisionNavigation).ThenInclude(c => c.IdPlanMateriaNavigation)
                .FirstOrDefaultAsync(i => i.Id == idInscripcionCursada);

            if (inscripcion == null || inscripcion.CursadaCerrada) return;

            var evaluacionesComision = await _context.Evaluacions.Where(e => e.IdComision == inscripcion.IdComision).ToListAsync();

            var resultado = CalcularEstadoYNotaDefinitiva(inscripcion.IdComisionNavigation.IdPlanMateriaNavigation, inscripcion.Nota.ToList(), evaluacionesComision);

            inscripcion.IdEstadoMateria = resultado.IdEstado;
            inscripcion.NotaFinalCursada = resultado.NotaFinal;

            _context.InscripcionCursada.Update(inscripcion);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CerrarActaComisionAsync(int idComision, string libro, string folio)
        {
            var comision = await _context.Comisions
                .Include(c => c.IdPlanMateriaNavigation)
                .FirstOrDefaultAsync(c => c.Id == idComision);

            if (comision == null) return false;

            var evaluacionesComision = await _context.Evaluacions.Where(e => e.IdComision == idComision).ToListAsync();

            var inscripciones = await _context.InscripcionCursada
                .Include(i => i.Nota)
                .Where(i => i.IdComision == idComision && i.Estado != "Baja")
                .ToListAsync();

            foreach (var inscripcion in inscripciones)
            {
                if (inscripcion.CursadaCerrada) continue;

                var resultado = CalcularEstadoYNotaDefinitiva(comision.IdPlanMateriaNavigation, inscripcion.Nota.ToList(), evaluacionesComision);

                inscripcion.IdEstadoMateria = resultado.IdEstado;
                inscripcion.NotaFinalCursada = resultado.NotaFinal;
                inscripcion.Estado = "Finalizada";
                inscripcion.CursadaCerrada = true;
            }

            comision.Estado = "Cerrada";
            await _context.SaveChangesAsync();
            return true;
        }

        // Métodos de Actas y Evaluaciones permanecen igual
        public async Task<bool> CerrarActaAsync(CierreActaDTO dto)
        {
            var evaluacion = await _context.Evaluacions.FindAsync(dto.IdEvaluacion);
            if (evaluacion == null) return false;

            evaluacion.EstadoActa = "Cerrada";
            evaluacion.FechaCierre = DateTime.Now;
            evaluacion.Libro = dto.Libro;
            evaluacion.Folio = dto.Folio;

            var result = await _context.SaveChangesAsync() > 0;

            if (result) await _notificationService.NotificarCierreActaAsync(evaluacion.Id, evaluacion.Nombre);
            return result;
        }

        public async Task<bool> EditarEvaluacionAsync(EvaluacionDTO dto)
        {
            var eval = await _context.Evaluacions.FindAsync(dto.IdEvaluacion);
            if (eval == null) return false;
            if (eval.EstadoActa == "Cerrada") return false;

            eval.Nombre = dto.Nombre;
            eval.Fecha = DateOnly.FromDateTime(dto.Fecha);
            eval.EsRecuperatorio = dto.EsRecuperatorio;
            eval.IdEvaluacionPadre = dto.IdEvaluacionPadre;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CrearEvaluacionAsync(int idComision, EvaluacionDTO dto)
        {
            var nuevaEval = new Evaluacion
            {
                IdComision = idComision,
                Nombre = dto.Nombre,
                Fecha = DateOnly.FromDateTime(dto.Fecha),
                EsRecuperatorio = dto.EsRecuperatorio,
                IdEvaluacionPadre = dto.IdEvaluacionPadre,
                EstadoActa = "Abierta"
            };
            _context.Evaluacions.Add(nuevaEval);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ReabrirActaAsync(int idEvaluacion)
        {
            var eval = await _context.Evaluacions.FindAsync(idEvaluacion);
            if (eval == null || eval.EstadoActa != "Cerrada") return false;

            eval.EstadoActa = "Abierta";
            eval.FechaCierre = null;
            eval.Libro = null;
            eval.Folio = null;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarEvaluacionAsync(int idEvaluacion)
        {
            var evaluacion = await _context.Evaluacions.Include(e => e.Nota).FirstOrDefaultAsync(e => e.Id == idEvaluacion);
            if (evaluacion == null || evaluacion.EstadoActa == "Cerrada") return false;

            _context.Nota.RemoveRange(evaluacion.Nota);
            _context.Evaluacions.Remove(evaluacion);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleCierreCursadaIndividualAsync(int idInscripcion)
        {
            var inscripcion = await _context.InscripcionCursada.FindAsync(idInscripcion);
            if (inscripcion == null) return false;

            inscripcion.CursadaCerrada = !inscripcion.CursadaCerrada;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReabrirActaComisionAsync(int idComision)
        {
            var comision = await _context.Comisions.Include(c => c.InscripcionCursada).FirstOrDefaultAsync(c => c.Id == idComision);
            if (comision == null) return false;

            comision.Estado = "Abierta";
            foreach (var ins in comision.InscripcionCursada) ins.CursadaCerrada = false;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}