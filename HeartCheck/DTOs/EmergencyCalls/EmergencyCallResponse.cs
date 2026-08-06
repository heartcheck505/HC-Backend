namespace HeartCheck.DTOs.EmergencyCalls
{
    public class EmergencyCallResponse
    {
        public string Id { get; set; } = null!;
        public string AlertId { get; set; } = null!;
        public string PatientId { get; set; } = null!;
        public string EmergencyContactId { get; set; } = null!;
        public string ContactName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int? Duration { get; set; }
        public string? Result { get; set; }
        public DateTime? InitiatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
