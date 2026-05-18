using System;

namespace EduSys.Shared.DTOs
{
    public class EventoDTO
    {
        public string Tipo { get; set; } = string.Empty; // Valores esperados: "Success", "Warning", "Info" o "Error"
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}