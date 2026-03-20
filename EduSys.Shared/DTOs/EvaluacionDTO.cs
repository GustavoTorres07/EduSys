using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class EvaluacionDTO
    {
        public int IdEvaluacion { get; set; }

        [Required(ErrorMessage = "El nombre de la evaluación es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }

        public bool EsRecuperatorio { get; set; }
        public int? IdEvaluacionPadre { get; set; }
        public bool AsignarATodos { get; set; } = true;
        public List<int> IdsInscripcionesSeleccionadas { get; set; } = new();

        public string EstadoActa { get; set; } = "Abierta"; 

        [MaxLength(20)]
        public string? Libro { get; set; }

        [MaxLength(20)]
        public string? Folio { get; set; }

        public DateTime? FechaCierre { get; set; }
        public bool RequiereConfirmacion { get; set; }

        [Range(0, 720, ErrorMessage = "Las horas no pueden ser negativas.")]
        public int HorasAnticipacionConfirmar { get; set; } = 72;

        [Range(0, 720, ErrorMessage = "Las horas no pueden ser negativas.")]
        public int HorasAnticipacionBaja { get; set; } = 48;

        public bool EstaCerrada => EstadoActa == "Cerrada";
        public bool EsIntegrador { get; set; }
    }
}