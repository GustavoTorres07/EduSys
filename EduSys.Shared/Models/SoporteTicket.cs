using System;
using System.Collections.Generic;

namespace EduSys.Shared.Models
{
    public class SoporteTicket
    {
        public int Id { get; set; }
        public string NumeroTicket { get; set; } = null!;
        public int IdUsuario { get; set; }
        public string Categoria { get; set; } = null!;
        public string Asunto { get; set; } = null!;
        public string Estado { get; set; } = "Abierto"; // Abierto, Pendiente, Cerrado
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaCierre { get; set; }

        // Propiedades de navegación
        public virtual Usuario UsuarioNavigation { get; set; } = null!;
        public virtual ICollection<SoporteMensaje> Mensajes { get; set; } = new List<SoporteMensaje>();
    }
}