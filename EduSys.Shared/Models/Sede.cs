namespace EduSys.Shared.Models;

public partial class Sede
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? CodigoPostal { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Aula> Aulas { get; set; } = new List<Aula>();

    public virtual ICollection<CarreraSede> CarreraSedes { get; set; } = new List<CarreraSede>();

    public virtual ICollection<Comision> Comisions { get; set; } = new List<Comision>();

    public virtual ICollection<PlanEstudioSede> PlanEstudioSedes { get; set; } = new List<PlanEstudioSede>();
}
