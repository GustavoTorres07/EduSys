namespace EduSys.Shared.Models;

public partial class CarreraSede
{
    public int Id { get; set; }
    public int IdCarrera { get; set; }
    public int IdSede { get; set; }
    public bool? Activo { get; set; }
    public virtual Carrera IdCarreraNavigation { get; set; } = null!;
    public virtual Sede IdSedeNavigation { get; set; } = null!;
}
