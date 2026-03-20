namespace EduSys.Shared.DTOs
{
    public class InscripcionFinalRequestDTO
    {
        public int IdAlumno { get; set; }
        public int IdMesaFinal { get; set; }
        public string Condicion { get; set; } = "Regular"; 
    }
}
