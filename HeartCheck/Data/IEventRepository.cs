using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface IEventRepository
    {
        Task CreateAsync(Event evt);
        Task<List<Event>> GetByAlertIdAsync(ObjectId alertId);
    }
}
