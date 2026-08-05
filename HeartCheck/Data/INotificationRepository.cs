using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetByUserIdAsync(ObjectId userId);
        Task CreateAsync(Notification notification);
        Task UpdateAsync(Notification notification);
    }
}