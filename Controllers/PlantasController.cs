using HuertaFacilApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HuertaFacilApi.Controllers;
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PlantasController : ControllerBase
{
  private readonly DataContext _context;
  private readonly IConfiguration _configuration;
  public PlantasController(DataContext context, IConfiguration configuration)
  {
    this._context = context;
    this._configuration = configuration;
  }

  [HttpGet] // ponerle segridad de token
  [Route("ListadoPrincipal")] // Obtener el listado de todas las plantas. si es posible crear infinite scroll
  public async Task<IActionResult> ListadoPrincipal()
  {
    List<PlantaListado> plantas = new List<PlantaListado>();
    try
    {
      _context.Plantas.OrderByDescending(p => p.Id).ToList().ForEach(p =>
      {
        plantas.Add(new PlantaListado(p.Id, p.Nombre, p.Logo));
      });
      return Ok(plantas);
    }
    catch
    {
      return BadRequest("Error al obtener las plantas");
    }
  }

  [HttpGet]
  [Route("ListadoFavoritas")]
  public async Task<IActionResult> listadoFavoritas()
  {
    var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == User.Identity.Name);
    if(usuario == null)
    {
      return BadRequest("Usuario no encontrado");
    }
    var plantas = new List<PlantaListado>();
    try{
      _context.Favoritos.Include(f => f.Planta).Where(f => f.UsuarioId == usuario.Id).Select(f => f.Planta).ToList().ForEach(p => {
        plantas.Add(new PlantaListado(p.Id, p.Nombre, p.Logo));
      });
      return Ok(plantas);
    }
    catch
    {
      return BadRequest("Error al obtener las plantas");
    }
  }

  [HttpGet]
  public async Task<IActionResult> InformacionPlanta(int PlantaId)
  {
    if(PlantaId == 0) return BadRequest("El id de la planta no es valido");
    try
    {
      var planta = await _context.Plantas.FirstOrDefaultAsync(p => p.Id == PlantaId);
      if (planta == null)
      {
        return BadRequest("Planta no encontrada");
      }
      return Ok(planta);
    }
    catch
    {
      return BadRequest("Ocurrio un problema al obtener la informacion de la planta");
    }
  }
  
  [HttpGet]
  [Route("tip")]
  public async Task<IActionResult> ObtenerTips(int PlantaId)
  {
    if(PlantaId == 0) return BadRequest("El id de la planta no es valido");
    try
    {
      var tips = await _context.Tips.Where(p => p.PlantaId == PlantaId).ToListAsync();
      if (tips == null)
      {
        return BadRequest("No se encontraron tips para esta planta");
      }
      return Ok(tips);
    }
    catch
    {
      return BadRequest("Ocurrio un problema al obtener la informacion de los tips de la planta");
    }
  }
}