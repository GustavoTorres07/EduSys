namespace EduSys.Shared.DTOs
{
    public class ComisionDocenteDTO
    {
        public int IdComision { get; set; }
        public string Materia { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string CodigoComision { get; set; } = string.Empty; // Ej: "1° A"
        public string Sede { get; set; } = string.Empty;
        public string Aula { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty; // Ej: "Lun 18:00-22:00 / Mie 18:00-20:00"
        public int CantidadAlumnos { get; set; }
        public string Rol { get; set; } = string.Empty; // Titular, Suplente, etc.
        public string Estado { get; set; } = string.Empty; // Abierta, Cerrada
    }
}