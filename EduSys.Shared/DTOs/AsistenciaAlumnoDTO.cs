using System;
using System.Collections.Generic;
using System.Linq;

namespace EduSys.Shared.DTOs
{
    public class AsistenciaMateriaDTO
    {
        public string Materia { get; set; } = string.Empty;
        public string Comision { get; set; } = string.Empty;
        public int CicloLectivo { get; set; }
        public decimal PorcentajeRequerido { get; set; }

        public List<AsistenciaRegistroDTO> Registros { get; set; } = new();

        // Propiedades calculadas dinámicamente (no se guardan en la BD, se calculan al vuelo)
        public int TotalClases => Registros.Count;
        public int Presentes => Registros.Count(r => r.Estado == "Presente");
        public int Ausentes => Registros.Count(r => r.Estado == "Ausente");
        public int Justificados => Registros.Count(r => r.Estado == "Justificado");

        public decimal PorcentajeActual => TotalClases == 0
            ? 100
            : Math.Round((decimal)(Presentes + Justificados) / TotalClases * 100, 1);

        public bool EnRiesgo => PorcentajeActual < PorcentajeRequerido;
    }

    public class AsistenciaRegistroDTO
    {
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty; // "Presente", "Ausente", "Justificado"
        public string? Observacion { get; set; }
    }
}