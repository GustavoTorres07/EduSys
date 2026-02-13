using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class ExamenAlumnoDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal? Nota { get; set; }
        public string EstadoActa { get; set; } = "Abierta"; // Para saber si es nota oficial
        public bool EsOficial => EstadoActa == "Cerrada";
    }
}
