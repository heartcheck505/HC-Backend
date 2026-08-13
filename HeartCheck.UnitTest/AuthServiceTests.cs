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
        private readonly Mock<IPatientRepository> _patientRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
            _patientRepositoryMock = new Mock<IPatientRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _jwtServiceMock = new Mock<IJwtService>();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _userRoleRepositoryMock.Object,
                _patientRepositoryMock.Object,
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
                .Setup(x => x.GenerateToken(createdUserId, registerRequest.Email, "Patient", "Web", It.IsAny<string>()))
                .Returns("generated_jwt_token");

            var result = await _authService.RegisterAsync(registerRequest);

            result.Email.Should().Be(registerRequest.Email);
            result.Token.Should().Be("generated_jwt_token");

            _userRepositoryMock.Verify(x => x.GetByEmailAsync(registerRequest.Email), Times.Once);
            _userRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(u => u.Email == registerRequest.Email)), Times.Once);
            _roleRepositoryMock.Verify(x => x.CreateAsync(It.Is<Role>(r => r.Name == "Patient" && r.Permissions.Count == 10)), Times.Once);
            _userRoleRepositoryMock.Verify(x => x.CreateAsync(It.Is<UserRole>(ur => ur.UserId.ToString() == createdUserId)), Times.Once);
            _patientRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Patient>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WithCompletePatientProfile_SavesPatientLinkedToNewUserId()
        {
            var registerRequest = new RegisterRequest
            {
                Email = "profile@example.com",
                Password = "Password123",
                FirstName = "Jane",
                LastName = "Roe",
                Phone = "1234567890",
                DateOfBirth = new DateTime(1992, 5, 10),
                Gender = "female",
                Weight = 60,
                Height = 165,
                BloodType = "O+",
                Address = "123 Main St",
                EmergencyContacts = new List<EmergencyContactDto>
                {
                    new EmergencyContactDto
                    {
                        Name = "Contact",
                        Relationship = "family",
                        Phone = "0987654321",
                        IsPrimary = true
                    }
                },
                Medications = new List<string> { "atorvastatin" }
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(registerRequest.Email))
                .ReturnsAsync((User?)null);

            _passwordHasherMock
                .Setup(x => x.Hash(registerRequest.Password))
                .Returns("hashed_password");

            var createdUserId = "507f1f77bcf86cd799439033";
            _userRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Callback<User>(u => u.Id = createdUserId)
                .Returns(Task.CompletedTask);

            _roleRepositoryMock
                .Setup(x => x.GetByNameAsync("Patient"))
                .ReturnsAsync(new Role
                {
                    Id = ObjectId.Parse("507f1f77bcf86cd799439022"),
                    Name = "Patient"
                });

            _userRoleRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<UserRole>()))
                .Returns(Task.CompletedTask);

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(ObjectId.Parse(createdUserId)))
                .ReturnsAsync((Patient?)null);

            _patientRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Patient>()))
                .Returns(Task.CompletedTask);

            _jwtServiceMock
                .Setup(x => x.GenerateToken(createdUserId, registerRequest.Email, "Patient", "Web", It.IsAny<string>()))
                .Returns("generated_jwt_token");

            var result = await _authService.RegisterAsync(registerRequest);

            result.Token.Should().Be("generated_jwt_token");

            _patientRepositoryMock.Verify(x => x.CreateAsync(It.Is<Patient>(p =>
                p.UserId.ToString() == createdUserId &&
                p.FirstName == "Jane" &&
                p.LastName == "Roe" &&
                p.DateOfBirth == registerRequest.DateOfBirth &&
                p.Gender == "female" &&
                p.Weight == 60 &&
                p.Height == 165 &&
                p.BloodType == "O+" &&
                p.EmergencyContacts.Count == 1 &&
                p.Medications.Contains("atorvastatin")
            )), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithAgeAndEmergencyContactName_MapsToDateOfBirthAndEmergencyContacts()
        {
            var registerRequest = new RegisterRequest
            {
                Email = "agecontact@example.com",
                Password = "Password123",
                FirstName = "Ana",
                LastName = "Perez",
                Phone = "1234567890",
                Age = 30,
                Gender = "female",
                Weight = 55,
                Height = 160,
                EmergencyContactName = "Luis",
                EmergencyRelationship = "hermano",
                EmergencyPhone = "0987654321",
                EmergencyEmail = "luis@example.com",
                SetAsPrimaryEmergency = true
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(registerRequest.Email))
                .ReturnsAsync((User?)null);

            _passwordHasherMock
                .Setup(x => x.Hash(registerRequest.Password))
                .Returns("hashed_password");

            var createdUserId = "507f1f77bcf86cd799439044";
            _userRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Callback<User>(u => u.Id = createdUserId)
                .Returns(Task.CompletedTask);

            _roleRepositoryMock
                .Setup(x => x.GetByNameAsync("Patient"))
                .ReturnsAsync(new Role
                {
                    Id = ObjectId.Parse("507f1f77bcf86cd799439022"),
                    Name = "Patient"
                });

            _userRoleRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<UserRole>()))
                .Returns(Task.CompletedTask);

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(ObjectId.Parse(createdUserId)))
                .ReturnsAsync((Patient?)null);

            Patient? capturedPatient = null;
            _patientRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Patient>()))
                .Callback<Patient>(p => capturedPatient = p)
                .Returns(Task.CompletedTask);

            _jwtServiceMock
                .Setup(x => x.GenerateToken(createdUserId, registerRequest.Email, "Patient", "Web", It.IsAny<string>()))
                .Returns("generated_jwt_token");

            await _authService.RegisterAsync(registerRequest);

            capturedPatient.Should().NotBeNull();
            capturedPatient!.EmergencyContacts.Should().HaveCount(1);
            capturedPatient.EmergencyContacts[0].Name.Should().Be("Luis");
            capturedPatient.EmergencyContacts[0].Relationship.Should().Be("hermano");
            capturedPatient.EmergencyContacts[0].Phone.Should().Be("0987654321");
            capturedPatient.EmergencyContacts[0].Email.Should().Be("luis@example.com");
            capturedPatient.EmergencyContacts[0].IsPrimary.Should().BeTrue();

            var expectedBirth = DateTime.UtcNow.AddYears(-30);
            capturedPatient.DateOfBirth.Date.Should().Be(expectedBirth.Date);
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
                .Setup(x => x.GenerateToken(
                    user.Id, user.Email, "Patient", "Web", It.IsAny<string>()))
                .Returns("generated_jwt_token");

            var result = await _authService.LoginAsync(loginRequest);

            result.Email.Should().Be(user.Email);
            result.Token.Should().Be("generated_jwt_token");

            _userRepositoryMock.Verify(x => x.GetByEmailAsync(loginRequest.Email), Times.Once);
            _passwordHasherMock.Verify(x => x.Verify(loginRequest.Password, user.PasswordHash), Times.Once);
            _userRoleRepositoryMock.Verify(x => x.GetByUserIdAsync(ObjectId.Parse(user.Id)), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_SamePlatform_ReplacesPreviousSessionButKeepsOthers()
        {
            var firstName = "John";
            var lastName = "Doe";
            var email = "test@example.com";
            var user = new User
            {
                Id = "507f1f77bcf86cd799439011",
                Email = email,
                PasswordHash = "hashed_password",
                FirstName = firstName,
                LastName = lastName,
                ActiveSessions = new Dictionary<string, string>
                {
                    ["Web"] = "old-web-session",
                    ["Mobile"] = "mobile-session-stays"
                }
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify("Password123", user.PasswordHash))
                .Returns(true);

            _roleRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<ObjectId>()))
                .ReturnsAsync(new Role { Id = ObjectId.GenerateNewId(), Name = "Patient" });

            _jwtServiceMock
                .Setup(x => x.GenerateToken(
                    user.Id, email, "Patient", "Web", It.IsAny<string>()))
                .Returns("new_web_token");

            var result = await _authService.LoginAsync(new LoginRequest
            {
                Email = email,
                Password = "Password123",
                Platform = "Web"
            });

            result.Token.Should().Be("new_web_token");

            _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u =>
                u.ActiveSessions["Web"] != "old-web-session" &&
                u.ActiveSessions["Mobile"] == "mobile-session-stays"
            )), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_DifferentPlatform_KeepsBothSessions()
        {
            var email = "test@example.com";
            var user = new User
            {
                Id = "507f1f77bcf86cd799439011",
                Email = email,
                PasswordHash = "hashed_password",
                FirstName = "John",
                LastName = "Doe",
                ActiveSessions = new Dictionary<string, string>
                {
                    ["Web"] = "web-session-1"
                }
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify("Password123", user.PasswordHash))
                .Returns(true);

            _roleRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<ObjectId>()))
                .ReturnsAsync(new Role { Id = ObjectId.GenerateNewId(), Name = "Patient" });

            _jwtServiceMock
                .Setup(x => x.GenerateToken(
                    user.Id, email, "Patient", "Mobile", It.IsAny<string>()))
                .Returns("mobile_token");

            await _authService.LoginAsync(new LoginRequest
            {
                Email = email,
                Password = "Password123",
                Platform = "Mobile"
            });

            _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u =>
                u.ActiveSessions["Web"] == "web-session-1" &&
                !string.IsNullOrEmpty(u.ActiveSessions["Mobile"])
            )), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_UnknownPlatform_NormalizesToWeb()
        {
            var email = "test@example.com";
            var user = new User
            {
                Id = "507f1f77bcf86cd799439011",
                Email = email,
                PasswordHash = "hashed_password",
                FirstName = "John",
                LastName = "Doe",
                ActiveSessions = new Dictionary<string, string>()
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify("Password123", user.PasswordHash))
                .Returns(true);

            _roleRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<ObjectId>()))
                .ReturnsAsync(new Role { Id = ObjectId.GenerateNewId(), Name = "Patient" });

            _jwtServiceMock
                .Setup(x => x.GenerateToken(
                    user.Id, email, "Patient", "Web", It.IsAny<string>()))
                .Returns("web_token");

            await _authService.LoginAsync(new LoginRequest
            {
                Email = email,
                Password = "Password123",
                Platform = "unknown-device"
            });

            _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u =>
                u.ActiveSessions.ContainsKey("Web")
            )), Times.Once);
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
