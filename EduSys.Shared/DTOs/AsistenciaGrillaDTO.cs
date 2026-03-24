using System;
using System.Collections.Generic;

namespace EduSys.Shared.DTOs
{
    public class AsistenciaGrillaDTO
    {
        public List<DateTime> Fechas { get; set; } = new();
        public List<AlumnoAsistenciaFilaDTO> Alumnos { get; set; } = new();
    }

    public class AlumnoAsistenciaFilaDTO
    {
        public int IdInscripcionCursada { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public List<AsistenciaDetalleDTO> Asistencias { get; set; } = new();
    }

    public class AsistenciaDetalleDTO
    {
        public int Id { get; set; }
        public int IdInscripcionCursada { get; set; }
        public DateTime Fecha { get; set; }
        public bool EstaPresente { get; set; }
        public bool EsJustificado { get; set; }
        public string? Observacion { get; set; }
        public string? UrlCertificado { get; set; }
        public bool Registrado { get; set; } // Propiedad para la UI (saber si fue modificada)
    }

    public class GuardarAsistenciaRequestDTO
    {
        public int IdComision { get; set; }
        public List<AsistenciaDetalleDTO> Asistencias { get; set; } = new();
    }
}