namespace EduSys.Shared.DTOs
{
    public class ProcesarSolicitudDTO
    {
        public int SolicitudId { get; set; }
        public bool EsAprobado { get; set; }
        public string? MotivoRechazo { get; set; } 
    }
}
