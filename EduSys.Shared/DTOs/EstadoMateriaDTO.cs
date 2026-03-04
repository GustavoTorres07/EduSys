namespace EduSys.Shared.DTOs
{
    public class EstadoMateriaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool EsAprobatoria { get; set; }
        public bool HabilitaFinal { get; set; }
        public bool Activo { get; set; }
    }
}