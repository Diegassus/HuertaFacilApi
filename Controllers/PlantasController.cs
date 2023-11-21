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

  [HttpGet("ListadoPrincipal")] // ponerle segridad de token
 // Obtener el listado de todas las plantas. si es posible crear infinite scroll
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
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
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
    { // crear un modelo de planta detalle vista para poder presentarlo apropiadamente (que haga JOINS)
      var planta = await _context.Plantas.Include(p => p.Tipo)
                    .Include(p => p.Iluminacion)
                    .FirstOrDefaultAsync(p => p.Id == PlantaId);
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
  [Route("tips")]
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

  [HttpGet("Rotaciones")] 
  public async Task<IActionResult> Rotaciones (int PlantaId)
  {
    try
    {
      List<PlantaListado> plantas = _context.Rotaciones
    .Where(rotacion => rotacion.Anterior == PlantaId)
    .Select(rotacion => new PlantaListado(
        rotacion.Planta.Id,
        rotacion.Planta.Nombre ,
         rotacion.Planta.Logo
    ))
    .ToList();
      return Ok(plantas);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        return BadRequest("Error al obtener las plantas");
    }
  }

  [HttpGet("Contrarias")] 
  public async Task<IActionResult> Contrarias (int PlantaId)
  {
    try
    {
      List<PlantaListado> plantas = _context.Contrarias.Include(p => p.Planta)
    .Where(c => c.PlantaId == PlantaId)
    .Select(rotacion => new PlantaListado(
        rotacion.Planta.Id,
        rotacion.Planta.Nombre ,
         rotacion.Planta.Logo
    ))
    .ToList();
      return Ok(plantas);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        return BadRequest("Error al obtener las plantas");
    }
  }

  [HttpGet("Bonificadores")] 
  public async Task<IActionResult> Bonificadores (int PlantaId)
  {
    try
    {
      List<Biopreparados> bio = _context.Bonificadores.Include(b => b.Biopreparado)
    .Where(b => b.PlantaId == PlantaId)
    .Select(b => new Biopreparados{
      Id = b.BiopreparadoId,
      Foto = b.Biopreparado.Foto,
      Descripcion = b.Biopreparado.Descripcion,
      Nombre = b.Biopreparado.Nombre,
      Ingredientes = b.Biopreparado.Ingredientes
    })
    .ToList();
      return Ok(bio);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        return BadRequest("Error al obtener los biopreparados");
    }
  }

  [HttpGet("Biopreparados")] 
  public async Task<IActionResult> Biopreparados (int AmenazaId)
  {
    try
    {
      List<Biopreparados> bio = _context.Curas.Include(b => b.Biopreparado)
    .Where(b => b.AmenazaId == AmenazaId)
    .Select(b => new Biopreparados{
      Id = b.BiopreparadoId,
      Foto = b.Biopreparado.Foto,
      Descripcion = b.Biopreparado.Descripcion,
      Nombre = b.Biopreparado.Nombre,
      Ingredientes = b.Biopreparado.Ingredientes
    })
    .ToList();
      return Ok(bio);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        return BadRequest("Error al obtener los biopreparados");
    }
  }

  [HttpGet("Amenazas")] 
  public async Task<IActionResult> Amenazas (int PlantaId)
  {
    try
    {
      List<Amenazas> plagas = _context.Ataques.Include(b => b.Amenaza)
    .Where(b => b.PlantaId == PlantaId)
    .Select(b => new Amenazas{
      Id = b.AmenazaId,
      Foto = b.Amenaza.Foto,
      Descripcion = b.Amenaza.Descripcion,
      Nombre = b.Amenaza.Nombre,
    })
    .ToList();
      return Ok(plagas);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        return BadRequest("Error al obtener los biopreparados");
    }
  }

  [HttpGet("Usos")] 
  public async Task<IActionResult> Usos (int PlantaId)
  {
    try
    {
      List<Usos> usos = _context.Usos.Where(u => u.PlantaId == PlantaId).ToList();
      return Ok(usos);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        return BadRequest("Error al obtener las plantas");
    }
  }
}