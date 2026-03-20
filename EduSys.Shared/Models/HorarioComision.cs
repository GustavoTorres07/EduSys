using System.ComponentModel.DataAnnotations.Schema;

namespace EduSys.Shared.Models;

public partial class HorarioComision
{
    public int Id { get; set; }
    public int IdComision { get; set; }
    public string DiaSemana { get; set; } = null!;
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFin { get; set; }
    public int? IdAula { get; set; }

    [ForeignKey("IdAula")]
    public virtual Aula? IdAulaNavigation { get; set; }

    public virtual Comision IdComisionNavigation { get; set; } = null!;
}