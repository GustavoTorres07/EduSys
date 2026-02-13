namespace EduSys.Shared.Models;

public partial class DocenteComision
{
    public int Id { get; set; }

    public int IdComision { get; set; }

    public int IdDocente { get; set; }

    public string RolDocente { get; set; } = null!;
    public bool Activo { get; set; }
    public virtual Comision IdComisionNavigation { get; set; } = null!;

    public virtual Docente IdDocenteNavigation { get; set; } = null!;
}
