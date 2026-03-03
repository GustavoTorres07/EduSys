using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class MesaFinalDTO
    {
        public int Id { get; set; }
        public int IdPlanMateria { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public string CarreraNombre { get; set; } = string.Empty;
        public int IdPeriodo { get; set; }
        public string PeriodoNombre { get; set; } = string.Empty;

        public int IdPresidenteMesa { get; set; }
        public string PresidenteNombre { get; set; } = string.Empty;

        public int? IdVocal1 { get; set; }
        public string Vocal1Nombre { get; set; } = string.Empty;

        public int? IdVocal2 { get; set; }
        public string Vocal2Nombre { get; set; } = string.Empty;

        public DateTime FechaHora { get; set; }
        public string? Estado { get; set; } // "Abierta", "Cerrada", "Anulada"
        public string? Libro { get; set; }
        public string? Folio { get; set; }

        // Campo auxiliar para saber cuántos se anotaron
        public int CantidadInscriptos { get; set; }
    }
}
