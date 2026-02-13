namespace EduSys.Shared.Models;

public partial class PlanMateria
{
    public int Id { get; set; }

    public int IdPlan { get; set; }

    public int IdMateria { get; set; }

    public int AnioCursada { get; set; }

    public int Cuatrimestre { get; set; }

    public int CargaHorariaTotal { get; set; }

    public bool? EsPromocionable { get; set; }

    public decimal? NotaMinimaAprobacion { get; set; }

    public string? Objetivos { get; set; }

    public string? ContenidosMinimos { get; set; }

    public bool? TieneProyecto { get; set; }

    public string? DescripcionProyecto { get; set; }

    public string? CondicionesCursada { get; set; }

    public string? CondicionesAprobacion { get; set; }

    public int? IdRegimen { get; set; }

    public int? TipoCalificacion { get; set; }

    public decimal? NotaMinimaRegularizar { get; set; }

    public decimal? NotaMinimaPromocion { get; set; }

    public int? PorcentajeAsistenciaRegularizar { get; set; }

    public int? PorcentajeAsistenciaPromocion { get; set; }

    public int? CantidadParciales { get; set; }

    public int? VigenciaCursadaAnios { get; set; }

    public bool EsLibre { get; set; } = false;

    public bool? TieneFinalObligatorio { get; set; }

    // --- IMPORTANTE: Esta es la colección que faltaba y causaba el error 500 ---
    public virtual ICollection<Comision> Comisions { get; set; } = new List<Comision>();
    // -------------------------------------------------------------------------

    public virtual ICollection<Correlatividad> CorrelatividadIdPlanMateriaOrigenNavigations { get; set; } = new List<Correlatividad>();

    public virtual ICollection<Correlatividad> CorrelatividadIdPlanMateriaRequisitoNavigations { get; set; } = new List<Correlatividad>();

    public virtual Materia IdMateriaNavigation { get; set; } = null!;

    public virtual PlanEstudio IdPlanNavigation { get; set; } = null!;

    public virtual Regimen? IdRegimenNavigation { get; set; }

    public virtual ICollection<MesaFinal> MesaFinals { get; set; } = new List<MesaFinal>();
}