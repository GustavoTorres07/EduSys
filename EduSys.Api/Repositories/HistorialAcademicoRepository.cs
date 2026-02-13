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
    public class HistorialAcademicoRepository : IHistorialAcademicoRepository
    {
        private readonly EduSysDbContext _context;

        public HistorialAcademicoRepository(EduSysDbContext context)
        {
            _context = context;
        }

        // 1. VISTA AVANCE (Lo que ve el alumno como "Analítico")
        public async Task<HistoriaAcademicaDTO> GetAvanceCarreraAsync(int idAlumno)
        {
            var alumno = await _context.Alumnos
                .Include(a => a.IdUsuarioNavigation)
                .Include(a => a.IdPlanActualNavigation).ThenInclude(p => p.IdCarreraNavigation)
                .FirstOrDefaultAsync(a => a.Id == idAlumno);

            if (alumno == null || alumno.IdPlanActual == null) return null;

            int idPlan = alumno.IdPlanActual.Value;

            // Traemos TODAS las materias del plan
            var planMaterias = await _context.PlanMateria
                .Include(pm => pm.IdMateriaNavigation)
                .Where(pm => pm.IdPlan == idPlan)
                .ToListAsync();

            // Traemos solo lo APROBADO o REGULARIZADO (Estado actual)
            var cursadas = await _context.InscripcionCursada
                .Include(i => i.IdComisionNavigation)
                .Where(i => i.IdAlumno == idAlumno && i.Estado != "Baja")
                .ToListAsync();

            var finales = await _context.InscripcionFinals
                .Include(f => f.IdMesaFinalNavigation)
                .Where(f => f.IdAlumno == idAlumno && f.Nota >= 4) // Solo finales aprobados
                .ToListAsync();

            var historia = new HistoriaAcademicaDTO
            {
                AlumnoNombre = $"{alumno.IdUsuarioNavigation.Apellido}, {alumno.IdUsuarioNavigation.Nombre}",
                Legajo = alumno.Legajo,
                Carrera = alumno.IdPlanActualNavigation.IdCarreraNavigation.Nombre,
                Plan = alumno.IdPlanActualNavigation.Nombre,
                TotalMateriasPlan = planMaterias.Count,
                Detalle = new List<DetalleMateriaAvanceDTO>()
            };

            decimal sumaNotas = 0;
            int cantidadAprobadas = 0;

            foreach (var pm in planMaterias)
            {
                var item = new DetalleMateriaAvanceDTO
                {
                    IdPlanMateria = pm.Id,
                    AnioCursada = pm.AnioCursada,
                    Materia = pm.IdMateriaNavigation.Nombre,
                    Codigo = pm.IdMateriaNavigation.Codigo ?? "-",
                    Estado = "Pendiente"
                };

                // Prioridad 1: Final Aprobado
                var final = finales.FirstOrDefault(f => f.IdMesaFinalNavigation.IdPlanMateria == pm.Id);
                if (final != null)
                {
                    item.Estado = "Aprobada";
                    item.Nota = final.Nota;
                    item.Fecha = final.FechaInscripcion;

                    if (final.Nota.HasValue)
                    {
                        sumaNotas += final.Nota.Value;
                        cantidadAprobadas++;
                    }
                }
                else
                {
                    // Prioridad 2: Cursada (Promoción o Regular)
                    var cursada = cursadas.FirstOrDefault(c => c.IdComisionNavigation.IdPlanMateria == pm.Id);
                    if (cursada != null)
                    {
                        if (cursada.CondicionFinal == "Promocionado")
                        {
                            item.Estado = "Promocionada";
                            item.Nota = cursada.NotaFinalCursada;
                            item.Fecha = cursada.FechaInscripcion;

                            if (cursada.NotaFinalCursada.HasValue)
                            {
                                sumaNotas += cursada.NotaFinalCursada.Value;
                                cantidadAprobadas++;
                            }
                        }
                        else if (cursada.CondicionFinal == "Regular")
                        {
                            item.Estado = "Regular"; // Debe final
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

            // Estadísticas
            historia.MateriasAprobadas = cantidadAprobadas;
            if (cantidadAprobadas > 0)
            {
                historia.PromedioGeneral = Math.Round(sumaNotas / cantidadAprobadas, 2);
            }

            if (historia.TotalMateriasPlan > 0)
            {
                historia.PorcentajeAvance = Math.Round(((double)cantidadAprobadas / historia.TotalMateriasPlan) * 100, 2);
            }

            // Ordenamiento por Código Numérico
            historia.Detalle = historia.Detalle
                .OrderBy(x => x.AnioCursada)
                .ThenBy(x => {
                    var partes = x.Codigo.Split('-');
                    if (partes.Length >= 2 && int.TryParse(partes.Last(), out int num)) return num;
                    return 9999;
                })
                .ToList();

            return historia;
        }

        // 2. VISTA CRONOLÓGICA (Timeline completo)
        // ✅ CORREGIDO: Usamos PeriodoHistorialDTO (El nombre correcto)
        public async Task<List<PeriodoHistorialDTO>> GetHistorialCronologicoAsync(int idAlumno)
        {
            // 1. Traer TODA la info relacionada a las cursadas del alumno
            var cursadas = await _context.InscripcionCursada
                .Include(i => i.IdComisionNavigation)
                    .ThenInclude(c => c.IdPeriodoNavigation) // Para agrupar por periodo
                .Include(i => i.IdComisionNavigation)
                    .ThenInclude(c => c.IdPlanMateriaNavigation)
                    .ThenInclude(pm => pm.IdMateriaNavigation) // Para el nombre de la materia
                .Include(i => i.Nota) // ✅ CORREGIDO: Singular
                    .ThenInclude(n => n.IdEvaluacionNavigation)
                .Include(i => i.Asistencia) // ✅ CORREGIDO: Singular
                .Where(i => i.IdAlumno == idAlumno && i.Estado != "Baja")
                .ToListAsync();

            // 2. Agrupar y Proyectar
            var historial = cursadas
                .GroupBy(c => c.IdComisionNavigation.IdPeriodoNavigation)
                .OrderByDescending(g => g.Key.FechaInicio)
                // ✅ CORREGIDO: new PeriodoHistorialDTO
                .Select(grupoPeriodo => new PeriodoHistorialDTO
                {
                    IdPeriodo = grupoPeriodo.Key.Id,
                    NombrePeriodo = grupoPeriodo.Key.Nombre,
                    Anio = grupoPeriodo.Key.FechaInicio.Year,
                    Materias = grupoPeriodo.Select(m =>
                    {
                        // Cálculo de Asistencia
                        int totalClases = m.Asistencia.Count; // ✅ Singular
                        int presentes = m.Asistencia.Count(a => a.EstaPresente); // ✅ Singular
                        int porcentaje = totalClases > 0 ? (int)((double)presentes / totalClases * 100) : 0;

                        // Formateo de Notas Parciales
                        var listaNotas = m.Nota // ✅ Singular
                            .OrderBy(n => n.IdEvaluacionNavigation.Fecha)
                            .Select(n => $"{n.IdEvaluacionNavigation.Nombre}: {n.Valor:0.#}")
                            .ToList();

                        // Determinación de Estado Visual
                        string estadoFinal = m.Estado;
                        if (!string.IsNullOrEmpty(m.CondicionFinal)) estadoFinal = m.CondicionFinal;
                        if (m.EsLibre) estadoFinal = "Libre";

                        return new DetalleCursadaDTO
                        {
                            Materia = m.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                            Estado = estadoFinal,
                            Condicion = m.Estado == "Cursando" ? "En Curso" : estadoFinal,
                            NotaFinal = m.NotaFinalCursada,
                            PorcentajeAsistencia = porcentaje,
                            NotasParciales = listaNotas
                        };
                    }).ToList()
                })
                .ToList();

            return historial;
        }
    }
}