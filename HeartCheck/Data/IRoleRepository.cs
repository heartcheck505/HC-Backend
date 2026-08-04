using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string name);
        Task<Role?> GetByIdAsync(ObjectId id);
        Task CreateAsync(Role role);
    }
}
