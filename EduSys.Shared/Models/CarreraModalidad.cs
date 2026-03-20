namespace EduSys.Shared.Models
{
    public partial class CarreraModalidad
    {
        public int Id { get; set; }
        public int IdCarrera { get; set; }
        public int IdModalidad { get; set; }
        public virtual Carrera IdCarreraNavigation { get; set; } = null!;
        public virtual Modalidad IdModalidadNavigation { get; set; } = null!;
    }
}
