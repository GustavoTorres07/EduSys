using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class DocenteComisionRequestDTO
    {
        [Required]
        public int IdComision { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un docente")]
        public int IdDocente { get; set; }

        [Required(ErrorMessage = "Debe asignar un rol (Titular, Adjunto, etc)")]
        public int IdRolDocente { get; set; } // 1: Titular, 2: Adjunto, 3: JTP, 4: Ayudante
    }
}

