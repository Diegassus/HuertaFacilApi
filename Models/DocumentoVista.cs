namespace HuertaFacilApi.Models;
public class DocumentoVista
{
    public int Id { get; set; }
    public string Titulo { get; set; }

    public DocumentoVista(int id, string titulo)
    {
        this.Id = id;
        this.Titulo = titulo;
    }
}