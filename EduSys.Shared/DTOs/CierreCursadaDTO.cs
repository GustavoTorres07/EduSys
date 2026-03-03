using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class CierreCursadaDTO
    {
        public int IdComision { get; set; }
        public string Libro { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
    }
}
