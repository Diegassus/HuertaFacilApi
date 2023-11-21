using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HuertaFacilApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace HuertaFacilApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
  private readonly DataContext _context;
  private readonly IConfiguration _configuration;
  public UsuariosController(DataContext context, IConfiguration configuration)
  {
    this._context = context;
    this._configuration = configuration;
  }

  [HttpPost]
  [Route("Registro")] // api/usuarios/registro
  public async Task<IActionResult> Registro(Registro registro)
  {
    // Se verifica que los datos enviados sean correctos
    if (registro == null) return BadRequest();
    if (registro.Correo == null || registro.Clave == null) return BadRequest("Datos incompletos");
    if (!isValidEmail(registro.Correo) || registro.Clave.Length < 6) return BadRequest("Correo o contraseña no valido");

    // Se verifica que no exista un usuario con ese correo
    var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == registro.Correo);
    if (usuario != null) return BadRequest("Ya existe una cuenta vinculada a este correo");

    // Se crea un nuevo usuario y se registra en la base de datos
    usuario = new Usuario
    {
      Correo = registro.Correo,
      Clave = createHash(registro.Clave),
      Hemisferio = registro.Hemisferio,
      Administrador = false,
      Alta = DateTime.Now
    };

    try
    {
      _context.Add(usuario);
      await _context.SaveChangesAsync();
    }
    catch
    {
      return BadRequest("Ocurrio un error al guardar el usuario");
    }

    try
    {
      return Ok(new JwtSecurityTokenHandler().WriteToken(createToken(usuario))); // retorna el token
    }
    catch
    {
      return BadRequest("Ocurrio un error al crear el token");
    }
  }

  [HttpPost]
  [Route("login")] // api/usuarios/login
  public async Task<IActionResult> Login(Login login)
  {
    // Se verifica que los datos enviados sean correctos
    if (login == null) return BadRequest();
    if (login.Correo == null || login.Clave == null) return BadRequest("Datos incompletos");
    if (!isValidEmail(login.Correo) || login.Clave.Length < 6) return BadRequest("Correo o contraseña no valido");

    // recuperar el usuario
    var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == login.Correo);
    if (usuario == null) return BadRequest("Correo o contraseña incorrectos");

    // hashear la contraseña y comparar con el registro
    try
    {
      var hash = createHash(login.Clave);
      if (usuario.Clave != hash) return BadRequest("Correo o contraseña incorrectos");
    }
    catch
    {
      return BadRequest("Ocurrio un problema al hashear la clave");
    }
    return Ok(new JwtSecurityTokenHandler().WriteToken(createToken(usuario)));
  }

  [Authorize]
  [HttpPost]
  [Route("Favorito")]
  public async Task<IActionResult> AgregarFavorito(int PlantaId)
  {
    if (PlantaId == 0) return BadRequest("No se envio un id de planta");

    // obtener al usuario que hizo la solicitud
    Usuario usuario;
    try
    {
      usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == User.Identity.Name);
    }
    catch
    {
      return BadRequest("No se pudo obtener al usuario");
    }

    // Se comprueba que no exista el registro y se crea uno nuevo. Si existe se elimina
    var existe = await _context.Favoritos.FirstOrDefaultAsync(f => f.PlantaId == PlantaId && f.UsuarioId == usuario.Id);
    if (existe != null)
    {
      try
      {
        _context.Favoritos.Remove(existe);
        await _context.SaveChangesAsync();
      }
      catch
      {
        throw;
      }
      return Ok(false);
    }

    var favorito = new Favoritos
    {
      UsuarioId = usuario.Id,
      PlantaId = PlantaId
    };

    try
    {
      _context.Add(favorito);
      await _context.SaveChangesAsync();
      return Ok(true);
    }
    catch
    {
      return BadRequest("No se pudo agregar el favorito");
    }
  }

  [Authorize]
  [HttpGet]
  [Route("Favorito")]
  public async Task<IActionResult> ConsultarFavorito(int PlantaId)
  {
    if (PlantaId == 0) return BadRequest("No se envio un id de planta");

    // obtener al usuario que hizo la solicitud
    Usuario usuario;
    try
    {
      usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == User.Identity.Name);
    }
    catch
    {
      return BadRequest("No se pudo obtener al usuario");
    }

    // Se comprueba que exista el registro y se elimina
    var existe = await _context.Favoritos.FirstOrDefaultAsync(f => f.PlantaId == PlantaId && f.UsuarioId == usuario.Id);
    if (existe == null)
    {
      return Ok(false);
    }
    return Ok(true);
  }

  [Authorize]
  [HttpGet]
  [Route("Documentos")]
  public async Task<IActionResult> ObtenerDocumentos()
  {
    // obtener al usuario que hizo la solicitud
    Usuario usuario;
    try
    {
      usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == User.Identity.Name);
    }
    catch
    {
      return BadRequest("No se pudo obtener al usuario");
    }

    var docs = new List<DocumentoVista>();
    try
    {
      _context.Documentos.Where(d => d.UsuarioId == usuario.Id).ToList().ForEach(doc =>
      {
        string titulo = Path.GetFileNameWithoutExtension(doc.Url);
        docs.Add(new DocumentoVista(doc.Id, titulo));
      });
      return Ok(docs);
    }
    catch
    {
      throw;
    }
  }

  [Authorize]
  [HttpPost]
  [Route("ActualizarDocumentos")]
  public async Task<IActionResult> ActualizarDocumento(IFormFile file, [FromForm] string nombreViejo)
  {
    // obtener al usuario que hizo la solicitud
    Usuario usuario;
    try
    {
      usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == User.Identity.Name);
    }
    catch
    {
      return BadRequest("No se pudo obtener al usuario");
    }

    if (file == null || file.Length == 0) return BadRequest("No se proporciono ningun archivo");

    // guardar el archivo en el servidor
    try
    {
      var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
      var nombreNuevo = file.FileName;

      var filePathViejo = Path.Combine(uploadsFolder, nombreViejo + ".dat-" + usuario.Id);
      if (System.IO.File.Exists(filePathViejo))
      {
        System.IO.File.Delete(filePathViejo);
      }

      var filePath = Path.Combine(uploadsFolder, nombreNuevo + "-" + usuario.Id);
      using (var fileStream = new FileStream(filePath, FileMode.Create))
      {
        await file.CopyToAsync(fileStream);
      }

      // registrarlo en la base de datos
      var dbViejo = _context.Documentos.FirstOrDefault(d => d.Url == filePathViejo);
      if (dbViejo != null)
      {
        _context.Documentos.Remove(dbViejo);
        _context.SaveChanges();
      }

      _context.Documentos.Add(new Documentos
      {
        Url = filePath,
        UsuarioId = usuario.Id
      });

      _context.SaveChanges();

      return Ok("Guardado con exito");
    }
    catch
    {
      throw;
    }

  }

  [Authorize]
  [HttpPost]
  [Route("Documentos")]
  public async Task<IActionResult> GuardarDocumento(IFormFile file)
  {
    // obtener al usuario que hizo la solicitud
    Usuario usuario;
    try
    {
      usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == User.Identity.Name);
    }
    catch
    {
      return BadRequest("No se pudo obtener al usuario");
    }

    if (file == null || file.Length == 0) return BadRequest("No se proporciono ningun archivo");

    // guardar el archivo en el servidor
    try
    {
      var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
      var filePath = Path.Combine(uploadsFolder, file.FileName + "-" + usuario.Id);
      using (var fileStream = new FileStream(filePath, FileMode.Create))
      {
        await file.CopyToAsync(fileStream);
      }

      // registrarlo en la base de datos
      _context.Documentos.Add(new Documentos
      {
        Url = filePath,
        UsuarioId = usuario.Id
      });

      _context.SaveChanges();

      return Ok("Guardado con exito");
    }
    catch
    {
      throw;
    }

  }

  [Authorize]
  [HttpGet]
  [Route("Descargar")]
  public async Task<IActionResult> Descargar(int DocumentoId)
  {
    // obtener al usuario que hizo la solicitud
    Usuario usuario;
    try
    {
      usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == User.Identity.Name);
    }
    catch
    {
      return BadRequest("No se pudo obtener al usuario");
    }

    try
    {
      var documento = _context.Documentos.FirstOrDefault(d => d.Id == DocumentoId);
      if (documento == null) return BadRequest("No se encontro el documento");

      return PhysicalFile(documento.Url, "application/octet-stream", Path.GetFileName(documento.Url));
    }
    catch
    {
      throw;
    }
  }

  [Authorize]
  [HttpPost]
  [Route("EliminarRecordatorio")]
  public async Task<IActionResult> EliminarRecordatorios(int RecordatorioId)
  {
    try
    {
      _context.Recordatorios.Remove(_context.Recordatorios.Find(RecordatorioId));
      _context.SaveChanges();
      return Ok("Guardado con exito");
    }
    catch
    {
      throw;
    }
  }

  [Authorize]
  [HttpPost]
  [Route("ActualizarRecordatorio")]
  public async Task<IActionResult> Recordatorios(EnvioRegistroUpdate envio)
  {
    Usuario usuario;
    try
    {
      usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == User.Identity.Name);
    }
    catch
    {
      return BadRequest("No se pudo obtener al usuario");
    }

    try
    {
      var registro = _context.Recordatorios.Find(envio.Id);

      registro.Evento = DateTime.Parse(envio.evento);
      registro.PlantaId = envio.Planta;
      registro.Recordatorio_tipoId = envio.Recordatorio;

      _context.SaveChanges();
      return Ok("Guardado con exito");
    }
    catch
    {
      throw;
    }
  }
  [Authorize]
  [HttpPost]
  [Route("Recordatorios")]
  public async Task<IActionResult> Recordatorios(EnvioRegistro envio)
  {
    Usuario usuario;
    try
    {
      usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == User.Identity.Name);
    }
    catch
    {
      return BadRequest("No se pudo obtener al usuario");
    }

    try
    {
      _context.Recordatorios.Add(new Recordatorios
      {
        UsuarioId = usuario.Id,
        PlantaId = envio.Planta,
        Recordatorio_tipoId = envio.Tipo,
        Evento = DateTime.Parse(envio.evento)
      });

      _context.SaveChanges();
      return Ok("Guardado con exito");
    }
    catch
    {
      throw;
    }
  }

  [Authorize]
  [HttpGet]
  [Route("Recordatorio")]
  public async Task<IActionResult> Recordatorio(int idRecordatorio)
  {
    Usuario usuario;
    try
    {
      usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == User.Identity.Name);
    }
    catch
    {
      return BadRequest("No se pudo obtener al usuario");
    }
    RecordatorioVista recordatorio;
    try
    {
      var recordatorioDb = _context.Recordatorios.Include(r => r.Planta).Include(r => r.RecordatorioTipo).FirstOrDefault(r => r.UsuarioId == usuario.Id && r.Id == idRecordatorio);

      recordatorio = new RecordatorioVista{
        Id = recordatorioDb.Id,
        Planta = recordatorioDb.Planta.Nombre,
        Recordatorio= recordatorioDb.RecordatorioTipo.Titulo,
        Evento = recordatorioDb.Evento.ToShortDateString()
      };
      return Ok(recordatorio);
    }
    catch
    {
      throw;
    }
  }

  [Authorize]
  [HttpGet]
  [Route("Recordatorios")]
  public async Task<IActionResult> Recordatorios()
  {
    Usuario usuario;
    try
    {
      usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == User.Identity.Name);
    }
    catch
    {
      return BadRequest("No se pudo obtener al usuario");
    }
    List<RecordatorioVista> recordatorios = new List<RecordatorioVista>();
    try
    {
      _context.Recordatorios.Include(r => r.Planta).Include(r => r.RecordatorioTipo).Where(r => r.UsuarioId == usuario.Id && r.Evento >= DateTime.Now).ToList().ForEach(r =>
      {
        recordatorios.Add(new RecordatorioVista
        {
          Id = r.Id,
          Planta = r.Planta.Nombre,
          Recordatorio = r.RecordatorioTipo.Mensaje,
          Evento = r.Evento.ToShortDateString()
        });
      });
      return Ok(recordatorios);
    }
    catch
    {
      throw;
    }
  }

  [Authorize]
  [HttpGet]
  [Route("tiposRecordatorios")]
  public async Task<IActionResult> TiposRecordatorios()
  {
    Usuario usuario;
    try
    {
      usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == User.Identity.Name);
    }
    catch
    {
      return BadRequest("No se pudo obtener al usuario");
    }
    List<Tipo_RecordatorioVista> recordatorios = new List<Tipo_RecordatorioVista>();
    try
    {
      _context.Tipo_Recordatorio.ToList().ForEach(r =>
      {
        recordatorios.Add(new Tipo_RecordatorioVista
        {
          Id = r.Id,
          Titulo = r.Titulo
        });
      });
      return Ok(recordatorios);
    }
    catch
    {
      throw;
    }
  }

  [Authorize]
  [HttpGet]
  [Route("nombresPlantas")]
  public async Task<IActionResult> NombresPlantas()
  {
    Usuario usuario;
    try
    {
      usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == User.Identity.Name);
    }
    catch
    {
      return BadRequest("No se pudo obtener al usuario");
    }
    List<PlantaVista> plantas = new List<PlantaVista>();
    try
    {
      _context.Plantas.ToList().ForEach(r =>
      {
        plantas.Add(new PlantaVista
        {
          Id = r.Id,
          Planta = r.Nombre
        });
      });
      return Ok(plantas);
    }
    catch
    {
      throw;
    }
  }
  // Metodos Auxiliares

  private JwtSecurityToken createToken(Usuario usuario)
  { // Devuelve un token nuevo
    var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(_configuration["tokenAuthentication:SecretKey"]));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var Claims = new List<Claim> { new Claim(ClaimTypes.Name, usuario.Correo) };
    return new JwtSecurityToken(
        issuer: _configuration["tokenAuthentication:Issuer"],
        audience: _configuration["tokenAuthentication:Audience"],
        claims: Claims,
        expires: DateTime.Now.AddMinutes(7200),
        signingCredentials: credentials
    );
  }

  private string createHash(string cadena)
  {
    return Convert.ToBase64String(KeyDerivation.Pbkdf2( // se hashea la contraseña
            password: cadena,
            salt: System.Text.Encoding.ASCII.GetBytes(_configuration["Salt"]),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8
          ));
  }

  private bool isValidEmail(string email)
  {
    string strRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

    Regex regex = new Regex(strRegex, RegexOptions.IgnoreCase);

    if (regex.IsMatch(email)) return true;
    return false;
  }
}