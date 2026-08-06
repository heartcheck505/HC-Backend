namespace HeartCheck.DTOs.EmergencyCalls
{
    public class UpdateEmergencyCallStatusRequest
    {
        public string Status { get; set; } = null!;
        public int? Duration { get; set; }
        public string? Result { get; set; }
    }
}
