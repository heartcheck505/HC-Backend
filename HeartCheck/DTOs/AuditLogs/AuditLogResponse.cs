namespace HeartCheck.DTOs.AuditLogs
{
    public class AuditLogResponse
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public object? PreviousValues { get; set; }
        public object? NewValues { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
