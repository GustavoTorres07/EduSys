namespace EduSys.Shared.Models;

public partial class TipoSolicitud
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public bool? RequierePago { get; set; }

    public int? SlaHoras { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Solicitud> Solicituds { get; set; } = new List<Solicitud>();
}
