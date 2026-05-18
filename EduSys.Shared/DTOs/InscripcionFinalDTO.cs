using System;

namespace EduSys.Shared.DTOs
{
    public class InscripcionFinalDTO
    {
        public int IdInscripcion { get; set; }
        public int IdMesaFinal { get; set; }

        // ¡Estos son los datos visuales que necesita el Widget!
        public string MateriaNombre { get; set; } = string.Empty;
        public DateTime FechaExamen { get; set; }
        public string Condicion { get; set; } = string.Empty;

        // Opcionales por si luego quieres mostrarlos en la UI:
        public string Sede { get; set; } = string.Empty;
        public string Aula { get; set; } = string.Empty;
        public string Turno { get; set; } = string.Empty;
    }
}