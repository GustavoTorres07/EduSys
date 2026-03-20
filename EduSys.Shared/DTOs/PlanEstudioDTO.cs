using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class PlanEstudioDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty; 

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una carrera")]
        public int IdCarrera { get; set; }
        public string? NombreCarrera { get; set; } 

        [Required]
        public int AnioInicio { get; set; } 

        public string? ResolucionMinisterial { get; set; }
        public bool EsVigente { get; set; } = true;
        public int CantidadMaterias { get; set; }
    }
}
