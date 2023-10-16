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
    if(PlantaId == 0) return BadRequest("No se envio un id de planta");
  
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

    // Se comprueba que no exista el registro y se crea uno nuevo
    var existe = await _context.Favoritos.FirstOrDefaultAsync(f => f.PlantaId == PlantaId && f.UsuarioId == usuario.Id);
    if (existe != null)
    {
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