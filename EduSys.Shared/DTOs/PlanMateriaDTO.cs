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
        public int TipoCalificacion { get; set; } = 0;
        public decimal? NotaMinimaRegularizar { get; set; } = 4;
        public decimal? NotaMinimaAprobacion { get; set; } = 6;
        public bool EsPromocionable { get; set; } = true;
        public decimal? NotaMinimaPromocion { get; set; }
        public int? PorcentajeAsistenciaPromocion { get; set; }
        public int? PorcentajeAsistenciaRegularizar { get; set; }
        public int VigenciaCursadaAnios { get; set; } = 3;
        public bool TieneFinalObligatorio { get; set; } = false;
        public bool TieneProyecto { get; set; }
        public string? CondicionesCursada { get; set; }
        public string? CondicionesAprobacion { get; set; }
        public string? Objetivos { get; set; }
        public string? ContenidosMinimos { get; set; }
        public string? DescripcionProyecto { get; set; }
        public int CantidadParciales { get; set; } = 2;
        public int ModoAprobacionCursada { get; set; } = 0;
        public decimal? NotaEliminatoria { get; set; }
        public int? CantidadAplazosParaLibre { get; set; }
        public decimal? PromedioMinimoAprobacion { get; set; }
        public int ModoNotaRecuperatorio { get; set; } = 0;
        public bool TieneIntegrador { get; set; } = false;
        public int? CondicionIntegradorParciales { get; set; }
        public decimal? NotaAprobacionIntegrador { get; set; }
        public bool IntegradorPermitePromocion { get; set; } = false;
        public decimal? NotaPromocionIntegrador { get; set; }
        public int? IdEstadoPromocion { get; set; }
        public int? IdEstadoRegular { get; set; }
        public int? IdEstadoSiDesaprueba { get; set; }
        public int? IdEstadoSiFaltaAsistencia { get; set; }
        public string CorrelativasTexto { get; set; } = "";
        public List<int> IdsCorrelativas { get; set; } = new List<int>();
        public List<CorrelativaItemDTO> CorrelativasDetalle { get; set; } = new List<CorrelativaItemDTO>();
    }
}