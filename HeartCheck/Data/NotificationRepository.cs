using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly MongoDbContext _context;

        public NotificationRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetByUserIdAsync(ObjectId userId)
        {
            return await _context.Notifications
                .Find(n => n.UserId == userId)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateAsync(Notification notification)
        {
            await _context.Notifications.InsertOneAsync(notification);
        }

        public async Task UpdateAsync(Notification notification)
        {
            await _context.Notifications.ReplaceOneAsync(
                n => n.Id == notification.Id, notification);
        }
    }
}