namespace HeartCheck.DTOs.Events
{
    public class EventResponse
    {
        public string Id { get; set; } = null!;
        public string AlertId { get; set; } = null!;
        public string PatientId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? UserResponse { get; set; }
        public DateTime RespondedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
