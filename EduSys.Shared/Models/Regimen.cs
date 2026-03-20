using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduSys.Shared.Models
{
    [Table("Regimen")]
    public class Regimen
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = null!;

        public bool Activo { get; set; } = true;

    }
}