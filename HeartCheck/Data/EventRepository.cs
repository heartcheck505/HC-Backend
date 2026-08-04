using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class EventRepository : IEventRepository
    {
        private readonly MongoDbContext _context;

        public EventRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Event evt)
        {
            await _context.Events.InsertOneAsync(evt);
        }

        public async Task<List<Event>> GetByAlertIdAsync(ObjectId alertId)
        {
            return await _context.Events
                .Find(e => e.AlertId == alertId)
                .SortByDescending(e => e.CreatedAt)
                .ToListAsync();
        }
    }
}
