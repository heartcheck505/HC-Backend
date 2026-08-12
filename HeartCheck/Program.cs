using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using HeartCheck.Configurations;
using HeartCheck.Data;
using HeartCheck.Middleware;
using HeartCheck.Services;
using HeartCheck.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

var builder = WebApplication.CreateBuilder(args);

var conventionPack = new ConventionPack
{
    new CamelCaseElementNameConvention()
};
ConventionRegistry.Register("camelCase", conventionPack, _ => true);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();

builder.Services.AddScoped<IPatientRepository, PatientRepository>();

builder.Services.AddScoped<IPatientService, PatientService>();

builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();

builder.Services.AddScoped<IDeviceService, DeviceService>();

builder.Services.AddScoped<IMeasurementRepository, MeasurementRepository>();

builder.Services.AddScoped<IMeasurementService, MeasurementService>();

builder.Services.AddSingleton<IPredictionService, PredictionService>();

builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();

builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IEventService, EventService>();

builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<IUserPlanRepository, UserPlanRepository>();

builder.Services.AddScoped<IPlanService, PlanService>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<IDailyStatisticRepository, DailyStatisticRepository>();
builder.Services.AddScoped<IDailyStatisticService, DailyStatisticService>();

builder.Services.AddScoped<IEmergencyCallRepository, EmergencyCallRepository>();
builder.Services.AddScoped<IEmergencyCallService, EmergencyCallService>();

builder.Services.AddScoped<ISymptomRepository, SymptomRepository>();
builder.Services.AddScoped<ISymptomService, SymptomService>();


builder.Services.AddScoped<ISettingRepository, SettingRepository>();
builder.Services.AddScoped<ISettingService, SettingService>();

builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();


builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>();

if (jwtSettings is null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException(
        "La configuración Jwt es inválida: la clave 'Jwt:SecretKey' no está configurada.");
}

const string defaultPlaceholder = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!!";
var isPlaceholder = string.Equals(
    jwtSettings.SecretKey, defaultPlaceholder, StringComparison.Ordinal);

if (!builder.Environment.IsDevelopment() &&
    (isPlaceholder || jwtSettings.SecretKey.Length < 32))
{
    throw new InvalidOperationException(
        "La configuración Jwt es insegura: 'Jwt:SecretKey' debe ser una clave de al menos 32 " +
        "caracteres y no puede ser el valor de ejemplo por defecto fuera del entorno Development. " +
        "Configúrala mediante User Secrets o variables de entorno (por ejemplo, Jwt__SecretKey).");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
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

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateMeasurementRequestValidator>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
