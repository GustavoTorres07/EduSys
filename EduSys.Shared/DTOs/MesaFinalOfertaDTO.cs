using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class MesaFinalOfertaDTO
    {
        public int IdMesaFinal { get; set; }
        public int IdPlanMateria { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public int AnioCursada { get; set; }

        public DateTime FechaHora { get; set; }
        public string Tribunal { get; set; } = string.Empty; // Ej: "Pérez (Pres), Gómez, López"

        // Reglas de negocio
        public bool PuedeInscribirse { get; set; }
        public string MotivoBloqueo { get; set; } = string.Empty; // "Falta correlativa", "Ya aprobada"
        public string Condicion { get; set; } = string.Empty; // "Regular" o "Libre"

        // Para saber si ya hizo clic y está inscripto
        public bool YaInscripto { get; set; }
        public int? IdInscripcionFinal { get; set; }
    }
}
