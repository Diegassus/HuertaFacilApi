namespace HuertaFacilApi.Models;

public class Registro
{
    public required string Correo { get; set; }
    public required string Clave { get; set; }
    public bool Hemisferio { get; set; }
}