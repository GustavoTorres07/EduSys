using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class NotificacionDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public bool Leida { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Examen", "Sistema", etc.
        public string ColorIcono => Tipo switch
        {
            "Examen" => "Warning",
            "Sistema" => "Info",
            "Asistencia" => "Error",
            _ => "Default"
        };
    }
}
