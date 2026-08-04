using MongoDB.Bson;

namespace HeartCheck.DTOs.Events
{
    public class CreateEventRequest
    {
        public string AlertId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? UserResponse { get; set; }
    }
}
