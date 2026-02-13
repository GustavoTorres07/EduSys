using System;
using System.Collections.Generic;

namespace EduSys.Shared.Models
{
    public partial class Notificacion
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string Titulo { get; set; } = null!;
        public string Mensaje { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public bool Leida { get; set; }
        public string? Tipo { get; set; } // Ej: "Examen", "Asistencia", "Sistema"

        // Relación de navegación
        public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
    }
}