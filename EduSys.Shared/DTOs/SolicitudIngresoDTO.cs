using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class SolicitudIngresoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Direccion { get; set; }

        // --- DATOS ACADÉMICOS ---
        public int IdCarreraInteres { get; set; }
        public string NombreCarrera { get; set; } = string.Empty;

        // ✅ NUEVOS CAMPOS PARA SEDE
        public int IdSede { get; set; }
        public string NombreSede { get; set; } = string.Empty; // Para mostrar en la grilla del admin

        // --- ESTADO Y FECHAS ---
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaProcesado { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        // --- RUTAS DE ARCHIVOS ---
        public string? RutaFotoPerfil { get; set; }
        public string? RutaFotoDniFrente { get; set; }
        public string? RutaFotoDniDorso { get; set; }
        public string? RutaTituloSecundario { get; set; }
        public string? RutaAntecedentesPenales { get; set; }
        public string? RutaFotoSosteniendoDNI { get; set; }
        public string? ObservacionAdmin { get; set; }
    }
}