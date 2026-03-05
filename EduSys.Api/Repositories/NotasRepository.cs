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
        // =======================================================================
        // MOTOR DE REGLAS ACADÉMICAS - ENCAPSULADO Y REUTILIZABLE
        // =======================================================================
        // =======================================================================
        // MOTOR DE REGLAS ACADÉMICAS - ENCAPSULADO Y REUTILIZABLE
        // =======================================================================
        private (int? IdEstado, decimal? NotaFinal) CalcularEstadoYNotaDefinitiva(PlanMateria plan, List<Nota> notasAlumno, List<Evaluacion> evaluacionesComision, int idEstadoActual)
        {
            if (!notasAlumno.Any()) return (idEstadoActual, null); // Sin notas, no hay cambios

            // =========================================================================
            // 1. ESTADO LIBRE / ABANDONO: MUERTE SÚBITA
            // =========================================================================
            if (plan.NotaEliminatoria.HasValue && notasAlumno.Any(n => n.Valor < plan.NotaEliminatoria.Value))
            {
                var notasValidas = notasAlumno.Where(n => n.Valor.HasValue).Select(n => n.Valor.Value).ToList();
                decimal prom = notasValidas.Any() ? Math.Round(notasValidas.Average(), 2) : 0m;
                return (plan.IdEstadoSiFaltaAsistencia, prom); // Estado Libre/Abandono
            }

            // =========================================================================
            // 2. SEPARAR TIPOS DE EXÁMENES
            // =========================================================================
            var integrador = evaluacionesComision.FirstOrDefault(e => e.EsIntegrador == true);
            var parcialesPrincipales = evaluacionesComision.Where(e => e.IdEvaluacionPadre == null && e.EsIntegrador != true).ToList();

            var notasPorInstancia = new List<decimal>();

            // =========================================================================
            // 3. CALCULAR LA NOTA DEFINITIVA DE CADA INSTANCIA (PISAR O MAX)
            // =========================================================================
            foreach (var parcial in parcialesPrincipales)
            {
                var notaParcial = notasAlumno.FirstOrDefault(n => n.IdEvaluacion == parcial.Id)?.Valor;
                var recuperatorios = evaluacionesComision.Where(e => e.IdEvaluacionPadre == parcial.Id).ToList();

                bool rindioInstancia = notaParcial.HasValue || notasAlumno.Any(n => recuperatorios.Select(r => r.Id).Contains(n.IdEvaluacion));

                if (rindioInstancia)
                {
                    decimal notaDefinitiva = notaParcial ?? 0m;

                    var notasRecusRendidos = notasAlumno.Where(n => recuperatorios.Select(r => r.Id).Contains(n.IdEvaluacion) && n.Valor.HasValue).ToList();
                    if (notasRecusRendidos.Any())
                    {
                        if (plan.ModoNotaRecuperatorio == 0) // ✅ MODO 0: Queda la nota más alta
                        {
                            decimal maxRecu = notasRecusRendidos.Max(n => n.Valor.Value);
                            notaDefinitiva = Math.Max(notaDefinitiva, maxRecu);
                        }
                        else // ✅ MODO 1: El recuperatorio reemplaza/pisa siempre
                        {
                            var ultimoRecuRendido = notasRecusRendidos.OrderByDescending(n => n.FechaCarga).First();
                            notaDefinitiva = ultimoRecuRendido.Valor.Value;
                        }
                    }
                    notasPorInstancia.Add(notaDefinitiva);
                }
            }

            if (!notasPorInstancia.Any()) return (idEstadoActual, null);

            decimal promedioInstancias = Math.Round(notasPorInstancia.Average(), 2);
            decimal notaDeCorte = plan.PromedioMinimoAprobacion ?? plan.NotaMinimaRegularizar ?? 4m;
            int cantidadAplazos = notasPorInstancia.Count(n => n < notaDeCorte);

            // =========================================================================
            // 4. ESTADO LIBRE / ABANDONO: POR INASISTENCIA O LÍMITE DE APLAZOS
            // =========================================================================
            decimal porcentajeAsistencia = 100m; // Integrar módulo de asistencia futuro
            if (plan.PorcentajeAsistenciaRegularizar.HasValue && porcentajeAsistencia < plan.PorcentajeAsistenciaRegularizar.Value)
            {
                return (plan.IdEstadoSiFaltaAsistencia, promedioInstancias); // Queda Libre
            }

            // "Límite de aplazos" define cuándo el alumno pierde el derecho a cursar
            if (plan.CantidadAplazosParaLibre.HasValue && cantidadAplazos > plan.CantidadAplazosParaLibre.Value)
            {
                return (plan.IdEstadoSiFaltaAsistencia, promedioInstancias); // Queda Libre
            }

            // =========================================================================
            // 5. EVALUACIÓN FINAL DE CURSADA (Rindió todos los exámenes)
            // =========================================================================
            if (plan.CantidadParciales.HasValue && notasPorInstancia.Count >= plan.CantidadParciales.Value)
            {
                bool aproboCursadaBase = plan.ModoAprobacionCursada == 1
                    ? notasPorInstancia.All(n => n >= notaDeCorte)
                    : promedioInstancias >= notaDeCorte;

                if (aproboCursadaBase)
                {
                    // Aprobó limpio -> Verificamos Promoción
                    bool cumplePromo = plan.NotaMinimaPromocion.HasValue &&
                        (plan.ModoAprobacionCursada == 1 ? notasPorInstancia.All(n => n >= plan.NotaMinimaPromocion.Value) : promedioInstancias >= plan.NotaMinimaPromocion.Value);

                    if (plan.EsPromocionable == true && cumplePromo && plan.IdEstadoPromocion.HasValue)
                        return (plan.IdEstadoPromocion.Value, promedioInstancias);

                    return (plan.IdEstadoRegular ?? 2, null);
                }
                else
                {
                    // =================================================================
                    // 6. LÓGICA DE EXAMEN INTEGRADOR Y ESTADO DESAPROBADO
                    // =================================================================
                    int parcialesAprobados = notasPorInstancia.Count(n => n >= notaDeCorte);

                    // ¿Cumple requisitos para rendir integrador?
                    bool tieneDerechoIntegrador = plan.TieneIntegrador &&
                        (!plan.CondicionIntegradorParciales.HasValue || parcialesAprobados >= plan.CondicionIntegradorParciales.Value);

                    if (tieneDerechoIntegrador && integrador != null)
                    {
                        var notaIntegrador = notasAlumno.FirstOrDefault(n => n.IdEvaluacion == integrador.Id)?.Valor;

                        if (notaIntegrador.HasValue)
                        {
                            decimal minAprobarInt = plan.NotaAprobacionIntegrador ?? notaDeCorte;

                            if (notaIntegrador.Value >= minAprobarInt)
                            {
                                // Aprobó el Integrador -> ¿Lo salva o lo promociona?
                                if (plan.IntegradorPermitePromocion && plan.NotaPromocionIntegrador.HasValue &&
                                    notaIntegrador.Value >= plan.NotaPromocionIntegrador.Value && plan.IdEstadoPromocion.HasValue)
                                {
                                    return (plan.IdEstadoPromocion.Value, notaIntegrador.Value); // Salva y Promociona
                                }

                                return (plan.IdEstadoRegular ?? 2, null); // Salvó la cursada
                            }
                            else
                            {
                                // Rindió el integrador y le fue mal
                                return (plan.IdEstadoSiDesaprueba, notaIntegrador.Value); // ESTADO DESAPROBADO
                            }
                        }
                        else
                        {
                            // Cumple los requisitos, existe el integrador, PERO AÚN NO LO RINDE. Sigue en carrera.
                            return (idEstadoActual, promedioInstancias);
                        }
                    }

                    // Si NO tiene derecho a integrador (o la materia no lo tiene), entonces reprueba la cursada
                    return (plan.IdEstadoSiDesaprueba, promedioInstancias); // ESTADO DESAPROBADO
                }
            }

            return (idEstadoActual, promedioInstancias); // Sigue cursando...
        }
        private async Task RecalcularEstadoAlumnoAsync(int idInscripcionCursada)
        {
            var inscripcion = await _context.InscripcionCursada
                .Include(i => i.Nota)
                .Include(i => i.IdComisionNavigation).ThenInclude(c => c.IdPlanMateriaNavigation)
                .FirstOrDefaultAsync(i => i.Id == idInscripcionCursada);

            if (inscripcion == null || inscripcion.CursadaCerrada) return;

            var evaluacionesComision = await _context.Evaluacions.Where(e => e.IdComision == inscripcion.IdComision).ToListAsync();

            var resultado = CalcularEstadoYNotaDefinitiva(inscripcion.IdComisionNavigation.IdPlanMateriaNavigation, inscripcion.Nota.ToList(), evaluacionesComision, inscripcion.IdEstadoMateria ?? 1);

            if (resultado.IdEstado.HasValue) inscripcion.IdEstadoMateria = resultado.IdEstado.Value;
            inscripcion.NotaFinalCursada = resultado.NotaFinal;

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

                var resultado = CalcularEstadoYNotaDefinitiva(comision.IdPlanMateriaNavigation, inscripcion.Nota.ToList(), evaluacionesComision, inscripcion.IdEstadoMateria ?? 1);

                if (resultado.IdEstado.HasValue) inscripcion.IdEstadoMateria = resultado.IdEstado.Value;
                inscripcion.NotaFinalCursada = resultado.NotaFinal;


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