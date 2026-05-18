using System;

namespace EduSys.Shared.Models
{
    public class SoporteMensaje
    {
        public int Id { get; set; }
        public int IdTicket { get; set; }
        public int IdUsuario { get; set; }
        public string Mensaje { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public bool EsRespuestaSoporte { get; set; }

        // Propiedades de navegación
        public virtual SoporteTicket TicketNavigation { get; set; } = null!;
        public virtual Usuario UsuarioNavigation { get; set; } = null!;
    }
}