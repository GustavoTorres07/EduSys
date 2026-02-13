using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class DocenteListadoDTO
    {
        public int IdDocente { get; set; }
        public string Legajo { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty; // "Apellido, Nombre"
        public string Dni { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TituloAcademico { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
