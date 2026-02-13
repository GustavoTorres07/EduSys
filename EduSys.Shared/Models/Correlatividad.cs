namespace EduSys.Shared.Models;

public partial class Correlatividad
{
    public int Id { get; set; }

    public int IdPlanMateriaOrigen { get; set; }

    public int IdPlanMateriaRequisito { get; set; }

    public string TipoRequisito { get; set; } = null!;

    public virtual PlanMateria IdPlanMateriaOrigenNavigation { get; set; } = null!;

    public virtual PlanMateria IdPlanMateriaRequisitoNavigation { get; set; } = null!;
}
