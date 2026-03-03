using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class CorrelativaItemDTO
    {
        public int IdPlanMateriaRequisito { get; set; }
        public string TipoRequisito { get; set; } = string.Empty; // "Regular" o "Obligatoria"
    }
}
