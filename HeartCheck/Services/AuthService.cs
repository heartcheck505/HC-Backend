using HeartCheck.Data;
using HeartCheck.DTOs.Auth;
using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IPasswordHasher passwordHasher,
            IJwtService jwtService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already registered");
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);

            var role = await _roleRepository.GetByNameAsync("Patient");
            if (role == null)
            {
                role = new Role
                {
                    Name = "Patient",
                    Permissions = new List<string>
                    {
                        "profile:read", "profile:write",
                        "measurements:read",
                        "alerts:read", "alerts:acknowledge",
                        "notifications:read", "notifications:mark_read",
                        "statistics:read",
                        "devices:read",
                        "history:read"
                    },
                    Description = "Standard patient role with basic permissions",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _roleRepository.CreateAsync(role);
            }

            var userRole = new UserRole
            {
                UserId = ObjectId.Parse(user.Id),
                RoleId = role.Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = null
            };
            await _userRoleRepository.CreateAsync(userRole);

            var token = _jwtService.GenerateToken(
                user.Id, user.Email, role.Name);

            return new AuthResponse
            {
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var userRole = await _userRoleRepository.GetByUserIdAsync(
                ObjectId.Parse(user.Id));

            string roleName = "Patient";
            if (userRole != null)
            {
                var role = await _roleRepository.GetByIdAsync(userRole.RoleId);
                if (role != null)
                    roleName = role.Name;
            }

            var token = _jwtService.GenerateToken(
                user.Id, user.Email, roleName);

            return new AuthResponse
            {
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }
    }
}
