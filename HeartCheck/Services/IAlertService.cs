using HeartCheck.DTOs.Alerts;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public interface IAlertService
    {
        Task<List<AlertResponse>> GetActiveAlertsByPatientAsync(ObjectId userId);
        Task AcknowledgeAlertAsync(ObjectId userId, ObjectId alertId, string? userResponse);
        Task ResolveAlertAsync(ObjectId userId, ObjectId alertId);
    }
}
