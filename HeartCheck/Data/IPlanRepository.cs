using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface IPlanRepository
    {
        Task<Plan?> GetByIdAsync(ObjectId id);
        Task<List<Plan>> GetAllActiveAsync();
        Task CreateAsync(Plan plan);
        Task SeedDefaultPlansAsync();
    }
}
