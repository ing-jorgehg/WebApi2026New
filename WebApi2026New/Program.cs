using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using WebApi2026New.Data;
using WebApi2026New.Helpers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<JwtHelper>();
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Falta la sección 'Jwt' en appsettings.json.");
if (Encoding.UTF8.GetByteCount(jwt.Key) < 32)
    throw new InvalidOperationException("Jwt:Key debe tener al menos 32 bytes UTF-8.");
if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience) || jwt.ExpiresMinutes <= 0)
    throw new InvalidOperationException("Jwt requiere Issuer, Audience y ExpiresMinutes mayor que cero.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = "name"
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Pega el token de POST /api/Auth/login, sin escribir Bearer."
        };
        return Task.CompletedTask;
    });
    // Scalar pide token solo en las operaciones que realmente tienen [Authorize].
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any())
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                }
            ];
        }
        return Task.CompletedTask;
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "WebApi2026New";
        options.AddPreferredSecuritySchemes("Bearer");
    });
}
app.UseHttpsRedirection();
// Primero identificar al usuario, después comprobar el acceso.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
