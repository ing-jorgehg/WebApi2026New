using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi2026New.Data;
using WebApi2026New.DTOs;
using WebApi2026New.Helpers;
using WebApi2026New.Models;

namespace WebApi2026New.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(AppDBContext context, JwtHelper jwt) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("registro")]
    public async Task<ActionResult<PersonaReadDTO>> Registro(RegistroDTO dto)
    {
        var correo = dto.correo.Trim().ToLowerInvariant();
        if (await context.Personas.AnyAsync(p => p.correo == correo))
            return Conflict(new { mensaje = "El correo ya está registrado." });

        var persona = new Persona
        {
            nombre = dto.nombre,
            apellido = dto.apellido,
            edad = dto.edad,
            correo = correo,
            password = BCrypt.Net.BCrypt.HashPassword(dto.password)
        };
        context.Personas.Add(persona);
        await context.SaveChangesAsync();

        return Ok(new PersonaReadDTO
        {
            idPersona = persona.idPersona,
            nombre = persona.nombre,
            apellido = persona.apellido,
            edad = persona.edad,
            correo = persona.correo
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginRespuestaDTO>> Login(LoginDTO dto)
    {
        var correo = dto.correo.Trim().ToLowerInvariant();
        var persona = await context.Personas.FirstOrDefaultAsync(p => p.correo == correo);
        if (persona is null || !VerificarPassword(dto.password, persona.password))
            return Unauthorized(new { mensaje = "Credenciales inválidas." });

        var (token, expira) = jwt.GenerarToken(persona);
        return Ok(new LoginRespuestaDTO
        {
            token = token,
            expira = expira,
            idPersona = persona.idPersona,
            nombre = $"{persona.nombre} {persona.apellido}"
        });
    }

    [Authorize]
    [HttpGet("yo")]
    public IActionResult Yo() => Ok(new
    {
        idPersona = User.FindFirst("sub")?.Value,
        correo = User.FindFirst("email")?.Value,
        nombre = User.Identity?.Name
    });

    private static bool VerificarPassword(string password, string hashGuardado)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashGuardado);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Las contraseñas antiguas en texto plano no son credenciales válidas.
            return false;
        }
    }
}
