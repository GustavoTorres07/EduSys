namespace EduSys.Shared.Models;

public partial class VentanaOperativa
{
    public int Id { get; set; }
    public int IdPeriodo { get; set; }
    public string TipoAccion { get; set; } = null!;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int? IdCarrera { get; set; }
    public int? IdSede { get; set; }
    public virtual PeriodoAcademico IdPeriodoNavigation { get; set; } = null!;
    public virtual Carrera? IdCarreraNavigation { get; set; }
    public virtual Sede? IdSedeNavigation { get; set; }
}
