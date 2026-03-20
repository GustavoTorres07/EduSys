namespace EduSys.Shared.Models
{
    public partial class Modalidad
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Codigo { get; set; }
        public bool? Activo { get; set; }
        public virtual ICollection<CarreraModalidad> CarreraModalidads { get; set; } = new List<CarreraModalidad>();
    }
}
