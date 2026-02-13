using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class PlanEstudioDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty; // Ej: "Plan 2024"

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una carrera")]
        public int IdCarrera { get; set; }
        public string? NombreCarrera { get; set; } // Para mostrar en la tabla

        [Required]
        public int AnioInicio { get; set; } // Cohorte (ej: 2024)

        public string? ResolucionMinisterial { get; set; }
        public bool EsVigente { get; set; } = true;

        // Cantidad de materias (para mostrar en la lista como dato útil)
        public int CantidadMaterias { get; set; }
    }
}
