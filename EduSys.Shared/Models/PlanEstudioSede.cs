namespace EduSys.Shared.Models
{
    public  partial class PlanEstudioSede
    {
        public int IdPlan { get; set; }
        public int IdSede { get; set; }
        public bool Activo { get; set; }
        public virtual PlanEstudio IdPlanNavigation { get; set; } = null!;
        public virtual Sede IdSedeNavigation { get; set; } = null!;
    }
}
