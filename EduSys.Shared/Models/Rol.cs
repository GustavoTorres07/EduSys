namespace EduSys.Shared.Models;

public partial class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }

    // ❌ ELIMINADO: public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    // ✅ COLECCIONES MUCHOS A MUCHOS
    public virtual ICollection<Permiso> IdPermisos { get; set; } = new List<Permiso>();
    public virtual ICollection<Usuario> IdUsuarios { get; set; } = new List<Usuario>();
}