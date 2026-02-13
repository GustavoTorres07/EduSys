using System;
using System.Collections.Generic;

namespace EduSys.Shared.Models
{
    public partial class SolicitudIngreso
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Dni { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public int IdCarreraInteres { get; set; }

        // --- RUTAS DE ARCHIVOS ---
        public string? RutaFotoPerfil { get; set; }
        public string? RutaFotoDniFrente { get; set; }
        public string? RutaFotoDniDorso { get; set; }
        public string? RutaTituloSecundario { get; set; }
        public string? RutaAntecedentesPenales { get; set; }

        // 👇 ESTOS DOS SUELEN FALTAR SI ACTUALIZASTE LA BD RECIÉN:
        public string? RutaFotoSosteniendoDNI { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        public string? Estado { get; set; }
        public DateTime? FechaSolicitud { get; set; }
        public DateTime? FechaProcesado { get; set; }
        public string? ObservacionAdmin { get; set; }

        // RELACIÓN DE NAVEGACIÓN (Importante para el Include)
        public virtual Carrera IdCarreraInteresNavigation { get; set; } = null!;

        public int? IdSede { get; set; }
        public virtual Sede? IdSedeNavigation { get; set; }
    }
}