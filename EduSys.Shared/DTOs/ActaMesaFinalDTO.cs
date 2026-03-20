namespace EduSys.Shared.DTOs
{
    public class ActaMesaFinalDTO
    {
        public int IdMesaFinal { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public string CarreraNombre { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string Tribunal { get; set; } = string.Empty;
        public string EstadoMesa { get; set; } = string.Empty; 
        public string Libro { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public List<AlumnoActaFinalDTO> Alumnos { get; set; } = new();
    }
}
