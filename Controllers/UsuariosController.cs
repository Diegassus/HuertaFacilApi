using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HuertaFacilApi.Models;

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
    public async Task<IActionResult> Registro(Registro registro){
        try{// primero se verifica que no exista un usuario con ese correo
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == registro.Correo);
            if(usuario != null ) return BadRequest("Ya existe una cuenta vinculada a este correo");

            usuario = new Usuario{ // se crea al ususario para el registro
                Correo = registro.Correo,
                Clave = 
                    Convert.ToBase64String(KeyDerivation.Pbkdf2( // se hashea la contraseña
                        password: registro.Clave,
                        salt: System.Text.Encoding.ASCII.GetBytes( _configuration["Salt"]),
                        prf: KeyDerivationPrf.HMACSHA1,
                        iterationCount: 10000,
                        numBytesRequested: 256 / 8
                    )),
                Hemisferio = registro.Hemisferio,
                Administrador = false,
                Alta = DateTime.Now
            };
            _context.Add(usuario);
            await _context.SaveChangesAsync(); // se crea el registro del usuario

            return Ok(new JwtSecurityTokenHandler().WriteToken(createToken(usuario))); // retorna el token
        }
        catch (System.Exception){
            throw;
        }
    }

    [HttpPost]
    [Route("login")] // api/usuarios/login
    public async Task<IActionResult> Login(Login login){
        try
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == login.Correo);
            if(usuario == null) return BadRequest("Correo o contraseña incorrectos");

            var hash = 
                Convert.ToBase64String(KeyDerivation.Pbkdf2( // se hashea la contraseña
                    password: login.Clave,
                    salt: System.Text.Encoding.ASCII.GetBytes( _configuration["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                ));
            if(usuario.Clave != hash) return BadRequest("Correo o contraseña incorrectos");

            return Ok(new JwtSecurityTokenHandler().WriteToken(createToken(usuario)));
        }
        catch (System.Exception)
        {
            throw;
        }
    }



    private JwtSecurityToken createToken(Usuario usuario){ // Devuelve un token nuevo
        var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(_configuration["tokenAuthentication:SecretKey"]));
        var credentials = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
        var Claims = new List<Claim>{new Claim(ClaimTypes.Name, usuario.Correo)};
        return new JwtSecurityToken(
            issuer: _configuration["tokenAuthentication:Issuer"],
            audience: _configuration["tokenAuthentication:Audience"],
            claims: Claims,
            expires: DateTime.Now.AddMinutes(7200),
            signingCredentials: credentials
        );
    }
}