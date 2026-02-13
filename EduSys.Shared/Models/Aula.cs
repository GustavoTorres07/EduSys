namespace EduSys.Shared.Models;

public partial class Aula
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int Capacidad { get; set; }

    public int IdSede { get; set; }

    public bool? Activo { get; set; }

    // --- ESTA LÍNEA DEBE BORRARSE ---
    // public virtual ICollection<Comision> Comisions { get; set; } = new List<Comision>(); 
    // --------------------------------

    public virtual Sede IdSedeNavigation { get; set; } = null!;
}