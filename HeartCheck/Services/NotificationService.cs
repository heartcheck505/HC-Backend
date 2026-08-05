using HeartCheck.Data;
using HeartCheck.DTOs.Notifications;
using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<List<NotificationResponse>> GetUserNotificationsAsync(ObjectId userId)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);
            return notifications.Select(MapToResponse).ToList();
        }

        public async Task MarkAsReadAsync(ObjectId notificationId, ObjectId userId)
        {
            var notification = await _notificationRepository.GetByUserIdAsync(userId);
            var target = notification.FirstOrDefault(n => n.Id == notificationId);
            if (target == null)
            {
                throw new KeyNotFoundException("Notification not found");
            }

            target.IsRead = true;
            target.ReadAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(target);
        }

        public async Task<NotificationResponse> CreateNotificationAsync(ObjectId userId, CreateNotificationRequest request)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(notification);
            return MapToResponse(notification);
        }

        private static NotificationResponse MapToResponse(Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id.ToString(),
                UserId = notification.UserId.ToString(),
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}