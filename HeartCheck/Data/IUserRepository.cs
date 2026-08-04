using HeartCheck.Models;

namespace HeartCheck.Data
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);
        Task CreateAsync(User user);
        Task UpdateAsync(User user);
    }
}
