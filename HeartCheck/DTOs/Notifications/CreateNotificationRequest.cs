namespace HeartCheck.DTOs.Notifications
{
    public class CreateNotificationRequest
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}