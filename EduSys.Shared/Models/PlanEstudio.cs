namespace EduSys.Shared.Models;

public partial class PlanEstudio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public int IdCarrera { get; set; }
    public int AnioInicio { get; set; }
    public bool? EsVigente { get; set; }
    public string? ResolucionMinisterial { get; set; }
    public virtual ICollection<Alumno> Alumnos { get; set; } = new List<Alumno>();
    public virtual Carrera IdCarreraNavigation { get; set; } = null!;
    public virtual ICollection<PlanMateria> PlanMateria { get; set; } = new List<PlanMateria>();
    public virtual ICollection<PlanEstudioSede> PlanEstudioSedes { get; set; } = new List<PlanEstudioSede>();
}
