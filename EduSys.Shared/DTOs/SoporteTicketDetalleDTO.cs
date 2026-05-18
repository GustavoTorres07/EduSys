using System.Collections.Generic;

namespace EduSys.Shared.DTOs
{
    public class SoporteTicketDetalleDTO
    {
        public SoporteTicketDTO Ticket { get; set; } = new SoporteTicketDTO();
        public List<SoporteMensajeDTO> HistorialMensajes { get; set; } = new List<SoporteMensajeDTO>();
    }
}