namespace EduSys.Shared.Models;

public partial class Carrera
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Titulo { get; set; } = null!;
    public int DuracionAnios { get; set; }
    public bool? Activo { get; set; }

    // ❌ ELIMINAR ESTA LÍNEA (Ya no usamos el string simple)
    // public string? Modalidad { get; set; }

    public string? Descripcion { get; set; }
    public string? ResolucionMinisterial { get; set; }

    // --- RELACIONES ---
    public virtual ICollection<CarreraSede> CarreraSedes { get; set; } = new List<CarreraSede>();

    public virtual ICollection<PlanEstudio> PlanEstudios { get; set; } = new List<PlanEstudio>();

    // ✅ AGREGAR ESTA NUEVA LISTA (Para la relación con Modalidades)
    public virtual ICollection<CarreraModalidad> CarreraModalidads { get; set; } = new List<CarreraModalidad>();
}