namespace EduSys.Shared.DTOs
{
    public class NotificacionMasivaDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Mensaje"; // Mensaje, Alerta, Info
        public string Destinatarios { get; set; } = "Todos"; // Todos, Alumnos, Docentes
    }
}