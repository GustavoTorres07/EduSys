using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class InscripcionCursadaListadoDTO
    {
        public int IdInscripcion { get; set; }
        public string Materia { get; set; } = string.Empty;
        public string ComisionCodigo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty; // "Cursando", "Baja", etc.
        public DateTime Fecha { get; set; }
    }
}
