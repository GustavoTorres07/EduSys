using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class HorarioComisionDTO
    {
        public int Id { get; set; }

        [Required]
        public int IdComision { get; set; }

        [Required(ErrorMessage = "El día es obligatorio")]
        public string DiaSemana { get; set; } = null!; 

        [Required(ErrorMessage = "La hora de inicio es obligatoria")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria")]
        public TimeSpan HoraFin { get; set; }

        [Required(ErrorMessage = "Debes asignar un aula")]
        public int IdAula { get; set; }
        public string? AulaNombre { get; set; } 
        public string? SedeNombre { get; set; }
        public bool EsHorarioValido => HoraFin > HoraInicio;
    }
}
