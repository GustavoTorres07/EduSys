using System.Collections.Generic;

namespace EduSys.Shared.Models
{
    public partial class EstadoMateria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public bool EsAprobatoria { get; set; }
        public bool HabilitaFinal { get; set; }
        public bool Activo { get; set; }
        public virtual ICollection<InscripcionCursada> InscripcionCursadas { get; set; } = new List<InscripcionCursada>();
    }
}