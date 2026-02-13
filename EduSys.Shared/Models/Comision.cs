namespace EduSys.Shared.Models;

public partial class Comision
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public int IdPlanMateria { get; set; }
    public int IdPeriodo { get; set; }
    public int IdSede { get; set; }
    public int CupoMaximo { get; set; }
    public string? Turno { get; set; }
    public string? Estado { get; set; }

    // Navegación
    public virtual ICollection<DocenteComision> DocenteComisions { get; set; } = new List<DocenteComision>();
    public virtual ICollection<Evaluacion> Evaluacions { get; set; } = new List<Evaluacion>();
    public virtual ICollection<HorarioComision> HorarioComisions { get; set; } = new List<HorarioComision>();
    public virtual ICollection<InscripcionCursada> InscripcionCursada { get; set; } = new List<InscripcionCursada>();

    public virtual PeriodoAcademico IdPeriodoNavigation { get; set; } = null!;
    public virtual PlanMateria IdPlanMateriaNavigation { get; set; } = null!;
    public virtual Sede IdSedeNavigation { get; set; } = null!;
}