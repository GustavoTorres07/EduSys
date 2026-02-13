using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class DocenteComisionListadoDTO
    {
        public int Id { get; set; } // Id de la relación (DocenteComision)
        public int IdDocente { get; set; }
        public string NombreDocente { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty; // "Titular", "Adjunto"


    }
}
