# WebApi2026New — Taller JWT

API de Personas y Direcciones con registro, login, contraseñas BCrypt y autenticación JWT.

## Ejecutar

Requiere .NET 10 y SQL Server LocalDB. Desde esta carpeta:

```powershell
dotnet restore WebApi2026New/WebApi2026New.csproj
dotnet ef database update --project WebApi2026New/WebApi2026New.csproj
dotnet run --project WebApi2026New/WebApi2026New.csproj --launch-profile https
```

Abrir https://localhost:7186/scalar/v1. Registrar una persona en `/api/Auth/registro`, iniciar sesión en `/api/Auth/login` y pegar el token sin comillas en **Authentication → Bearer Token**. La cuenta de ejemplo local `ana@upc.edu.co` usa `ClaveSegura2026*`; si ya existe, repetir el registro devuelve 409.

La clave de ejemplo en `appsettings.Development.json` es solo para desarrollo. En producción suministrar una clave nueva mediante `Jwt__Key`. La API valida firma HS256, emisor, audiencia y vencimiento, con un minuto de tolerancia.

## Probar la API

El archivo [WebApi2026New.http](WebApi2026New/WebApi2026New.http) contiene peticiones para comprobar registro, login, acceso con y sin token, credenciales incorrectas y validación de datos desde Visual Studio. Para obtener un registro 200, usar un correo nuevo; repetirlo devuelve 409.

La solución base sigue el enunciado: no incluye roles ni reglas de propiedad. Cualquier usuario autenticado puede operar sobre Personas y Direcciones. El índice único de correo se explica como mejora opcional y no está aplicado.

El PDF, las capturas y el respaldo del taller se conservan por separado y no forman parte del código de la API.
