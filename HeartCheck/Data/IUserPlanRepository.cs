using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface IUserPlanRepository
    {
        Task<UserPlan?> GetActiveByUserIdAsync(ObjectId userId);
        Task<List<UserPlan>> GetByUserIdAsync(ObjectId userId);
        Task CreateAsync(UserPlan userPlan);
        Task UpdateAsync(UserPlan userPlan);
    }
}
