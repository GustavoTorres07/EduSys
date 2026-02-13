using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class PeriodoAcademicoDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(50)]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        public DateTime? FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        public DateTime? FechaFin { get; set; }

        public string Estado { get; set; } = "Abierto";

        public bool Activo { get; set; } = true;

        public bool EsFechaValida => FechaInicio.HasValue && FechaFin.HasValue && FechaFin > FechaInicio;
    }
}
