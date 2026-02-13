namespace EduSys.Shared.Models;

public partial class Materia
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Codigo { get; set; }
    public bool? Activo { get; set; }

    // --- NUEVO CAMPO ---
    public string? Descripcion { get; set; }
    // -------------------

    public virtual ICollection<PlanMateria> PlanMateria { get; set; } = new List<PlanMateria>();
}