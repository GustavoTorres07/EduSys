using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Api.Services.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class ActasRepository : IActasRepository
    {
        private readonly EduSysDbContext _context;
        private readonly INotificationService _notificationService;

        public ActasRepository(EduSysDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // =======================================================================
        // 🛠️ MOTOR DE GENERACIÓN DE NOMENCLATURA LEGAL
        // =======================================================================
        private async Task<string> GenerarNumeroActaAsync(string prefijoTipo)
        {
            // Formato deseado: PAR-26-00015
            string anio = DateTime.Now.ToString("yy");
            string prefijoBusqueda = $"{prefijoTipo}-{anio}-";

            var ultimaActa = await _context.ActaAlumnos
                .Where(a => a.NumeroActa.StartsWith(prefijoBusqueda))
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();

            int correlativo = 1;
            if (ultimaActa != null)
            {
                string ultNumStr = ultimaActa.NumeroActa.Replace(prefijoBusqueda, "");
                if (int.TryParse(ultNumStr, out int ultNum))
                {
                    correlativo = ultNum + 1;
                }
            }

            return $"{prefijoBusqueda}{correlativo:D5}";
        }


        // =======================================================================
        // CIERRE DE CURSADA GLOBAL (Genera Actas "CUR")
        // =======================================================================
        public async Task<bool> CerrarActaComisionAsync(int idComision)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var comision = await _context.Comisions
                    .Include(c => c.IdPlanMateriaNavigation)
                    .FirstOrDefaultAsync(c => c.Id == idComision);

                if (comision == null) return false;

                comision.Estado = "Cerrada";

                var evaluaciones = await _context.Evaluacions.AsNoTracking().Where(e => e.IdComision == idComision).ToListAsync();
                var inscripciones = await _context.InscripcionCursada.Include(i => i.Nota).Where(i => i.IdComision == idComision && i.Estado != "Baja").ToListAsync();

                foreach (var ins in inscripciones)
                {
                    if (ins.CursadaCerrada) continue;

                    // 1. Calcular promedio
                    var res = CalcularEstadoYNotaDefinitiva(comision.IdPlanMateriaNavigation, ins.Nota.ToList(), evaluaciones, ins.IdEstadoMateria ?? 1);

                    ins.IdEstadoMateria = res.IdEstado;
                    ins.NotaFinalCursada = res.NotaFinal;
                    ins.Estado = "Finalizada";
                    ins.CursadaCerrada = true;

                    // 2. Determinar nombre del estado en texto para el Acta
                    string nombreEstado = "Cursando";
                    var objEstado = await _context.EstadoMaterias.FindAsync(res.IdEstado);
                    if (objEstado != null) nombreEstado = objEstado.Nombre;

                    // 🚀 3. GENERAR ACTA INDIVIDUAL DE CURSADA
                    var nuevaActa = new ActaAlumno
                    {
                        NumeroActa = await GenerarNumeroActaAsync("CUR"),
                        IdAlumno = ins.IdAlumno,
                        IdPlanMateria = comision.IdPlanMateria,
                        TipoActa = "Cierre de Cursada",
                        Detalle = $"Comisión {comision.Codigo}",
                        FechaEmision = DateTime.Now,
                        Nota = res.NotaFinal,
                        EstadoAcademico = nombreEstado,
                        IdInscripcionCursadaReferencia = ins.Id
                    };

                    _context.ActaAlumnos.Add(nuevaActa);
                    await _context.SaveChangesAsync(); // Guardamos una por una para asegurar el correlativo secuencial
                }

                await trans.CommitAsync();
                return true;
            }
            catch
            {
                await trans.RollbackAsync();
                return false;
            }
        }

        // =======================================================================
        // CIERRE DE EXAMEN PARCIAL/RECUPERATORIO (Genera Actas "PAR" o "REC")
        // =======================================================================
        // =======================================================================
        // CIERRE DE EXAMEN PARCIAL/RECUPERATORIO (Genera Actas "PAR" o "REC")
        // =======================================================================
        // =======================================================================
        // CIERRE DE EXAMEN PARCIAL/RECUPERATORIO (Genera Actas "PAR" o "REC")
        // =======================================================================
        public async Task<bool> CerrarActaAsync(int idEvaluacion)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Buscamos la evaluación incluyendo el PlanMateria para leer las reglas
                var eval = await _context.Evaluacions
                    .Include(e => e.IdComisionNavigation)
                        .ThenInclude(c => c.IdPlanMateriaNavigation)
                    .FirstOrDefaultAsync(e => e.Id == idEvaluacion);

                if (eval == null) return false;

                eval.EstadoActa = "Cerrada";
                eval.FechaCierre = DateTime.Now;

                // 2. Buscamos las notas
                var notas = await _context.Nota
                    .Include(n => n.IdInscripcionCursadaNavigation)
                    .Where(n => n.IdEvaluacion == idEvaluacion)
                    .ToListAsync();

                string prefijo = eval.EsRecuperatorio == true ? "REC" : "PAR";
                string tipoStr = eval.EsRecuperatorio == true ? "Recuperatorio" : "Parcial";

                // 🚀 FIX: Apuntamos exactamente a la columna que editas en la interfaz de "Reglas de Cursada"
                var plan = eval.IdComisionNavigation.IdPlanMateriaNavigation;
                decimal notaDeCorte = plan.PromedioMinimoAprobacion ?? plan.NotaMinimaRegularizar ?? 4m;

                foreach (var nota in notas)
                {
                    // Aplicamos la regla estricta
                    string estadoParcial = (nota.Valor >= notaDeCorte) ? "Aprobado" : "Desaprobado";

                    var nuevaActa = new ActaAlumno
                    {
                        NumeroActa = await GenerarNumeroActaAsync(prefijo),
                        IdAlumno = nota.IdInscripcionCursadaNavigation.IdAlumno,
                        IdPlanMateria = eval.IdComisionNavigation.IdPlanMateria,
                        TipoActa = tipoStr,
                        Detalle = eval.Nombre,
                        FechaEmision = DateTime.Now,
                        Nota = nota.Valor,
                        EstadoAcademico = estadoParcial,
                        IdEvaluacionReferencia = eval.Id
                    };

                    _context.ActaAlumnos.Add(nuevaActa);
                    await _context.SaveChangesAsync();
                }

                await trans.CommitAsync();
                await _notificationService.NotificarCierreActaAsync(eval.Id, eval.Nombre);

                return true;
            }
            catch
            {
                await trans.RollbackAsync();
                return false;
            }
        }

        // =======================================================================
        // LECTURA DE HISTORIAL DESDE LA NUEVA TABLA (Extremadamente optimizado)
        // =======================================================================
        public async Task<List<ActaResumenDTO>> GetActasPorAlumnoAsync(int idAlumno)
        {
            var actas = await _context.ActaAlumnos
                .AsNoTracking()
                .Include(a => a.IdPlanMateriaNavigation)
                    .ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(a => a.IdEvaluacionReferenciaNavigation) // Para parciales
                    .ThenInclude(e => e.IdComisionNavigation)
                        .ThenInclude(c => c.DocenteComisions)
                            .ThenInclude(dc => dc.IdDocenteNavigation)
                                .ThenInclude(d => d.IdUsuarioNavigation)
                .Include(a => a.IdInscripcionCursadaReferenciaNavigation) // Para cursadas
                    .ThenInclude(i => i.IdComisionNavigation)
                        .ThenInclude(c => c.DocenteComisions)
                            .ThenInclude(dc => dc.IdDocenteNavigation)
                                .ThenInclude(d => d.IdUsuarioNavigation)
                .Include(a => a.IdInscripcionFinalReferenciaNavigation) // Para finales
                    .ThenInclude(i => i.IdMesaFinalNavigation)
                        .ThenInclude(m => m.IdPresidenteMesaNavigation)
                            .ThenInclude(d => d.IdUsuarioNavigation)
                .Include(a => a.IdDocenteFirmaNavigation) // Si hay firma explícita
                    .ThenInclude(d => d.IdUsuarioNavigation)
                .Where(a => a.IdAlumno == idAlumno)
                .OrderByDescending(a => a.FechaEmision)
                .ToListAsync();

            return actas.Select(a =>
            {
                // 🚀 Lógica para extraer el docente (Igual a la del PDF)
                string nombreProfesor = "A designar";

                var docentesComision = a.IdEvaluacionReferenciaNavigation?.IdComisionNavigation?.DocenteComisions
                                    ?? a.IdInscripcionCursadaReferenciaNavigation?.IdComisionNavigation?.DocenteComisions;

                if (a.IdDocenteFirmaNavigation != null)
                {
                    nombreProfesor = $"{a.IdDocenteFirmaNavigation.IdUsuarioNavigation.Nombre} {a.IdDocenteFirmaNavigation.IdUsuarioNavigation.Apellido}";
                }
                else if (docentesComision != null && docentesComision.Any())
                {
                    var docentePrincipal = docentesComision.FirstOrDefault(dc => dc.RolDocente == "Titular")
                                         ?? docentesComision.First();
                    nombreProfesor = $"{docentePrincipal.IdDocenteNavigation.IdUsuarioNavigation.Nombre} {docentePrincipal.IdDocenteNavigation.IdUsuarioNavigation.Apellido}";
                }
                else if (a.IdInscripcionFinalReferenciaNavigation?.IdMesaFinalNavigation?.IdPresidenteMesaNavigation != null)
                {
                    var presidente = a.IdInscripcionFinalReferenciaNavigation.IdMesaFinalNavigation.IdPresidenteMesaNavigation;
                    nombreProfesor = $"{presidente.IdUsuarioNavigation.Nombre} {presidente.IdUsuarioNavigation.Apellido}";
                }

                return new ActaResumenDTO
                {
                    IdActa = a.Id,
                    TipoActa = a.TipoActa,
                    Materia = a.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre ?? "S/N",
                    Detalle = a.Detalle,
                    Libro = a.NumeroActa,
                    Folio = "",
                    NotaAlumno = a.Nota,
                    EstadoAlumno = a.EstadoAcademico,
                    IdReferencia = a.Id,
                    DocenteTitular = nombreProfesor // 👈 Asignamos la nueva propiedad
                };
            }).ToList();
        }


        // =======================================================================
        // METODOS ADMINISTRATIVOS
        // =======================================================================
        public async Task<bool> ReabrirActaAsync(int idEvaluacion)
        {
            var eval = await _context.Evaluacions.FindAsync(idEvaluacion);
            if (eval == null || eval.EstadoActa != "Cerrada") return false;

            eval.EstadoActa = "Abierta";
            eval.FechaCierre = null;

            // Opcional: Eliminar las actas individuales generadas si se reabre. 
            // Legalmente se suele hacer un acta de anulación, pero para simplificar, las borramos:
            var actasAEliminar = await _context.ActaAlumnos.Where(a => a.IdEvaluacionReferencia == idEvaluacion).ToListAsync();
            _context.ActaAlumnos.RemoveRange(actasAEliminar);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleCierreCursadaIndividualAsync(int idInsc)
        {
            var ins = await _context.InscripcionCursada.FindAsync(idInsc);
            if (ins == null) return false;
            ins.CursadaCerrada = !ins.CursadaCerrada;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ReabrirActaComisionAsync(int idCom)
        {
            try
            {
                var com = await _context.Comisions
                    .Include(c => c.InscripcionCursada)
                    .FirstOrDefaultAsync(c => c.Id == idCom);

                if (com == null) return false;

                com.Estado = "Abierta";

                // 🚀 FIX: Extraemos los IDs a una lista plana en memoria ANTES de ir a la BD.
                // Esto evita el colapso (Error 500) de Entity Framework.
                var inscripcionesIds = com.InscripcionCursada.Select(i => i.Id).ToList();

                foreach (var i in com.InscripcionCursada)
                {
                    i.CursadaCerrada = false;

                    // Le devolvemos el estado original de cursada al alumno
                    if (i.Estado == "Finalizada")
                    {
                        i.Estado = "Cursando";
                    }
                }

                // Usamos la lista plana para buscar y borrar las actas generadas
                if (inscripcionesIds.Any())
                {
                    var actasAEliminar = await _context.ActaAlumnos
                        .Where(a => a.IdInscripcionCursadaReferencia != null && inscripcionesIds.Contains(a.IdInscripcionCursadaReferencia.Value))
                        .ToListAsync();

                    if (actasAEliminar.Any())
                    {
                        _context.ActaAlumnos.RemoveRange(actasAEliminar);
                    }
                }

                // Guardamos todo de una sola vez
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // Si ocurre cualquier otro error interno, lo atajamos para no romper el servidor
                return false;
            }
        }

        // =======================================================================
        // LÓGICA DE CÁLCULO DE PROMEDIOS
        // =======================================================================
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
    }
}