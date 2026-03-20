namespace EduSys.Shared.Models;

public partial class Solicitud
{
    public int Id { get; set; }
    public int IdAlumno { get; set; }
    public int IdTipoSolicitud { get; set; }
    public DateTime? FechaSolicitud { get; set; }
    public string? Estado { get; set; }
    public string? ObservacionAlumno { get; set; }
    public string? RespuestaInstitucion { get; set; }
    public DateTime? FechaCierre { get; set; }
    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;
    public virtual TipoSolicitud IdTipoSolicitudNavigation { get; set; } = null!;
}
