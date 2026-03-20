namespace EduSys.Shared.DTOs
{
    public class ComisionDocenteDTO
    {
        public int IdComision { get; set; }
        public string Materia { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string CodigoComision { get; set; } = string.Empty; 
        public string Sede { get; set; } = string.Empty;
        public string Aula { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty; 
        public int CantidadAlumnos { get; set; }
        public string Rol { get; set; } = string.Empty; 
        public string Estado { get; set; } = string.Empty; 
    }
}