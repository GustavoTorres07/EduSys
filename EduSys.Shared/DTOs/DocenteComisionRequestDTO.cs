using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class DocenteComisionRequestDTO
    {
        [Required]
        public int IdComision { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un docente")]
        public int IdDocente { get; set; }

        [Required(ErrorMessage = "Debe asignar un rol (Titular, Adjunto, etc)")]
        public int IdRolDocente { get; set; } 
    }
}

