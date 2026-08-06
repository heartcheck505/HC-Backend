namespace HeartCheck.DTOs.EmergencyCalls
{
    public class CreateEmergencyCallRequest
    {
        public string AlertId { get; set; } = null!;
        public string EmergencyContactId { get; set; } = null!;
    }
}
