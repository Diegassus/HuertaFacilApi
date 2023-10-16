namespace HuertaFacilApi.Models
{
    public class Tips
    {
        public int Id { get; set; }
        public string Imagen { get; set; }
        public string Descripcion { get; set; }
        public int? PlantaId { get; set; }
    }
}