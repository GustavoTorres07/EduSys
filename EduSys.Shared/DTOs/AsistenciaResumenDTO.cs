using System;

namespace EduSys.Shared.DTOs
{
    public class AsistenciaResumenDTO
    {
        public int IdComision { get; set; }
        public string Materia { get; set; } = string.Empty;
        public int CicloLectivo { get; set; }

        public int TotalClases { get; set; }
        public int Presentes { get; set; }
        public int Ausentes { get; set; }
        public int Justificados { get; set; }

        public decimal PorcentajeActual { get; set; }
        public decimal PorcentajeRequerido { get; set; }
        public bool EnRiesgo { get; set; }
    }
}