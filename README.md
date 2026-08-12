<div align="center">

# ❤️ HeartCheck — Backend API

**API REST para el monitoreo de frecuencia cardíaca, generación de alertas y evaluación de riesgo cardiovascular mediante Machine Learning.**

.NET 10 · ASP.NET Core · MongoDB · ML.NET · JWT · FluentValidation · GitHub Actions

</div>

---

## Tabla de contenidos

- [Descripción](#descripción)
- [Características](#características)
- [Arquitectura](#arquitectura)
- [Stack tecnológico](#stack-tecnológico)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Requisitos previos](#requisitos-previos)
- [Configuración](#configuración)
- [Ejecución](#ejecución)
- [Docker](#docker)
- [Autenticación](#autenticación)
- [Endpoints](#endpoints)
- [Modelo de Machine Learning](#modelo-de-machine-learning)
- [Autorización por roles](#autorización-por-roles)
- [Manejo de errores](#manejo-de-errores)
- [Pruebas](#pruebas)
- [DevSecOps / CI/CD](#devsecops--cicd)
- [Licencia](#licencia)

---

## Descripción

**HeartCheck** es el backend de una plataforma diseñada para que pacientes y profesionales de la salud monitoreen la frecuencia cardíaca (BPM) en tiempo real. La plataforma:

- Registra mediciones de dispositivos de monitoreo (mayormente wearables).
- Detecta automáticamente bradicardia, taquicardia y valores fuera de rango según el contexto (reposo, ejercicio, sueño).
- Genera alertas con severidad calculada y permite que el paciente las confirme o resuelva.
- Evalúa el **riesgo cardiovascular** sobre cada medición mediante un **modelo de ML.NET** entrenado (nivel: bajo / moderado / crítico, con recomendaciones).
- Calcula **estadísticas diarias** agregadas (mínimo, máximo, promedio, normales/anormales).
- Administra dispositivos vinculados, planes de suscripción, contactos de emergencia, síntomas, configuraciones y registros de auditoría.

> ⚠️ **Aviso médico**: HeartCheck es una herramienta de apoyo al monitoreo, no reemplaza la evaluación de un profesional de la salud. Las recomendaciones generadas por el modelo son orientativas.

---

## Características

| Módulo | Descripción |
| --- | --- |
| **Autenticación (JWT)** | Registro y login con contraseñas encriptadas con **BCrypt** y tokens JWT firmados con `HMAC-SHA256`. |
| **Pacientes** | Perfil del paciente vinculado al usuario: datos personales, contacto de emergencia. |
| **Dispositivos** | Vincular/desvincular dispositivos (pair/unpair) por identificador físico. |
| **Mediciones** | Alta y consulta histórica de mediciones de BPM (con filtros por rango de fechas). |
| **Evaluación de riesgo (ML)** | `RiskAssessment` por medición: nivel de riesgo, score y recomendación en español. |
| **Alertas** | Generación automática según umbrales por contexto; confirmar y resolver. |
| **Síntomas** | Detección automática (bradicardia/taquicardia) y registro manual con confianza. |
| **Estadísticas diarias** | Agregación automática diaria por paciente. |
| **Notificaciones** | Notificaciones por usuario, marcado como leído. |
| **Planes / UserPlans** | Catálogo de planes y suscripción activa del usuario (cancela el anterior). |
| **Llamadas de emergencia** | Registro y cambio de estado de llamadas de emergencia. |
| **Eventos** | Eventos asociados a una alerta. |
| **Settings** | Configuración key-value del sistema (solo rol `Admin`). |
| **Audit Logs** | Trazabilidad de acciones (solo rol `Admin`). |

---

## Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                    Client (App / Web)                        │
└──────────────────────────┬──────────────────────────────────┘
                           │  HTTPS + Bearer JWT
┌──────────────────────────▼──────────────────────────────────┐
│                HeartCheck API (ASP.NET Core 10)              │
│                                                              │
│  ExceptionHandlingMiddleware  →  RFC 7807 (ProblemDetails)   │
│  FluentValidation (auto)      →  400 con detalles de modelo  │
│  JWT Authentication           →  [Authorize(Roles = "…")]   │
│                                                              │
│  Controllers (MVC) ──► Services (lógica de dominio)          │
│                            │                                 │
│                            ▼                                 │
│                   Repositories (MongoDB)                     │
│   PredictionService (ML.NET)  ──► HeartCheckML.zip           │
└──────────────────────────┬──────────────────────────────────┘
                           │  MongoDB.Driver
┌──────────────────────────▼──────────────────────────────────┐
│                    MongoDB (local o Atlas)                   │
└─────────────────────────────────────────────────────────────┘
```

**Patrones y principios aplicados:**

- **Layered architecture**: Controllers → Services → Repositories → Data.
- **Dependency Injection** nativa (Scoped/Singleton).
- **Async/Await** en todo el stack de datos.
- **DTOs** como contratos de entrada/salida (nunca se exponen entidades de Mongo).
- **FluentValidation** para validación declarativa de entradas.
- **Opciones fuertemente tipadas** (`IOptions<T>`) para configuración.

---

## Stack tecnológico

| Capa | Tecnología |
| --- | --- |
| Runtime | .NET 10 (net10.0) |
| Framework | ASP.NET Core Web (MVC) |
| Base de datos | MongoDB (MongoDB.Driver 3.x) |
| Autenticación | JWT Bearer + BCrypt.Net-Next |
| Machine Learning | Microsoft.ML 5.0 |
| Validación | FluentValidation.AspNetCore 11.x |
| Documentación API | Microsoft.AspNetCore.OpenApi (Solo Development) |
| Tests | xUnit + Moq + FluentAssertions |
| Escaneo de paquetes | `dotnet list package --vulnerable` |
| CI/CD | GitHub Actions (workflow DevSecOps) |
| Contenedores | Dockerfile multi-stage |

---

## Estructura del repositorio

```
HeartCheck.slnx
├── HeartCheck/                      # Proyecto principal (Web API)
│   ├── Configurations/              # MongoDbSettings, JwtSettings
│   ├── Controllers/                 # 14 controladores de la API
│   ├── Data/                        # MongoDbContext y repositorios
│   ├── DTOs/                        # Contratos de entrada/salida
│   ├── Middleware/                  # ExceptionHandlingMiddleware
│   ├── Models/                      # Entidades de MongoDB
│   ├── Services/                    # Lógica de dominio (incl. ML & JWT)
│   ├── Validators/                  # Validadores FluentValidation
│   ├── appsettings.json
│   ├── Dockerfile
│   └── Program.cs                   # Punto de entrada y composición DI
├── HeartCheck.UnitTest/             # Pruebas unitarias (103)
├── HeartCheck.SecurityTest/         # Pruebas de seguridad (19)
├── HeartCheck.IntegrationTest/      # Pruebas de integración (5)
├── HeartCheckTrainer/               # Proyecto entrenador del modelo ML.NET
├── .github/workflows/devsecops.yml  # Pipeline CI/CD
└── .gitignore
```

---

## Requisitos previos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- [MongoDB](https://www.mongodb.com/) (local con `mongod` o cuenta MongoDB Atlas)
- [Docker](https://www.docker.com/) (opcional, para contenedores)
- [Git](https://git-scm.com/)

---

## Configuración

La configuración vive en `HeartCheck/appsettings.json`:

```jsonc
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "HeartCheck"
  },
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!!",
    "Issuer": "HeartCheck",
    "Audience": "HeartCheck",
    "ExpirationHours": 8
  }
}
```

### Importante (seguridad)

- La `Jwt:SecretKey` del ejemplo **solo** funciona en entorno `Development`.
- En producción, la aplicación **aborta el arranque** si detecta la clave de ejemplo o una menor a 32 caracteres.
- Configura producción mediante **User Secrets** o **variables de entorno** (el proveedor de configuración mapea `__` a `:`):

  ```bash
  # Powershell
  $env:Jwt__SecretKey = "UnaClavePrivadaDeAlMenos32CaracteresSegura!!"
  $env:Jwt__Issuer     = "HeartCheck"
  $env:Jwt__Audience   = "HeartCheck"
  $env:MongoDb__ConnectionString = "mongodb+srv://usuario:clave@cluster.mongodb.net/"
  $env:MongoDb__DatabaseName     = "HeartCheck"
  ```

- Los archivos `appsettings.Production.json` y `appsettings.*.local.json` están **ignorados por Git** para evitar filtrar secretos.

---

## Ejecución

```bash
# 1. Restaurar dependencias
dotnet restore HeartCheck.slnx

# 2. Ejecutar la API (perfil HTTP, puerto 5141)
dotnet run --project HeartCheck/HeartCheck.csproj

# Aplicar también los fallos de arranque (JWT) en entorno de producción:
dotnet run --project HeartCheck/HeartCheck.csproj --environment Production
```

Por defecto (Development) la documentación OpenAPI está disponible en:

```
http://localhost:5141/openapi/v1.json
```

---

## Docker

```bash
# Construir la imagen
docker build -f HeartCheck/Dockerfile -t heartcheck-api .

# Ejecutar el contenedor (mapeando el puerto 80)
docker run -p 8080:80 \
  -e Jwt__SecretKey="UnaClavePrivadaDeAlMenos32CaracteresSegura!!" \
  -e MongoDb__ConnectionString="mongodb://host.docker.internal:27017" \
  heartcheck-api
```

> El `Dockerfile` usa una build multi-etapa (base → build → publish → final) para producir una imagen mínima.

---

## Autenticación

Todos los endpoints de recursos requieren el header `Authorization: Bearer <token>`.

### Registro

```
POST /api/auth/register
```

```json
{
  "email": "ana@correo.com",
  "password": "ContraseñaSegura123",
  "firstName": "Ana",
  "lastName": "Pérez",
  "phone": "+34 600 000 000"
}
```

### Login

```
POST /api/auth/login
```

```json
{
  "email": "ana@correo.com",
  "password": "ContraseñaSegura123"
}
```

**Respuesta:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "ana@correo.com",
  "firstName": "Ana",
  "lastName": "Pérez"
}
```

> El JWT incluye `ClaimTypes.NameIdentifier` (id del usuario), `ClaimTypes.Email` y `ClaimTypes.Role`. Vence según `Jwt:ExpirationHours` (8 h por defecto).

---

## Endpoints

> Todos los endpoints bajo `api/` requieren autenticación JWT salvo `api/auth/register` y `api/auth/login`.

### Auth
| Método | Ruta | Descripción |
| --- | --- | --- |
| POST | `/api/auth/register` | Registro de paciente (asigna rol `Patient`) |
| POST | `/api/auth/login` | Inicio de sesión (devuelve JWT) |

### Paciente
| Método | Ruta | Descripción |
| --- | --- | --- |
| POST | `/api/patients` | Crear perfil de paciente |
| GET | `/api/patients/me` | Obtener perfil propio |
| PUT | `/api/patients/me` | Actualizar perfil propio |

### Dispositivos
| Método | Ruta | Descripción |
| --- | --- | --- |
| POST | `/api/devices` | Vincular un dispositivo |
| GET | `/api/devices` | Listar dispositivos del usuario |
| DELETE | `/api/devices/{id}` | Desvincular un dispositivo |

### Mediciones
| Método | Ruta | Descripción |
| --- | --- | --- |
| POST | `/api/measurements` | Registrar medición (BPM es evaluado por contexto) |
| GET | `/api/measurements?from&to` | Historial con rango de fechas |

```json
// POST /api/measurements
{
  "deviceId": "507f1f77bcf86cd799439011",
  "bpm": 118,
  "quality": "good",
  "context": "rest",
  "notes": "Medición tras caminata",
  "symptoms": ["palpitaciones", "mareo"]
}
```

**Respuesta** (incluye evaluación de riesgo del modelo ML):

```json
{
  "timestamp": "2026-08-11T10:15:00.000Z",
  "patientId": "507f1f77bcf86cd799439011",
  "deviceId": "507f1f77bcf86cd799439011",
  "bpm": 118,
  "quality": "good",
  "context": "rest",
  "isNormal": false,
  "notes": "Medición tras caminata",
  "riskAssessment": {
    "riskLevel": "moderate",
    "score": 0.61,
    "recommendation": "Tu frecuencia cardíaca presenta variaciones fuera de lo habitual. Descansa unos minutos, hidrátate y toma una nueva medición en un rato."
  }
}
```

### Alertas
| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/alerts` | Alertas activas del paciente |
| PUT | `/api/alerts/{id}/acknowledge` | Confirmar alerta |
| PUT | `/api/alerts/{id}/resolve` | Resolver alerta |

### Síntomas
| Método | Ruta | Descripción |
| --- | --- | --- |
| POST | `/api/symptoms` | Registrar síntoma manual |
| GET | `/api/symptoms` | Síntomas del usuario |
| GET | `/api/symptoms/{id}` | Detalle de síntoma |
| GET | `/api/symptoms/measurement/{id}` | Síntomas de una medición |

### Estadísticas
| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/statistics/daily?from&to` | Estadísticas diarias del paciente |

### Notificaciones
| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/notifications` | Notificaciones del usuario |
| PUT | `/api/notifications/{id}/read` | Marcar como leída |

### Llamadas de emergencia
| Método | Ruta | Descripción |
| --- | --- | --- |
| POST | `/api/emergency-calls` | Registrar llamada |
| GET | `/api/emergency-calls` | Llamadas del usuario |
| GET | `/api/emergency-calls/{id}` | Detalle de llamada |
| PUT | `/api/emergency-calls/{id}/status` | Actualizar estado |

### Eventos
| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/events/alert/{alertId}` | Eventos de una alerta |

### Planes
| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/plans` | Planes activos |
| POST | `/api/user-plans` | Asignar plan al usuario (cancela el activo) |
| GET | `/api/user-plans/me` | Plan activo del usuario |

### Administración (rol `Admin`)
| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/settings?category` | Listar settings |
| GET | `/api/settings/{key}` | Obtener un setting |
| GET | `/api/settings/categories` | Categorías válidas |
| POST | `/api/settings` | Crear setting |
| PUT | `/api/settings/{key}` | Actualizar setting |
| GET | `/api/audit-logs` | Registros de auditoría (con filtros) |
| GET | `/api/audit-logs/{id}` | Detalle de registro de auditoría |

---

## Modelo de Machine Learning

Las mediciones son evaluadas por un **modelo supervisado de clasificación multiclase** entrenado con el proyecto `HeartCheckTrainer` (ML.NET 5.0).

- **Entrada (`MLModelInput`)**: `bpm`, `context`, `age`, `hasSymptoms`.
- **Salida (`MLModelOutput`)**: `PredictedLabel` (crítico / moderado / bajo) y vector de `Score`.
- **Modelo empaquetado**: `HeartCheckML.zip` (se copia al output con `CopyToOutputDirectory=PreserveNewest`).
- **Contextos**: `rest`, `sleep`, `exercise` (el valor `active` se normaliza a `exercise`).
- **Fallback**: si el modelo no está presente, se usa una evaluación por umbrales (`BpmThresholds` según el contexto).

> El proyecto incluye pruebas directas de `PredictionService`: modelo disponible, fallback y normalización de textos.

---

## Autorización por roles

| Rol | Alcance |
| --- | --- |
| `Patient` | Asignado automáticamente en el registro; acceso a sus propios recursos. |
| `Admin` | Acceso a `/api/settings` y `/api/audit-logs` (`[Authorize(Roles = "Admin")]`). |

Se debe crear un usuario con rol `Admin` directamente en la colección de Mongo para otorgar acceso de administración.

---

## Manejo de errores

`ExceptionHandlingMiddleware` captura excepciones no controladas y responde con **ProblemDetails (RFC 7807)**:

```json
{
  "status": 500,
  "title": "Internal Server Error",
  "type": "https://httpstatuses.com/500",
  "detail": "An unexpected error occurred. Please try again later.",
  "instance": "POST /api/measurements"
}
```

- **Mapeo de status**: `UnauthorizedAccessException` → 401 · `KeyNotFoundException` → 404 · `InvalidOperationException` → 400 · resto → 500.
- **Sin fuga de información**: en producción, los errores 500 ocultan el detalle y **nunca** se exponen stack traces. En Development se incluyen `traceId` y `stackTrace` para depuración.
- Cada error no controlado se registra con `ILogger` (método, ruta y excepción completa).

---

## Pruebas

```bash
# Ejecutar toda la suite en modo Release (como CI)
dotnet test HeartCheck.slnx --configuration Release
```

| Proyecto | Cantidad | Qué cubre |
| --- | --- | --- |
| `HeartCheck.UnitTest` | 103 | Servicios de dominio (auth, patients, devices, measurements, alerts, síntomas, estadísticas, planes, notificaciones, emergencias, eventos, settings, audit logs) y `PredictionService` (ML). |
| `HeartCheck.SecurityTest` | 19 | Hashing BCrypt, generación/claims del JWT, middleware de excepciones (RFC 7807, sin stack traces en producción) y validadores FluentValidation (BPM 30-250, DeviceId ObjectId, contextos). |
| `HeartCheck.IntegrationTest` | 5 | Estructurales con `WebApplicationFactory`: 401 sin token, 401 token inválido, 404 ruta desconocida, 400 payload inválido. |

---

## DevSecOps / CI/CD

El pipeline `.github/workflows/devsecops.yml` se ejecuta en `push` y `pull_request` hacia `main` / `develop`:

1. **Build and Test** — restore → build (Release) → `dotnet test` → sube resultados TRX.
2. **Vulnerable Package Scan** — `dotnet list package --vulnerable --include-transitive`.

> Requisito de Git/GitHub: el token o credencial usada para subir debe incluir el scope `workflow`, ya que el repositorio contiene acciones de GitHub.

---

## Licencia

Uso interno / académico. Consulte al mantenedor para detalles de licenciamiento.