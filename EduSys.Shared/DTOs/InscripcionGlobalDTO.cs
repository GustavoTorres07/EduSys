using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class InscripcionGlobalDTO
    {
        public int IdInscripcion { get; set; }
        public DateTime Fecha { get; set; }

        // Datos del Alumno
        public string AlumnoNombre { get; set; } = string.Empty;
        public string AlumnoLegajo { get; set; } = string.Empty;
        public string AlumnoDni { get; set; } = string.Empty;

        // Datos de la Materia
        public string Carrera { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public string Comision { get; set; } = string.Empty; // Ej: "1° A"
        public string Estado { get; set; } = string.Empty;
    }
}
