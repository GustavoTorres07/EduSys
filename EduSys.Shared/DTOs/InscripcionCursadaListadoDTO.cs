using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class InscripcionCursadaListadoDTO
    {
        public int Id { get; set; }
        public string Materia { get; set; }
        public string Comision { get; set; }
        public string Turno { get; set; }
        public string Estado { get; set; }
        public DateTime? Fecha { get; set; }

        public string Sede { get; set; }
    }
}
