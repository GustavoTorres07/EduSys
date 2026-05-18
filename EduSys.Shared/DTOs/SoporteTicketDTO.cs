using System;

namespace EduSys.Shared.DTOs
{
    public class SoporteTicketDTO
    {
        public int Id { get; set; }
        public string NumeroTicket { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public string NombreSolicitante { get; set; } = string.Empty; // Se obtiene haciendo JOIN con Usuario
        public string EmailSolicitante { get; set; } = string.Empty; // Se obtiene haciendo JOIN con Usuario
        public string Categoria { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaCierre { get; set; }

        // Campo calculado útil para la UI
        public bool EstaCerrado => Estado.Equals("Cerrado", StringComparison.OrdinalIgnoreCase);
    }
}