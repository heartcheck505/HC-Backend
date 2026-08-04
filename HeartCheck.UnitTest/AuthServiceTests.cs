using FluentAssertions;
using Moq;
using Xunit;
using HeartCheck.Data;
using HeartCheck.DTOs.Auth;
using HeartCheck.Models;
using HeartCheck.Services;

namespace HeartCheck.UnitTest
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _jwtServiceMock = new Mock<IJwtService>();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _userRoleRepositoryMock.Object,
                _passwordHasherMock.Object,
                _jwtServiceMock.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_Success_ReturnsAuthResponse()
        {
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe",
                Phone = "1234567890"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(registerRequest.Email))
                .ReturnsAsync((User?)null);

            _passwordHasherMock
                .Setup(x => x.Hash(registerRequest.Password))
                .Returns("hashed_password");

            var createdUserId = "507f1f77bcf86cd799439011";
            var user = new User
            {
                Id = createdUserId,
                Email = registerRequest.Email,
                PasswordHash = "hashed_password",
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                Phone = registerRequest.Phone,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

             _roleRepositoryMock
                .Setup(x => x.GetByNameAsync("Patient"))
                .ReturnsAsync((Role?)null);

            _userRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Callback<User>(u => u.Id = createdUserId)
                .Returns(Task.CompletedTask);

            _userRoleRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<UserRole>()))
                .Returns(Task.CompletedTask);

            _jwtServiceMock
                .Setup(x => x.GenerateToken(createdUserId, registerRequest.Email, "Patient"))
                .Returns("generated_jwt_token");

            var result = await _authService.RegisterAsync(registerRequest);

            result.Email.Should().Be(registerRequest.Email);
            result.Token.Should().Be("generated_jwt_token");

            _userRepositoryMock.Verify(x => x.GetByEmailAsync(registerRequest.Email), Times.Once);
            _userRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(u => u.Email == registerRequest.Email)), Times.Once);
            _roleRepositoryMock.Verify(x => x.CreateAsync(It.Is<Role>(r => r.Name == "Patient" && r.Permissions.Count == 10)), Times.Once);
            _userRoleRepositoryMock.Verify(x => x.CreateAsync(It.Is<UserRole>(ur => ur.UserId.ToString() == createdUserId)), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
        {
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe"
            };

            var existingUser = new User { Email = registerRequest.Email };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(registerRequest.Email))
                .ReturnsAsync(existingUser);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(registerRequest)
            );

            _userRepositoryMock.Verify(x => x.GetByEmailAsync(registerRequest.Email), Times.Once);
            _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_Success_ReturnsAuthResponse()
        {
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            var user = new User
            {
                Id = "507f1f77bcf86cd799439011",
                Email = loginRequest.Email,
                PasswordHash = "hashed_password",
                FirstName = "John",
                LastName = "Doe",
                LastLogin = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify(loginRequest.Password, user.PasswordHash))
                .Returns(true);

            var patientRole = new Role
            {
                Id = ObjectId.Parse("507f1f77bcf86cd799439022"),
                Name = "Patient"
            };

            var userRole = new UserRole
            {
                UserId = ObjectId.Parse(user.Id),
                RoleId = patientRole.Id
            };

            _userRoleRepositoryMock
                .Setup(x => x.GetByUserIdAsync(ObjectId.Parse(user.Id)))
                .ReturnsAsync(userRole);

            _roleRepositoryMock
                .Setup(x => x.GetByIdAsync(patientRole.Id))
                .ReturnsAsync(patientRole);

            _jwtServiceMock
                .Setup(x => x.GenerateToken(user.Id, user.Email, "Patient"))
                .Returns("generated_jwt_token");

            var result = await _authService.LoginAsync(loginRequest);

            result.Email.Should().Be(user.Email);
            result.Token.Should().Be("generated_jwt_token");

            _userRepositoryMock.Verify(x => x.GetByEmailAsync(loginRequest.Email), Times.Once);
            _passwordHasherMock.Verify(x => x.Verify(loginRequest.Password, user.PasswordHash), Times.Once);
            _userRoleRepositoryMock.Verify(x => x.GetByUserIdAsync(ObjectId.Parse(user.Id)), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_ThrowsUnauthorizedAccessException()
        {
            var loginRequest = new LoginRequest
            {
                Email = "wrong@example.com",
                Password = "WrongPassword"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(loginRequest.Email))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(loginRequest)
            );

            _userRepositoryMock.Verify(x => x.GetByEmailAsync(loginRequest.Email), Times.Once);
            _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
