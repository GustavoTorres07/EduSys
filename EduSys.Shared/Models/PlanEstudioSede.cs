using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.Models
{
    public class PlanEstudioSede
    {
        public int IdPlan { get; set; }
        public int IdSede { get; set; }
        public bool Activo { get; set; }

        public virtual PlanEstudio IdPlanNavigation { get; set; } = null!;
        public virtual Sede IdSedeNavigation { get; set; } = null!;
    }
}
