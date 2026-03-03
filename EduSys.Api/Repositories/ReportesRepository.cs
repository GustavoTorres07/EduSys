using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSys.Api.Repositories
{
    public class ReportesRepository : IReportesRepository
    {
        private readonly EduSysDbContext _context;

        public ReportesRepository(EduSysDbContext context)
        {
            _context = context;
        }

        // 1. CONSTANCIA DE ALUMNO REGULAR / INSCRIPCIÓN
        // En ReportesRepository.cs

        public async Task<ConstanciaInscripcionDTO> GetDatosConstanciaAsync(int idAlumno, int idPeriodo)
        {
            var alumno = await _context.Alumnos
                .Include(a => a.IdUsuarioNavigation)
                .Include(a => a.IdPlanActualNavigation).ThenInclude(p => p.IdCarreraNavigation)
                .Include(a => a.IdSedeNavigation)
                .FirstOrDefaultAsync(a => a.Id == idAlumno);

            if (alumno == null) return null;

            var periodo = await _context.PeriodoAcademicos.FindAsync(idPeriodo);

            var inscripciones = await _context.InscripcionCursada
                .Include(i => i.IdComisionNavigation).ThenInclude(c => c.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(i => i.IdComisionNavigation).ThenInclude(c => c.HorarioComisions)
                .Where(i => i.IdAlumno == idAlumno && i.IdComisionNavigation.IdPeriodo == idPeriodo && i.Estado != "Baja")
                .ToListAsync();

            return new ConstanciaInscripcionDTO
            {
                InstitucionNombre = "EduSys - Instituto de Tecnología",
                FechaEmision = DateTime.Now,
                PeriodoAcademico = periodo?.Nombre ?? "Ciclo Lectivo",
                AlumnoNombre = $"{alumno.IdUsuarioNavigation.Apellido}, {alumno.IdUsuarioNavigation.Nombre}",
                Dni = alumno.IdUsuarioNavigation.Dni,
                Legajo = alumno.Legajo,
                Carrera = alumno.IdPlanActualNavigation?.IdCarreraNavigation?.Nombre ?? "-",
                Sede = alumno.IdSedeNavigation?.Nombre ?? "Central",

                // Mapeo y Ordenamiento
                Materias = inscripciones.Select(i => new DetalleMateriaConstanciaDTO
                {
                    // Aseguramos que CodigoMateria no sea null
                    CodigoMateria = i.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Codigo ?? "-",
                    Materia = i.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    Comision = i.IdComisionNavigation.Codigo,
                    AnioCursada = i.IdComisionNavigation.IdPlanMateriaNavigation.AnioCursada,
                    FechaInscripcion = i.FechaInscripcion ?? DateTime.Now,
                    // Formato corto de horarios para que entre en la tabla
                    Horarios = string.Join(" / ", i.IdComisionNavigation.HorarioComisions
                        .Select(h => $"{h.DiaSemana.Substring(0, 3)} {h.HoraInicio:hh\\:mm}-{h.HoraFin:hh\\:mm}"))
                })
                .OrderBy(x => x.AnioCursada)
                .ThenBy(x =>
                {
                    // Lógica de ordenamiento numérico por código (TSDS-09 -> 9)
                    if (!string.IsNullOrEmpty(x.CodigoMateria) && x.CodigoMateria.Contains("-"))
                    {
                        var parts = x.CodigoMateria.Split('-');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int num))
                            return num;
                    }
                    return 9999;
                })
                .ToList()
            };
        }

        // 2. REPORTE GLOBAL (ADMIN)
        public async Task<List<InscripcionGlobalDTO>> GetReporteGlobalAsync(int idPeriodo, int? idCarrera)
        {
            var query = _context.InscripcionCursada
                .Include(i => i.IdAlumnoNavigation).ThenInclude(a => a.IdUsuarioNavigation)
                .Include(i => i.IdComisionNavigation).ThenInclude(c => c.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(i => i.IdComisionNavigation).ThenInclude(c => c.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdPlanNavigation).ThenInclude(p => p.IdCarreraNavigation)
                .Where(i => i.IdComisionNavigation.IdPeriodo == idPeriodo && i.Estado != "Baja")
                .AsQueryable();

            if (idCarrera.HasValue && idCarrera.Value > 0)
            {
                query = query.Where(i => i.IdComisionNavigation.IdPlanMateriaNavigation.IdPlanNavigation.IdCarrera == idCarrera.Value);
            }

            return await query.Select(i => new InscripcionGlobalDTO
            {
                IdInscripcion = i.Id,
                Fecha = i.FechaInscripcion ?? DateTime.Now,
                AlumnoNombre = $"{i.IdAlumnoNavigation.IdUsuarioNavigation.Apellido}, {i.IdAlumnoNavigation.IdUsuarioNavigation.Nombre}",
                AlumnoLegajo = i.IdAlumnoNavigation.Legajo,
                AlumnoDni = i.IdAlumnoNavigation.IdUsuarioNavigation.Dni,
                Carrera = i.IdComisionNavigation.IdPlanMateriaNavigation.IdPlanNavigation.IdCarreraNavigation.Nombre,
                Materia = i.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                Comision = $"{i.IdComisionNavigation.Codigo} ({i.IdComisionNavigation.Turno})",
                Estado = i.Estado
            })
            .OrderByDescending(x => x.Fecha)
            .ToListAsync();
        }


        public async Task<CertificadoAlumnoRegularDTO> GetDatosCertificadoRegularAsync(int idAlumno, int idPeriodo)
        {
            // 1. Datos básicos
            var alumno = await _context.Alumnos
                .Include(a => a.IdUsuarioNavigation)
                .Include(a => a.IdPlanActualNavigation).ThenInclude(p => p.IdCarreraNavigation)
                .Include(a => a.IdSedeNavigation)
                .FirstOrDefaultAsync(a => a.Id == idAlumno);

            if (alumno == null) return null;

            var periodo = await _context.PeriodoAcademicos.FindAsync(idPeriodo);

            // 2. Validar Regularidad: ¿Tiene materias cursando o regularizadas en este periodo?
            // Opcional: Podrías hacer un chequeo más estricto aquí si quisieras.
            bool esRegular = await _context.InscripcionCursada
                .AnyAsync(i => i.IdAlumno == idAlumno
                               && i.IdComisionNavigation.IdPeriodo == idPeriodo
                               && i.Estado != "Baja");

            // Si no está cursando nada, técnicamente no es alumno regular del ciclo (depende de tu regla de negocio)
            if (!esRegular) return null;

            return new CertificadoAlumnoRegularDTO
            {
                InstitucionNombre = "EduSys - Instituto de Tecnología",
                FechaEmision = DateTime.Now,
                PeriodoAcademico = periodo?.Nombre ?? "Ciclo Lectivo",
                AlumnoNombre = $"{alumno.IdUsuarioNavigation.Apellido}, {alumno.IdUsuarioNavigation.Nombre}",
                Dni = alumno.IdUsuarioNavigation.Dni,
                Legajo = alumno.Legajo,
                Carrera = alumno.IdPlanActualNavigation?.IdCarreraNavigation?.Nombre ?? "-",
                Sede = alumno.IdSedeNavigation?.Nombre ?? "Central",
                Ciudad = "Santa Rosa", // Podrías sacarlo de la Sede si tiene el campo
                Provincia = "La Pampa"
            };
        }

        // 3. REPORTE RESUMEN ALUMNOS (ADMIN)
        public async Task<List<AlumnoResumenInscripcionDTO>> GetAlumnosInscriptosAsync(int idPeriodo, int idCarrera, int? idSede)
        {
            var query = _context.InscripcionCursada
                .Include(i => i.IdAlumnoNavigation).ThenInclude(a => a.IdUsuarioNavigation)
                .Include(i => i.IdComisionNavigation).ThenInclude(c => c.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdPlanNavigation)
                .Where(i => i.IdComisionNavigation.IdPeriodo == idPeriodo
                            && i.Estado != "Baja"
                            && i.IdComisionNavigation.IdPlanMateriaNavigation.IdPlanNavigation.IdCarrera == idCarrera)
                .AsQueryable();

            if (idSede.HasValue && idSede.Value > 0)
            {
                query = query.Where(i => i.IdComisionNavigation.IdSede == idSede.Value);
            }

            return await query
                .GroupBy(i => i.IdAlumno)
                .Select(g => new AlumnoResumenInscripcionDTO
                {
                    IdAlumno = g.Key,
                    NombreCompleto = $"{g.First().IdAlumnoNavigation.IdUsuarioNavigation.Apellido}, {g.First().IdAlumnoNavigation.IdUsuarioNavigation.Nombre}",
                    Legajo = g.First().IdAlumnoNavigation.Legajo,
                    Dni = g.First().IdAlumnoNavigation.IdUsuarioNavigation.Dni,
                    CantidadMaterias = g.Count()
                })
                .ToListAsync();
        }

        public async Task<HistoriaAcademicaDTO> GetHistoriaAcademicaAsync(int idAlumno)
        {
            // 1. Obtener Alumno y su Plan
            var alumno = await _context.Alumnos
                .Include(a => a.IdUsuarioNavigation)
                .Include(a => a.IdPlanActualNavigation)
                    .ThenInclude(p => p.IdCarreraNavigation)
                .FirstOrDefaultAsync(a => a.Id == idAlumno);

            if (alumno == null || alumno.IdPlanActual == null) return null;

            int idPlan = alumno.IdPlanActual.Value;

            // 2. Obtener TODAS las materias del Plan
            var planMaterias = await _context.PlanMateria
                .Include(pm => pm.IdMateriaNavigation)
                .Where(pm => pm.IdPlan == idPlan)
                .ToListAsync();

            // 3. Obtener Cursadas del Alumno
            var cursadas = await _context.InscripcionCursada
                .Include(ic => ic.IdComisionNavigation)
                .Where(ic => ic.IdAlumno == idAlumno && ic.Estado != "Baja")
                .ToListAsync();

            // 4. Obtener Finales del Alumno
            var finales = await _context.InscripcionFinals
                .Include(ifinal => ifinal.IdMesaFinalNavigation)
                .Where(ifinal => ifinal.IdAlumno == idAlumno && ifinal.Estado != "Anulado")
                .ToListAsync();

            // 5. Construir el DTO
            var historia = new HistoriaAcademicaDTO
            {
                AlumnoNombre = $"{alumno.IdUsuarioNavigation.Apellido}, {alumno.IdUsuarioNavigation.Nombre}",
                Legajo = alumno.Legajo,
                Carrera = alumno.IdPlanActualNavigation.IdCarreraNavigation.Nombre,
                Plan = alumno.IdPlanActualNavigation.Nombre,
                TotalMateriasPlan = planMaterias.Count,
                Detalle = new List<DetalleMateriaAvanceDTO>() // Inicializar lista
            };

            decimal sumaNotas = 0;
            int cantidadAprobadas = 0;

            foreach (var pm in planMaterias)
            {
                // ✅ CORRECCIÓN: Usar DetalleMateriaAvanceDTO
                var item = new DetalleMateriaAvanceDTO
                {
                    IdPlanMateria = pm.Id,
                    AnioCursada = pm.AnioCursada,
                    Materia = pm.IdMateriaNavigation.Nombre,
                    Codigo = pm.IdMateriaNavigation.Codigo ?? "-",
                    Estado = "Pendiente"
                };

                // --- LÓGICA DE ESTADO ---

                // A. Buscar Aprobación por FINAL
                var finalAprobado = finales
                    .Where(f => f.IdMesaFinalNavigation.IdPlanMateria == pm.Id && f.Nota >= pm.NotaMinimaAprobacion)
                    .OrderByDescending(f => f.FechaInscripcion)
                    .FirstOrDefault();

                if (finalAprobado != null)
                {
                    item.Estado = "Aprobada";
                    item.Nota = finalAprobado.Nota;
                    item.Fecha = finalAprobado.FechaInscripcion;

                    // Sumar al promedio solo si es nota numérica válida
                    if (item.Nota.HasValue)
                    {
                        sumaNotas += item.Nota.Value;
                        cantidadAprobadas++;
                    }
                }
                else
                {
                    // B. Buscar Aprobación por PROMOCIÓN o Estado de Cursada
                    var cursada = cursadas
                        .FirstOrDefault(c => c.IdComisionNavigation.IdPlanMateria == pm.Id);

                    if (cursada != null)
                    {
                        if (cursada.CondicionFinal == "Promocionado")
                        {
                            item.Estado = "Promocionada";
                            item.Nota = cursada.NotaFinalCursada;
                            item.Fecha = cursada.FechaInscripcion;

                            if (item.Nota.HasValue)
                            {
                                sumaNotas += item.Nota.Value;
                                cantidadAprobadas++;
                            }
                        }
                        else if (cursada.CondicionFinal == "Regular")
                        {
                            item.Estado = "Regular";
                            item.Fecha = cursada.FechaInscripcion;
                        }
                        else if (cursada.EsLibre)
                        {
                            item.Estado = "Libre";
                        }
                        else if (cursada.Estado == "Cursando")
                        {
                            item.Estado = "Cursando";
                        }
                    }
                }

                historia.Detalle.Add(item);
            }

            // Ordenamiento
            historia.Detalle = historia.Detalle
                .OrderBy(x => x.AnioCursada)
                .ThenBy(x =>
                {
                    if (string.IsNullOrEmpty(x.Codigo)) return 9999;
                    var partes = x.Codigo.Split('-');
                    if (partes.Length >= 2 && int.TryParse(partes.Last(), out int numero))
                        return numero;
                    return 9999;
                })
                .ToList();

            // 6. Totales
            historia.MateriasAprobadas = cantidadAprobadas;

            if (cantidadAprobadas > 0)
            {
                historia.PromedioGeneral = Math.Round(sumaNotas / cantidadAprobadas, 2);
            }

            // Cálculo de porcentaje
            if (historia.TotalMateriasPlan > 0)
            {
                historia.PorcentajeAvance = Math.Round(((double)historia.MateriasAprobadas / historia.TotalMateriasPlan) * 100, 2);
            }

            return historia;
        }

        public async Task<ConstanciaFinalDTO?> GetDatosConstanciaFinalAsync(int idInscripcion, int idAlumno)
        {
            var inscripcion = await _context.InscripcionFinals
                .Include(i => i.IdAlumnoNavigation).ThenInclude(a => a.IdUsuarioNavigation)
                .Include(i => i.IdAlumnoNavigation).ThenInclude(a => a.IdPlanActualNavigation).ThenInclude(p => p.IdCarreraNavigation)
                .Include(i => i.IdMesaFinalNavigation).ThenInclude(m => m.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(i => i.IdMesaFinalNavigation).ThenInclude(m => m.IdPresidenteMesaNavigation).ThenInclude(d => d.IdUsuarioNavigation)
                .FirstOrDefaultAsync(i => i.Id == idInscripcion && i.IdAlumno == idAlumno && i.Estado != "Baja");

            if (inscripcion == null) return null;

            return new ConstanciaFinalDTO
            {
                AlumnoNombreCompleto = $"{inscripcion.IdAlumnoNavigation.IdUsuarioNavigation.Apellido}, {inscripcion.IdAlumnoNavigation.IdUsuarioNavigation.Nombre}",
                AlumnoDNI = inscripcion.IdAlumnoNavigation.IdUsuarioNavigation.Dni,
                AlumnoLegajo = inscripcion.IdAlumnoNavigation.Legajo,
                CarreraNombre = inscripcion.IdAlumnoNavigation.IdPlanActualNavigation?.IdCarreraNavigation?.Nombre ?? "Sin Carrera",
                MateriaNombre = inscripcion.IdMesaFinalNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                FechaExamen = inscripcion.IdMesaFinalNavigation.FechaHora,
                Tribunal = $"{inscripcion.IdMesaFinalNavigation.IdPresidenteMesaNavigation.IdUsuarioNavigation.Apellido} (Pres.)",
                Condicion = inscripcion.Estado ?? "Regular",
                FechaInscripcion = inscripcion.FechaInscripcion ?? DateTime.Now,
                NumeroTransaccion = inscripcion.Id
            };
        }
    }
}