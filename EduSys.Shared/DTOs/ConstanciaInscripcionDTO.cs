using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class ConstanciaInscripcionDTO
    {
        // Datos Institucionales / Cabecera
        public string InstitucionNombre { get; set; } = "EduSys Instituto Superior";
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public string PeriodoAcademico { get; set; } = string.Empty;

        // Datos del Alumno
        public string AlumnoNombre { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty; // ✅ Importante

        // Lista de Materias
        public List<DetalleMateriaConstanciaDTO> Materias { get; set; } = new();
    }

    public class DetalleMateriaConstanciaDTO
    {
        public string CodigoMateria { get; set; } = string.Empty; // ✅ AGREGADO: Código de la materia (ej: "MAT101", "FIS102")

        public string Materia { get; set; } = string.Empty;
        public string Comision { get; set; } = string.Empty; // "1° A"
        public string Horarios { get; set; } = string.Empty; // "Lun 18:00 - 20:00"
        public int AnioCursada { get; set; }

        public DateTime FechaInscripcion { get; set; }
    }
}
