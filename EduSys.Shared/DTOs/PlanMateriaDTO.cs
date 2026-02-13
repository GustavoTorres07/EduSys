namespace EduSys.Shared.DTOs
{
    public class PlanMateriaDTO
    {
        public int Id { get; set; }
        public int IdPlan { get; set; }
        public int IdMateria { get; set; }
        public string? NombreMateria { get; set; }
        public string? CodigoMateria { get; set; }
        public int AnioCursada { get; set; }
        public int? IdRegimen { get; set; }
        public string? NombreRegimen { get; set; }
        public int Cuatrimestre { get; set; }
        public int CargaHorariaTotal { get; set; }
        public bool EsLibre { get; set; } = false;

        // --- NUEVAS REGLAS ESTRUCTURADAS ---
        public int TipoCalificacion { get; set; } = 0; // 0: Numérica, 1: Conceptual

        // Notas Decimales (Usamos decimal? para evitar errores de conversión)
        public decimal? NotaMinimaRegularizar { get; set; } = 4;
        public decimal? NotaMinimaAprobacion { get; set; } = 6;

        public bool EsPromocionable { get; set; } = true;
        public decimal? NotaMinimaPromocion { get; set; }
        public int? PorcentajeAsistenciaPromocion { get; set; }

        public int? PorcentajeAsistenciaRegularizar { get; set; }
        public int VigenciaCursadaAnios { get; set; } = 3;
        public bool TieneFinalObligatorio { get; set; } = false;

        // Textos
        public string? CondicionesCursada { get; set; }
        public string? CondicionesAprobacion { get; set; }
        public string? Objetivos { get; set; }
        public string? ContenidosMinimos { get; set; }
        public bool TieneProyecto { get; set; }
        public string? DescripcionProyecto { get; set; }
        public int CantidadParciales { get; set; } = 2;

        // Correlativas
        public string CorrelativasTexto { get; set; } = "";
        public List<int> IdsCorrelativas { get; set; } = new List<int>();
    }
}