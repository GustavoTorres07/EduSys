using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class PermisoDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string Modulo { get; set; } = null!;
        public bool Asignado { get; set; } // Útil para los checks en el Front
    }
}
