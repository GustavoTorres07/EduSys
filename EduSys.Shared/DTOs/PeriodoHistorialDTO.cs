using System;
using System.Collections.Generic;

namespace EduSys.Shared.DTOs
{
    // 1. EL CONTENEDOR DEL PERIODO (Ej: Ciclo 2025)
    public class PeriodoHistorialDTO
    {
        public int IdPeriodo { get; set; }
        public string NombrePeriodo { get; set; } = string.Empty;
        public int Anio { get; set; }

        public List<DetalleCursadaDTO> Materias { get; set; } = new();
    }

    // 2. EL DETALLE DE LA MATERIA (Ej: Álgebra)
    public class DetalleCursadaDTO
    {
        public string Materia { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty; // Regular, Promocionado, Libre
        public string Condicion { get; set; } = string.Empty; // Texto para mostrar

        public decimal? NotaFinal { get; set; }
        public int PorcentajeAsistencia { get; set; }

        // Lista de parciales
        public List<string> NotasParciales { get; set; } = new();

        // Helpers Visuales
        public bool EsPositivo => Estado == "Promocionado" || Estado == "Regular" || Estado == "Aprobada";
        public bool EsCursando => Estado == "Cursando";
    }
}