using System;
using System.Collections.Generic;

namespace EduSys.Shared.DTOs
{
    public class HistoriaAcademicaDTO
    {
        public string AlumnoNombre { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;

        // Estadísticas
        public decimal PromedioGeneral { get; set; }
        public int MateriasAprobadas { get; set; }
        public int TotalMateriasPlan { get; set; }
        public double PorcentajeAvance { get; set; }

        // Lista de materias del plan
        public List<DetalleMateriaAvanceDTO> Detalle { get; set; } = new();
    }

    public class DetalleMateriaAvanceDTO
    {
        public int IdPlanMateria { get; set; }
        public int AnioCursada { get; set; }
        public string Materia { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Estado { get; set; } = "Pendiente";
        public decimal? Nota { get; set; }
        public DateTime? Fecha { get; set; }
    }
}