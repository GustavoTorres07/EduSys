namespace EduSys.Shared.DTOs
{
    public class ExamenAlumnoDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal? Nota { get; set; }
        public string EstadoActa { get; set; } = "Abierta";
        public bool EsOficial { get; set; }
    }
}
