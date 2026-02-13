using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class AulaDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre/número es obligatorio")]
        public string Nombre { get; set; } = null!;

        [Required]
        [Range(1, 1000, ErrorMessage = "La capacidad debe ser válida")]
        public int Capacidad { get; set; }

        public int IdSede { get; set; }
        public bool Activo { get; set; } = true;
    }
}
