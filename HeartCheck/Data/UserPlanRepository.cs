using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class UserPlanRepository : IUserPlanRepository
    {
        private readonly MongoDbContext _context;

        public UserPlanRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<UserPlan?> GetActiveByUserIdAsync(ObjectId userId)
        {
            return await _context.UserPlans
                .Find(up => up.UserId == userId && up.Status == "active")
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserPlan>> GetByUserIdAsync(ObjectId userId)
        {
            return await _context.UserPlans
                .Find(up => up.UserId == userId)
                .SortByDescending(up => up.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateAsync(UserPlan userPlan)
        {
            await _context.UserPlans.InsertOneAsync(userPlan);
        }

        public async Task UpdateAsync(UserPlan userPlan)
        {
            await _context.UserPlans.ReplaceOneAsync(
                up => up.Id == userPlan.Id, userPlan);
        }
    }
}
