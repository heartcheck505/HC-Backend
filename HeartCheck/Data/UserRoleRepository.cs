using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly MongoDbContext _context;

        public UserRoleRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(UserRole userRole)
        {
            await _context.UserRoles.InsertOneAsync(userRole);
        }

        public async Task<UserRole?> GetByUserIdAsync(ObjectId userId)
        {
            return await _context.UserRoles
                .Find(ur => ur.UserId == userId)
                .FirstOrDefaultAsync();
        }
    }
}
