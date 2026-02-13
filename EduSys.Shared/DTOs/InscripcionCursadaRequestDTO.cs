using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class InscripcionCursadaRequestDTO
    {
        [Required]
        public int IdAlumno { get; set; }

        [Required]
        public int IdComision { get; set; }

        public bool EsLibre { get; set; } = false;
    }
}
