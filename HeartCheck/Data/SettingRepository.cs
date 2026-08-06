using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class SettingRepository : ISettingRepository
    {
        private readonly MongoDbContext _context;

        public SettingRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Setting?> GetByKeyAsync(string key)
        {
            return await _context.Settings
                .Find(s => s.Key == key)
                .FirstOrDefaultAsync();
        }

        public async Task<Setting?> GetByIdAsync(ObjectId id)
        {
            return await _context.Settings
                .Find(s => s.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Setting>> GetByCategoryAsync(string category)
        {
            return await _context.Settings
                .Find(s => s.Category == category)
                .ToListAsync();
        }

        public async Task<List<Setting>> GetAllAsync()
        {
            return await _context.Settings
                .Find(_ => true)
                .ToListAsync();
        }

        public async Task CreateAsync(Setting setting)
        {
            await _context.Settings.InsertOneAsync(setting);
        }

        public async Task UpdateAsync(Setting setting)
        {
            await _context.Settings.ReplaceOneAsync(s => s.Id == setting.Id, setting);
        }
    }
}
