using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class MesaFinalRequestDTO
    {
        public int Id { get; set; }
        public int IdPlanMateria { get; set; }
        public int IdPeriodo { get; set; }
        public int IdPresidenteMesa { get; set; }
        public int? IdVocal1 { get; set; }
        public int? IdVocal2 { get; set; }
        public DateTime FechaHora { get; set; }
        public string? Estado { get; set; } = "Abierta"; // Por defecto al crear
    }
}
