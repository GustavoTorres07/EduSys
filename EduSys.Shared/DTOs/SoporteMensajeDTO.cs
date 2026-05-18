using System;

namespace EduSys.Shared.DTOs
{
    public class SoporteMensajeDTO
    {
        public int Id { get; set; }
        public int IdTicket { get; set; }
        public string NombreAutor { get; set; } = string.Empty; // Se obtiene haciendo JOIN con Usuario
        public string Mensaje { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public bool EsRespuestaSoporte { get; set; }
    }
}