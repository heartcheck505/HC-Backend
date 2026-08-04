using HeartCheck.DTOs.Events;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public interface IEventService
    {
        Task<List<EventResponse>> GetByAlertIdAsync(ObjectId userId, ObjectId alertId);
    }
}
