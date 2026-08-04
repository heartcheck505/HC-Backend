using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface IUserRoleRepository
    {
        Task CreateAsync(UserRole userRole);
        Task<UserRole?> GetByUserIdAsync(ObjectId userId);
    }
}
