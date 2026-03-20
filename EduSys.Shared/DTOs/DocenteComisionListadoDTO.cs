namespace EduSys.Shared.DTOs
{
    public class DocenteComisionListadoDTO
    {
        public int Id { get; set; } 
        public int IdDocente { get; set; }
        public string NombreDocente { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty; 


    }
}
