using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class RoleRepository : IRoleRepository
    {
        private readonly MongoDbContext _context;

        public RoleRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _context.Roles
                .Find(r => r.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task<Role?> GetByIdAsync(ObjectId id)
        {
            return await _context.Roles
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Role role)
        {
            await _context.Roles.InsertOneAsync(role);
        }
    }
}
