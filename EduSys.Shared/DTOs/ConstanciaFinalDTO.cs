using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class ConstanciaFinalDTO
    {
        public string AlumnoNombreCompleto { get; set; } = string.Empty;
        public string AlumnoDNI { get; set; } = string.Empty;
        public string AlumnoLegajo { get; set; } = string.Empty;
        public string CarreraNombre { get; set; } = string.Empty;
        public string MateriaNombre { get; set; } = string.Empty;
        public DateTime FechaExamen { get; set; }
        public string Tribunal { get; set; } = string.Empty;
        public string Condicion { get; set; } = string.Empty;
        public DateTime FechaInscripcion { get; set; }
        public int NumeroTransaccion { get; set; }
    }
}
