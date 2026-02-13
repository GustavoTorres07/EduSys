namespace EduSys.Shared.Models;

public partial class MesaFinal
{
    public int Id { get; set; }

    public int IdPlanMateria { get; set; }

    public int IdPeriodo { get; set; }

    public int IdPresidenteMesa { get; set; }

    public int? IdVocal1 { get; set; }

    public int? IdVocal2 { get; set; }

    public DateTime FechaHora { get; set; }

    public string? Estado { get; set; }

    public string? Libro { get; set; }

    public string? Folio { get; set; }

    public virtual PeriodoAcademico IdPeriodoNavigation { get; set; } = null!;

    public virtual PlanMateria IdPlanMateriaNavigation { get; set; } = null!;

    public virtual Docente IdPresidenteMesaNavigation { get; set; } = null!;

    public virtual ICollection<InscripcionFinal> InscripcionFinals { get; set; } = new List<InscripcionFinal>();
}
