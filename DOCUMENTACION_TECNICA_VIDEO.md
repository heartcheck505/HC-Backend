# 📊 DOCUMENTACIÓN TÉCNICA — HeartCheck Backend

> Documentación técnica orientada a la presentación: arquitectura del backend, blindaje DevSecOps y telemetría con aprendizaje automático.

---

## 1. Arquitectura de Backend (.NET Core + MongoDB)

El proyecto es una **Web API de ASP.NET Core 10** con **MongoDB** como base de datos. Sigue una arquitectura en capas: `Controllers → Services → Repositories → Data`, con **inyección de dependencias** nativa y expone únicamente **DTOs** como contratos de entrada/salida.

### 1.1 Estructura de `Program.cs`

El punto de entrada compone toda la aplicación: convenciones de MongoDB, configuración fuertemente tipada (`IOptions<T>`), registro de repositorios y servicios, la caducidad de JWT y la construcción del pipeline HTTP.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Convenciones de MongoDB: nombres de campos en camelCase
var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
ConventionRegistry.Register("camelCase", conventionPack, _ => true);

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();

builder.Services.AddScoped<IMeasurementRepository, MeasurementRepository>();
builder.Services.AddScoped<IMeasurementService, MeasurementService>();
builder.Services.AddSingleton<IPredictionService, PredictionService>();
```

### 1.2 Configuración JWT + `MapInboundClaims`

El servicio de autenticación usa **JWT Bearer firmado con HMAC-SHA256**. Se valida en todo momento: emisor, audiencia, tiempo de vida y clave de firma.

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;   // usa los nombres de claim estándar
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});
```

**Puntos clave de seguridad en `Program.cs`:**

- `MapInboundClaims = false`: evita el re-mapeo de claims por defecto de la librería y fuerza el contrato explícito del token (NameIdentifier, Email, Role, platform, session_id).
- **Blindaje de clave en arranque**: si `Jwt:SecretKey` es vacía, menor de 32 caracteres o el *placeholder* de ejemplo, el proceso **genera una clave aleatoria en memoria** y registra una advertencia:

```csharp
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) ||
    isPlaceholder ||
    jwtSettings.SecretKey.Length < 32)
{
    jwtSettings.SecretKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    startupLogger.LogWarning("... Se generó una clave aleatoria en memoria ...");
}
```

- **No hay fugas en logs**: se eliminaron los `Console.WriteLine` de `OnTokenValidated`/`OnAuthenticationFailed`.

### 1.3 Inyección de dependencias (resumen del registro)

| Ciclo de vida | Servicios |
| --- | --- |
| `Singleton` | `MongoDbContext`, `PredictionService`, `IOptions<JwtSettings>` |
| `Scoped` | Repositorios (`Patient`, `Device`, `Measurement`, `Alert`, `Event`, `Plan`, `Notification`, `DailyStatistic`, `EmergencyCall`, `Symptom`, `Setting`, `AuditLog`, `User`, `Role`, `UserRole`, `UserPlan`) |
| `Scoped` | Servicios de dominio (`PatientService`, `MeasurementService`, `AlertService`, `AuthService`, `JwtService`, `PasswordHasher`, etc.) |

---

### 1.4 `SinglePlatformSessionMiddleware` y sesiones por plataforma

El modelo `User` almacena un **diccionario de sesiones activas por plataforma**:

```csharp
[BsonElement("activeSessions")]
public Dictionary<string, string> ActiveSessions { get; set; } = new();
```

Las plataformas se normalizan en `AuthService.NormalizePlatform()`. El diccionario soporta **Web**, **Mobile** y **Watch** (cualquier valor desconocido se trata como `Web`):

```csharp
private static string NormalizePlatform(string? platform)
{
    var normalized = string.IsNullOrWhiteSpace(platform) ? "Web" : platform.Trim();
    return normalized.ToUpperInvariant() switch
    {
        "MOBILE" => "Mobile",
        "WATCH"  => "Watch",
        _        => "Web"
    };
}
```

Al iniciar sesión se genera un `sessionId` único y se guarda en la plataforma correspondiente:

```csharp
var sessionId = Guid.NewGuid().ToString("N");
user.ActiveSessions[platform] = sessionId;
await _userRepository.UpdateAsync(user);

var token = _jwtService.GenerateToken(user.Id, user.Email, roleName, platform, sessionId);
```

El **`SinglePlatformSessionMiddleware`** valida que el token que llega sea el de la **sesión activa de esa categoría de plataforma**:

```csharp
public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var platform = context.User.FindFirstValue(JwtService.PlatformClaim) ?? "Web";
        var sessionId = context.User.FindFirstValue(JwtService.SessionIdClaim);

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(sessionId))
        {
            var user = await userRepository.GetByIdAsync(userId);
            var activeSession = FindActiveSession(user, platform);

            if (activeSession != null &&
                !string.Equals(activeSession, sessionId, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Sesión rechazada para el usuario {UserId} en {Platform} ...", userId, platform);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Sesión caducada: se ha iniciado sesión en otro dispositivo de esta misma categoría."
                });
                return;
            }
        }
    }
    await _next(context);
}
```

> **Comportamiento**: cada categoría de dispositivo (Web / Mobile / Watch) mantiene **una sola sesión**. Si el mismo usuario inicia sesión en otro dispositivo del mismo tipo, el token anterior queda invalidado y recibe `401 Unauthorized`.

---

### 1.5 `PatientService.cs`: `MapToResponse`, edad dinámica y datos clínicos

`PatientService` es el servicio de dominio que garantiza que **nunca se exponga la entidad de MongoDB** al cliente; toda la salida pasa por `MapToResponse`.

**Cálculo dinámico de la edad** — no se persiste, se deriva de `DateOfBirth` en cada respuesta:

```csharp
private static int CalculateAge(DateTime dateOfBirth)
{
    if (dateOfBirth == default) return 0;

    var today = DateTime.UtcNow;
    var age = today.Year - dateOfBirth.Year;
    if (dateOfBirth.Date > today.AddYears(-age)) age--;

    return age < 0 ? 0 : age;
}
```

**`MapToResponse`** — contrato de salida tipado (`PatientResponse`) con segundo apellido, diagnóstico inicial y doctor asignado:

```csharp
private static PatientResponse MapToResponse(Patient patient)
{
    return new PatientResponse
    {
        Id = patient.Id.ToString(),
        UserId = patient.UserId.ToString(),
        FirstName = patient.FirstName,
        LastName = patient.LastName,
        SecondLastName = patient.SecondLastName,
        DateOfBirth = patient.DateOfBirth,
        Age = CalculateAge(patient.DateOfBirth),
        Gender = patient.Gender,
        Weight = patient.Weight,
        Height = patient.Height,
        BloodType = patient.BloodType,
        Phone = patient.Phone,
        Address = patient.Address,
        PhotoUrl = patient.PhotoUrl,
        Observations = patient.Observations,
        Status = patient.Status,
        EmergencyContacts = patient.EmergencyContacts.Select(c => new EmergencyContactDto
        {
            Id = c.Id.ToString(),
            Name = c.Name,
            Relationship = c.Relationship,
            Phone = c.Phone,
            Email = c.Email,
            IsPrimary = c.IsPrimary
        }).ToList(),
        Medications = patient.Medications?.ToList() ?? new List<string>(),
        InitialDiagnosis = patient.InitialDiagnosis,
        AssignedDoctor = patient.AssignedDoctor,
        CreatedAt = patient.CreatedAt,
        UpdatedAt = patient.UpdatedAt
    };
}
```

**Reglas de dominio**:

- Máximo **3 contactos de emergencia** (`MaxEmergencyContacts = 3`), validado en `CreateAsync` y `UpdateAsync`.
- Ids de contactos: se reutiliza el `ObjectId` recibido o se genera uno nuevo si viene vacío (`ObjectId.TryParse`).
- Los campos clínicos `InitialDiagnosis` (diagnóstico inicial) y `AssignedDoctor` (doctor asignado) son opcionales y se actualizan de forma parcial en `UpdateAsync`.

---

## 2. DevSecOps y Seguridad

### 2.1 Blindaje de secretos

| Control | Implementación |
| --- | --- |
| **Config sensible fuera de git** | `.gitignore` excluye `.env`, `appsettings.Production.json`, `appsettings.*.local.json`, `*.prod.json`, `.pfx`, `.pubxml`. |
| **Sin claves reales commiteadas** | `appsettings.json` solo contiene el *placeholder* `YourSuperSecretKey...` y `mongodb://localhost:27017` (local). |
| **Fail-safe de clave JWT en producción** | El arranque genera una clave aleatoria si detecta placeholder/clave débil, invalidando tokens de sesiones previas (evita funcionar con secretos conocidos públicamente). |
| **Errores sin fuga de información** | `ExceptionHandlingMiddleware`: en producción los errores 500 ocultan el detalle y el stack trace; los logs registran método/ruta/excepción completa. |
| **Contraseñas** | Hash con **BCrypt** (`BCrypt.Net-Next`); nunca se almacena texto plano. |
| **JWT endurecido** | Validación de issuer, audience, lifetime y signing key; claims explícitos (`MapInboundClaims = false`). |
| **Sesiones por plataforma** | `SinglePlatformSessionMiddleware` revoca tokens de la misma categoría de dispositivo al iniciar sesión de nuevo. |
| **Config vía entorno** | Usa User Secrets (Development) o variables de entorno (`Jwt__SecretKey`); el proveedor mapea `__` a `:` con `IOptions<T>`. |

### 2.2 Pipeline de pruebas (GitHub Actions)

`.github/workflows/devsecops.yml` — se ejecuta en `push` y `pull_request` hacia `main`/`develop`, con **permisos mínimos** (`contents: read`), **acciones fijadas por SHA** y concurrencia que cancela ejecuciones duplicadas:

| Etapa | Herramienta |
| --- | --- |
| Build + Test | `dotnet restore` → `dotnet build` (Release) → `dotnet test`, sube TRX como artefacto |
| Gate de dependencias vulnerables | `dotnet list package --vulnerable --include-transitive` — **falla el pipeline** si hay severity `Critical`/`High` |
| Secret Scanning | **TruffleHog** sobre el código y el historial completo |
| Análisis estático (SAST) | **CodeQL** (C#) → resultados en Code Scanning (SARIF) |
| Escaneo de contenedor | Build de la imagen Docker + **Trivy** (CRITICAL/HIGH bloquea el CI) |

### 2.3 Resultado de los 132 tests automatizados

Suite ejecutada localmente en modo Release (`dotnet test HeartCheck.slnx --configuration Release`):

| Proyecto | Resultado |
| --- | --- |
| `HeartCheck.UnitTest` | ✅ **108** superados, 0 fallos |
| `HeartCheck.SecurityTest` | ✅ **19** superados, 0 fallos |
| `HeartCheck.IntegrationTest` | ✅ **5** superados, 0 fallos |
| **TOTAL** | ✅ **132** / 132 — 0 errores, 0 omitidos |

**Cobertura por proyecto:**

- **UnitTest (108)**: servicios de dominio — auth, patients, devices, measurements, alerts, síntomas, estadísticas, planes, notificaciones, emergencias, eventos, settings, audit logs y `PredictionService` (ML).
- **SecurityTest (19)**: hashing BCrypt (hash con salt, verificación correcta/incorrecta), generación y claims del JWT, middleware de excepciones (RFC 7807, sin stack traces en producción) y validadores FluentValidation (BPM 30–250, `ObjectId`, contextos).
- **IntegrationTest (5)**: estructurales con `WebApplicationFactory` — 401 sin token, 401 token inválido, 404 ruta desconocida, 400 payload inválido.

---

## 3. Telemetría y Machine Learning

### 3.1 `MeasurementController` (mediciones / telemetría)

Endpoint REST protegido con `[Authorize]` que registra y consulta la frecuencia cardíaca:

```csharp
[ApiController]
[Route("api/measurements")]
[Authorize]
public class MeasurementController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMeasurementRequest request)
    {
        var userId = GetUserId();
        var response = await _measurementService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetHistory), null, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var measurements = await _measurementService.GetHistoryAsync(userId, from, to, page, pageSize);
        return Ok(measurements);
    }
}
```

**Flujo de registro de una medición (`MeasurementService.CreateAsync`)** — validación y orquestación:

1. Verifica que exista el perfil de paciente.
2. Valida que el dispositivo exista, pertenezca al paciente y esté `active`.
3. Calcula si el BPM es normal según el **contexto** (reposo/activo/ejercicio/sueño) usando `BpmThresholds`:

```csharp
internal static (int Low, int High) Get(string context)
{
    return context.ToLowerInvariant() switch
    {
        "rest"   => (60, 100),
        "active" => (80, 160),
        "sleep"  => (40, 80),
        _        => (60, 100)
    };
}
```

4. **Si es anormal**: calcula tipo de alerta (`high_bpm`/`low_bpm`), **severidad** por desviación del umbral y la persiste junto con un síntoma automático:

```csharp
private static string CalculateSeverity(int bpm, int threshold)
{
    var gap = Math.Abs(bpm - threshold);
    var pct = threshold > 0 ? (double)gap / threshold : 0;

    return pct switch
    {
        > 0.40 => "critical",
        > 0.25 => "high",
        > 0.10 => "medium",
        _      => "low"
    };
}
```

5. Evalúa el **riesgo cardiovascular** con el modelo de ML y devuelve el `RiskAssessmentDto`.

### 3.2 Alertas (`AlertController`)

Cada medición fuera de rango dispara una alerta que el paciente puede **confirmar** o **resolver**:

| Método | Ruta | Acción |
| --- | --- | --- |
| `GET` | `/api/alerts` | Alertas activas del paciente |
| `PUT` | `/api/alerts/{id}/acknowledge` | Confirmar alerta (registra `response_received` como `Event`) |
| `PUT` | `/api/alerts/{id}/resolve` | Resolver alerta (marca `resolved`) |

`AlertService` garantiza que las alertas **solo** pertenezcan al paciente autenticado (compara `alert.PatientId != patient.Id`), previniendo accesos cruzados. Cada confirmación crea un `Event` de trazabilidad (`type = response_received`, `userResponse`, `RespondedAt`).

### 3.3 Análisis predictivo con ML.NET (`PredictionService`)

La evaluación de riesgo usa un **modelo supervisado de clasificación multiclase** (`heartcheck_risk_dataset.csv`) empaquetado en `HeartCheckML.zip`:

- **Características (`MLModelInput`)**: `bpm` (BpmValue), `context`, `age`, `hasSymptoms`.
- **Salida (`MLModelOutput`)**: `PredictedLabel` (`critical` / `moderate` / `low`) y vector `Score`.
- **Contextos**: `rest`, `sleep`, `exercise` — el valor `active` se **normaliza a `exercise`** antes de predecir.

```csharp
public RiskAssessmentDto PredictRisk(float bpm, string context, int age, bool hasSymptoms)
{
    if (_model == null) return FallbackAssessment(bpm, context);

    var input = new MLModelInput
    {
        BpmValue = bpm,
        Context  = NormalizeContext(context),
        Age      = age,
        HasSymptoms = hasSymptoms,
        RiskLevel = "low"
    };

    var predictionEngine = _mlContext.Model
        .CreatePredictionEngine<MLModelInput, MLModelOutput>(_model);
    var prediction = predictionEngine.Predict(input);

    var riskLevel = NormalizeLabel(prediction.PredictedLabel);
    var score = prediction.Score is { Length: > 0 } ? prediction.Score.Max() : 0f;

    return new RiskAssessmentDto
    {
        RiskLevel = riskLevel,
        Score = score,
        Recommendation = GetRecommendation(riskLevel, bpm, context)
    };
}
```

**Comportamiento resiliente (fallback)**:

- Si el modelo no existe o no se puede cargar, se activa una **evaluación por umbrales** (`BpmThresholds`) sin romper la API.
- La ruta del modelo se resuelve primero por `ContentRootPath` y luego por `AppContext.BaseDirectory`, soportando despliegues en IIS y contenedores.

> ⚠️ **Aviso médico**: las recomendaciones del modelo son orientativas y no reemplazan la evaluación de un profesional de la salud.

---

### Resumen visual del flujo de telemetría

```
Device (wearable/watch)
   └─► POST /api/measurements { deviceId, bpm, context, symptoms }      [Authorize]
         ├─ 1. Validación de paciente + dispositivo (MongoDB)
         ├─ 2. ¿BPM dentro del rango según contexto? (BpmThresholds)
         │     └─ NO ► Genera alerta (severity critical/high/medium/low)
         │              └─ Registra síntoma automático
         ├─ 3. Predicción de riesgo con ML.NET (HeartCheckML.zip)
         │     └─ RiskAssessmentDto { riskLevel, score, recommendation }
         └─ 4. Persistencia de la medición (camelCase, ObjectId)
              └─ GET /api/measurements (historial con paginación)
```