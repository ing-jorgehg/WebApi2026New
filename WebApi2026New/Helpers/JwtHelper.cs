using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebApi2026New.Models;

namespace WebApi2026New.Helpers;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 60;
}

public class JwtHelper(IOptions<JwtSettings> options)
{
    private readonly JwtSettings _settings = options.Value;

    public (string token, DateTime expira) GenerarToken(Persona persona)
    {
        var expira = DateTime.UtcNow.AddMinutes(_settings.ExpiresMinutes);
        // El token identifica a la persona; nunca incluye su contraseña ni su hash.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, persona.idPersona.ToString()),
            new(JwtRegisteredClaimNames.Email, persona.correo),
            new(JwtRegisteredClaimNames.Name, $"{persona.nombre} {persona.apellido}")
        };
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expira,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
                SecurityAlgorithms.HmacSha256)
        };
        return (new JsonWebTokenHandler().CreateToken(descriptor), expira);
    }
}
