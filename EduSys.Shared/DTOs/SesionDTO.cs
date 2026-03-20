namespace EduSys.Shared.DTOs
{
    public class SesionDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public bool DebeCambiarPass { get; set; }
        public string FotoPerfilUrl { get; set; } = string.Empty;
        public List<string> Permisos { get; set; } = new List<string>();
    }
}
