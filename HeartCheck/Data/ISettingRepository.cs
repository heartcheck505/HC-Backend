using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface ISettingRepository
    {
        Task<Setting?> GetByKeyAsync(string key);
        Task<Setting?> GetByIdAsync(ObjectId id);
        Task<List<Setting>> GetByCategoryAsync(string category);
        Task<List<Setting>> GetAllAsync();
        Task CreateAsync(Setting setting);
        Task UpdateAsync(Setting setting);
    }
}
