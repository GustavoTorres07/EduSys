using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class ProcesarSolicitudDTO
    {
        public int SolicitudId { get; set; }
        public bool EsAprobado { get; set; }
        public string? MotivoRechazo { get; set; } // Solo si rechaza
    }
}
