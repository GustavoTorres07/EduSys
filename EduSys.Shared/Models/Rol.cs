namespace EduSys.Shared.Models;

public partial class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public virtual ICollection<Permiso> IdPermisos { get; set; } = new List<Permiso>();
}
