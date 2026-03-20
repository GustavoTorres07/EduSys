using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class InscripcionCursadaRequestDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El Id de Alumno no es válido.")]
        public int IdAlumno { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El Id de Comisión no es válido.")]
        public int IdComision { get; set; }

        public bool EsLibre { get; set; } = false;
    }
}