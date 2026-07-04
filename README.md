# Prueba Técnica — Programador Web (ASP.NET)

Implementación del flujo de **inicio de sesión con validación de usuario, control de errores y gestión de sesión** basado en el diseño de Figma entregado (estética CEPLAN / gob.pe).

## Stack

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET Core 8 MVC (C#) |
| Front-end | Bootstrap 5 + CSS propio |
| Base de datos | SQL Server 2022 (contenedor Docker) |
| ORM | Entity Framework Core 8 |
| Contraseñas | BCrypt (hash + salt) |

## Funcionalidad implementada

- **Login** con selector de tipo de documento (DNI / CE), campos con iconos, mostrar/ocultar contraseña y botón deshabilitado hasta completar los campos.
- **Validación de credenciales** contra SQL Server con mensajes inline según el flujo del diseño: *"Usuario incorrecto"* y *"Contraseña incorrecta"*.
- **CVF (Contador de Validaciones Fallidas)**: al 5.º intento fallido la cuenta se **bloquea 15 minutos** y se muestra la pantalla *"Cuenta bloqueada temporalmente"*. El envío del correo de notificación (N2 del diseño) se simula en el log de la aplicación.
- **Perfil de usuario** tras autenticarse (pestañas, información básica, insignia "Activo"), protegido con `[Authorize]`.
- **Expiración de sesión por inactividad (20 min)**: modal *"Su sesión está a punto de expirar"* con cuenta regresiva y botón **Extender sesión** (renueva la cookie vía fetch). Si el tiempo se agota, se cierra la sesión y el login muestra *"Su sesión ha expirado debido a inactividad"*.
- **Pantalla de activación de cuenta** (`/Cuenta/Activacion`).

## Cómo ejecutarlo

Requisitos: .NET 8 SDK y Docker.

```bash
# 1. Levantar SQL Server
docker compose up -d

# 2. Ejecutar la aplicación (crea la BD y siembra usuarios automáticamente)
dotnet run
```

Abrir la URL que indique la consola (por defecto la del perfil `http` de `Properties/launchSettings.json`).

> La estructura de la tabla está documentada en [`Database/script.sql`](Database/script.sql), pero no hace falta ejecutarla: al arrancar, la app crea la base de datos y siembra los usuarios de prueba.

## Usuarios de prueba

| Tipo | Usuario | Contraseña | Perfil |
|---|---|---|---|
| DNI | `07079879` | `Ceplan#2026` | July Camila Mendoza Quispe — Administrador de Recursos |
| CE | `001234567` | `Ceplan#2026` | Adriana Osorio Montes — Operador |

## Configuración (`appsettings.json`)

```json
"Seguridad": {
  "MaxIntentosFallidos": 5,
  "MinutosBloqueo": 15,
  "MinutosSesion": 20,
  "SegundosAvisoExpiracion": 60
}
```

En `appsettings.Development.json` la sesión está **reducida a 2 minutos** para poder demostrar el flujo de expiración sin esperar 20 minutos. En producción rigen los valores del diseño.

## Decisiones técnicas

- **Cookie authentication** con expiración deslizante en lugar de sesión en memoria: es el mecanismo idiomático de ASP.NET Core y sobrevive a reinicios del servidor.
- **BCrypt** para las contraseñas: nunca se guardan en texto plano.
- **El CVF y el bloqueo viven en la tabla `Usuarios`**, no en sesión: el contador no se puede evadir reiniciando el navegador.
- **Anti-CSRF** en todos los formularios (`ValidateAntiForgeryToken`), incluido el endpoint `ExtenderSesion`.
- **Errores inline por campo** (no un mensaje genérico) para seguir fielmente el flujo del Figma.
- Los recursos gráficos (fondo andino SVG, iconos) son propios/libres, ya que la prueba no entrega assets.
