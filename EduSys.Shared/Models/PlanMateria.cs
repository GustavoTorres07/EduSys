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
    public int ModoAprobacionCursada { get; set; } = 0;
    public decimal? NotaEliminatoria { get; set; }
    public int? CantidadAplazosParaLibre { get; set; }
    public decimal? PromedioMinimoAprobacion { get; set; }
    public int? IdEstadoPromocion { get; set; }
    public int? IdEstadoRegular { get; set; }
    public int? IdEstadoSiDesaprueba { get; set; }
    public int? IdEstadoSiFaltaAsistencia { get; set; }
    public virtual ICollection<Comision> Comisions { get; set; } = new List<Comision>();
    public virtual ICollection<Correlatividad> CorrelativasComoOrigen { get; set; } = new List<Correlatividad>();
    public virtual ICollection<Correlatividad> CorrelativasComoRequisito { get; set; } = new List<Correlatividad>();
    public virtual Materia IdMateriaNavigation { get; set; } = null!;
    public virtual PlanEstudio IdPlanNavigation { get; set; } = null!;
    public virtual Regimen? IdRegimenNavigation { get; set; }
    public virtual ICollection<MesaFinal> MesaFinals { get; set; } = new List<MesaFinal>();
    public virtual EstadoMateria? IdEstadoPromocionNavigation { get; set; }
    public virtual EstadoMateria? IdEstadoRegularNavigation { get; set; }
    public virtual EstadoMateria? IdEstadoSiDesapruebaNavigation { get; set; }
    public virtual EstadoMateria? IdEstadoSiFaltaAsistenciaNavigation { get; set; }
    public int ModoNotaRecuperatorio { get; set; } = 0;
    public bool TieneIntegrador { get; set; } = false;
    public int? CondicionIntegradorParciales { get; set; }
    public decimal? NotaAprobacionIntegrador { get; set; }
    public bool IntegradorPermitePromocion { get; set; } = false;
    public decimal? NotaPromocionIntegrador { get; set; }
}