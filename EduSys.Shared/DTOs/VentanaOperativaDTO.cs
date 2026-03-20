using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class VentanaOperativaDTO
    {
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un período académico.")]
        public int IdPeriodo { get; set; }

        public string NombrePeriodo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de acción es obligatorio.")]
        [MaxLength(50)]
        public string TipoAccion { get; set; } = "INSCRIPCION_CURSADA";

        [Required(ErrorMessage = "Debe especificar la fecha de inicio.")]
        public DateTime? FechaInicio { get; set; }

        [Required(ErrorMessage = "Debe especificar la fecha de fin.")]
        public DateTime? FechaFin { get; set; }

        public int? IdCarrera { get; set; }
        public string NombreCarrera { get; set; } = "Todas"; 

        public int? IdSede { get; set; }
        public string NombreSede { get; set; } = "Todas"; 
        public bool EstaVigente => FechaInicio.HasValue && FechaFin.HasValue &&
                                   DateTime.Now >= FechaInicio.Value && DateTime.Now <= FechaFin.Value;
    }
}