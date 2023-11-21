using System.ComponentModel.DataAnnotations.Schema;

namespace HuertaFacilApi.Models;

public class Recordatorios {
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int PlantaId { get; set; }
    public Planta? Planta { get; set; }
    public int Recordatorio_tipoId { get; set; }
    [ForeignKey("Recordatorio_tipoId")]
    public Tipo_Recordatorio? RecordatorioTipo { get; set; }
    public DateTime Alta { get; set; }
    public DateTime Evento { get; set;}

}