namespace EduSys.Shared.Models;

public partial class Alumno
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Legajo { get; set; } = null!;
    public int? IdPlanActual { get; set; }
    public string? EstadoAcademico { get; set; }
    public bool? Activo { get; set; }
    public string? Ocupacion { get; set; }
    public string? LugarTrabajo { get; set; }
    public string? HorarioLaboral { get; set; }
    public bool? EstaBloqueado { get; set; }
    public string? MotivoBloqueo { get; set; }
    public bool TituloSecundarioEntregado { get; set; }
    public string? Observaciones { get; set; }
    public DateOnly? FechaIngreso { get; set; }
    public DateOnly? FechaEgreso { get; set; }

    // --- NUEVO: LEGAJO DIGITAL ---
    public string? UrlDniFrente { get; set; }
    public string? UrlDniDorso { get; set; }
    public string? UrlTituloSecundario { get; set; }
    public string? UrlAntecedentesPenales { get; set; }
    public string? UrlValidacionIdentidad { get; set; }
    // -----------------------------
    public int? IdSede { get; set; }
    public virtual Sede? IdSedeNavigation { get; set; }
    public virtual PlanEstudio? IdPlanActualNavigation { get; set; }
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
    public virtual ICollection<InscripcionCursada> InscripcionCursada { get; set; } = new List<InscripcionCursada>();
    public virtual ICollection<InscripcionFinal> InscripcionFinals { get; set; } = new List<InscripcionFinal>();
}