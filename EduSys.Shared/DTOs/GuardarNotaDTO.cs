using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class GuardarNotaDTO
    {
        public int IdInscripcion { get; set; }
        public int IdEvaluacion { get; set; }
        public decimal? Valor { get; set; } 
    }
}
