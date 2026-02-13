using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class VentanaOperativaDTO
    {
        public int Id { get; set; }
        public int IdPeriodo { get; set; }
        public string NombrePeriodo { get; set; } = string.Empty;

        public string TipoAccion { get; set; } = "INSCRIPCION_CURSADA";

        public DateTime? FechaInicio { get; set; } // Nullable para facilitar binding en MudBlazor
        public DateTime? FechaFin { get; set; }

        public int? IdCarrera { get; set; }
        public string NombreCarrera { get; set; } = "Todas"; // "Todas" si es null

        public int? IdSede { get; set; }
        public string NombreSede { get; set; } = "Todas"; // "Todas" si es null

        // Propiedad auxiliar para saber si está activa hoy
        public bool EstaVigente => FechaInicio.HasValue && FechaFin.HasValue &&
                                   DateTime.Now >= FechaInicio.Value && DateTime.Now <= FechaFin.Value;
    }
}
