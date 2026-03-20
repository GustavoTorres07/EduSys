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
        public string? Direccion { get; set; } = string.Empty;
        public int IdCarreraInteres { get; set; } 
        public string NombreCarrera { get; set; } = string.Empty;
        public int IdSede { get; set; } 
        public string NombreSede { get; set; } = string.Empty; 
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; } 
        public DateTime? FechaProcesado { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? RutaFotoPerfil { get; set; }
        public string? RutaFotoDniFrente { get; set; }
        public string? RutaFotoDniDorso { get; set; }
        public string? RutaTituloSecundario { get; set; }
        public string? RutaAntecedentesPenales { get; set; }
        public string? RutaFotoSosteniendoDNI { get; set; }
        public string? ObservacionAdmin { get; set; }
    }
}