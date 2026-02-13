namespace EduSys.Shared.Models;

public partial class Permiso
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string Modulo { get; set; } = null!;

    public virtual ICollection<Rol> IdRols { get; set; } = new List<Rol>();
}
