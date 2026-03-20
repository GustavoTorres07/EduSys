using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class CarreraDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El título es obligatorio")]
        public string Titulo { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "La duración debe ser entre 1 y 10 años")]
        public int DuracionAnios { get; set; }
        public bool Activo { get; set; } = true;
        public List<string> Modalidades { get; set; } = new List<string>();
        public List<string> NombresSedes { get; set; } = new List<string>();
        public string? Descripcion { get; set; } 
        public string? ResolucionMinisterial { get; set; } 
    }
}