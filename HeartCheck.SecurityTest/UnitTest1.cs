using System.Net;
using System.Text;
using System.Text.Json;
using HeartCheck.Configurations;
using HeartCheck.DTOs.Measurements;
using HeartCheck.Middleware;
using HeartCheck.Services;
using HeartCheck.Validators;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace HeartCheck.SecurityTest
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher = new();

        [Fact]
        public void Hash_ProducesSaltedHash_DifferentFromPlainPassword()
        {
            var hash = _hasher.Hash("MyS3curePass!");

            Assert.False(string.IsNullOrWhiteSpace(hash));
            Assert.NotEqual("MyS3curePass!", hash);
        }

        [Fact]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            var hash = _hasher.Hash("MyS3curePass!");

            var result = _hasher.Verify("MyS3curePass!", hash);

            Assert.True(result);
        }

        [Fact]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            var hash = _hasher.Hash("MyS3curePass!");

            var result = _hasher.Verify("WrongPass!", hash);

            Assert.False(result);
        }

        [Fact]
        public void Hash_SamePassword_ProducesDifferentHashes()
        {
            var hash1 = _hasher.Hash("MyS3curePass!");
            var hash2 = _hasher.Hash("MyS3curePass!");

            Assert.NotEqual(hash1, hash2);
        }
    }

    public class JwtServiceTests
    {
        private static JwtService CreateJwtService()
        {
            var settings = Options.Create(new JwtSettings
            {
                SecretKey = "ThisIsATestSecretKeyThatIsAtLeast32CharactersLong!!",
                Issuer = "HeartCheck",
                Audience = "HeartCheck.Client",
                ExpirationHours = 8
            });

            return new JwtService(settings);
        }

        [Fact]
        public void GenerateToken_ContainsRoleClaim()
        {
            var jwtService = CreateJwtService();

            var token = jwtService.GenerateToken("user123", "user@test.com", "Admin");

            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.Contains(token, t => t == '.');

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal("HeartCheck", jwt.Issuer);
            Assert.Equal("HeartCheck.Client", jwt.Audiences.First());
            Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Admin");
            Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Email && c.Value == "user@test.com");
        }
    }

    public class ExceptionHandlingMiddlewareTests
    {
        private static Mock<ILogger<ExceptionHandlingMiddleware>> CreateLogger()
        {
            return new Mock<ILogger<ExceptionHandlingMiddleware>>();
        }

        private static DefaultHttpContext CreateContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/test";
            context.Response.Body = new MemoryStream();
            return context;
        }

        private static async Task<string> InvokeAsync(
            RequestDelegate next,
            string environmentName)
        {
            var context = CreateContext();

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.EnvironmentName).Returns(environmentName);

            var middleware = new ExceptionHandlingMiddleware(next, CreateLogger().Object, envMock.Object);
            await middleware.InvokeAsync(context);

            context.Response.Body.Position = 0;
            using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        [Fact]
        public async Task InvokeAsync_UnhandledException_Returns500ProblemDetails()
        {
            RequestDelegate next = _ => throw new ApplicationException("boom");

            var body = await InvokeAsync(next, "Production");

            var problem = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(500, problem.GetProperty("status").GetInt32());
            Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("title").GetString()));
        }

        [Fact]
        public async Task InvokeAsync_UnhandledException_DoesNotLeakStackTraceInProduction()
        {
            RequestDelegate next = _ => throw new ApplicationException("boom");

            var body = await InvokeAsync(next, "Production");

            Assert.DoesNotContain("at HeartCheck", body);
            Assert.DoesNotContain("ExceptionHandlingMiddleware.cs", body);
            Assert.DoesNotContain("stackTrace", body);
        }

        [Fact]
        public async Task InvokeAsync_UnhandledException_InDevelopment_IncludesStackTrace()
        {
            RequestDelegate next = _ => throw new ApplicationException("boom");

            var body = await InvokeAsync(next, "Development");

            var problem = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.True(problem.TryGetProperty("stackTrace", out _));
        }

        [Fact]
        public async Task InvokeAsync_KeyNotFoundException_Returns404()
        {
            RequestDelegate next = _ => throw new KeyNotFoundException("resource not found");

            var body = await InvokeAsync(next, "Production");

            var problem = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(404, problem.GetProperty("status").GetInt32());
        }

        [Fact]
        public async Task InvokeAsync_UnauthorizedAccessException_Returns401()
        {
            RequestDelegate next = _ => throw new UnauthorizedAccessException("no access");

            var body = await InvokeAsync(next, "Production");

            var problem = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(401, problem.GetProperty("status").GetInt32());
        }
    }

    public class CreateMeasurementRequestValidatorTests
    {
        private readonly CreateMeasurementRequestValidator _validator = new();

        [Theory]
        [InlineData(30)]
        [InlineData(250)]
        [InlineData(75)]
        public void Validate_BpmWithinRange_IsValid(int bpm)
        {
            var request = ValidRequest();
            request.Bpm = bpm;

            var result = _validator.Validate(request);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(29)]
        [InlineData(251)]
        [InlineData(0)]
        [InlineData(-5)]
        public void Validate_BpmOutOfRange_IsInvalid(int bpm)
        {
            var request = ValidRequest();
            request.Bpm = bpm;

            var result = _validator.Validate(request);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateMeasurementRequest.Bpm));
        }

        [Fact]
        public void Validate_InvalidDeviceId_IsInvalid()
        {
            var request = ValidRequest();
            request.DeviceId = "not-a-valid-objectid";

            var result = _validator.Validate(request);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateMeasurementRequest.DeviceId));
        }

        [Fact]
        public void Validate_InvalidContext_IsInvalid()
        {
            var request = ValidRequest();
            request.Context = "running";

            var result = _validator.Validate(request);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateMeasurementRequest.Context));
        }

        private static CreateMeasurementRequest ValidRequest()
        {
            return new CreateMeasurementRequest
            {
                DeviceId = "507f1f77bcf86cd799439011",
                Bpm = 75,
                Quality = "good",
                Context = "rest"
            };
        }
    }
}
