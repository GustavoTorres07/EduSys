namespace EduSys.Shared.DTOs
{
    public class EvaluacionDTO
    {
        public int IdEvaluacion { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public bool EsRecuperatorio { get; set; }

        // ✅ NUEVO: Permite enlazar un recuperatorio con su parcial original
        public int? IdEvaluacionPadre { get; set; }

        // Lógica de asignación UI
        public bool AsignarATodos { get; set; } = true;
        public List<int> IdsInscripcionesSeleccionadas { get; set; } = new();

        // ✅ NUEVOS CAMPOS (Para la lógica de Actas y Confirmación)
        public string EstadoActa { get; set; } = "Abierta"; // "Abierta", "Cerrada"
        public string? Libro { get; set; }
        public string? Folio { get; set; }
        public DateTime? FechaCierre { get; set; }

        public bool RequiereConfirmacion { get; set; }
        public int HorasAnticipacionConfirmar { get; set; } = 72;
        public int HorasAnticipacionBaja { get; set; } = 48;

        // Propiedad de ayuda para la Vista (Read-Only)
        public bool EstaCerrada => EstadoActa == "Cerrada";
    }
}