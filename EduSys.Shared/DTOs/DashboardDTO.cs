namespace EduSys.Shared.DTOs
{
    public class DashboardDTO
    {
        public int CantidadAlumnos { get; set; }
        public int CantidadDocentes { get; set; }
        public int CantidadCarreras { get; set; }
        public int CantidadSedes { get; set; }
        public List<EventoRecienteDTO> UltimosEventos { get; set; } = new List<EventoRecienteDTO>();

    }

    public class EventoRecienteDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = "Info";
    }
}