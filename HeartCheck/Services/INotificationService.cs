using HeartCheck.DTOs.Notifications;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public interface INotificationService
    {
        Task<List<NotificationResponse>> GetUserNotificationsAsync(ObjectId userId);
        Task MarkAsReadAsync(ObjectId notificationId, ObjectId userId);
        Task<NotificationResponse> CreateNotificationAsync(ObjectId userId, CreateNotificationRequest request);
    }
}