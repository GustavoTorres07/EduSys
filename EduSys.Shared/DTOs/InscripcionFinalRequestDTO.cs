using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class InscripcionFinalRequestDTO
    {
        public int IdAlumno { get; set; }
        public int IdMesaFinal { get; set; }
        public string Condicion { get; set; } = "Regular"; // Para guardarlo en el historial luego
    }
}
