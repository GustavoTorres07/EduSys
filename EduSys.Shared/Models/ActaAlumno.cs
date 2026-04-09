using System;

namespace EduSys.Shared.Models;

public partial class ActaAlumno
{
    public int Id { get; set; }
    public string NumeroActa { get; set; } = null!;
    public int IdAlumno { get; set; }
    public int IdPlanMateria { get; set; }
    public string TipoActa { get; set; } = null!;
    public string Detalle { get; set; } = null!;
    public DateTime FechaEmision { get; set; }
    public decimal? Nota { get; set; }
    public string EstadoAcademico { get; set; } = null!;
    public int? IdEvaluacionReferencia { get; set; }
    public int? IdInscripcionCursadaReferencia { get; set; }
    public int? IdInscripcionFinalReferencia { get; set; }
    public int? IdDocenteFirma { get; set; }

    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;
    public virtual PlanMateria IdPlanMateriaNavigation { get; set; } = null!;
    public virtual Evaluacion? IdEvaluacionReferenciaNavigation { get; set; }
    public virtual InscripcionCursada? IdInscripcionCursadaReferenciaNavigation { get; set; }
    public virtual InscripcionFinal? IdInscripcionFinalReferenciaNavigation { get; set; }
    public virtual Docente? IdDocenteFirmaNavigation { get; set; }
}