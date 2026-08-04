namespace HeartCheck.DTOs.Measurements
{
    public class CreateMeasurementRequest
    {
        public string DeviceId { get; set; } = null!;
        public int Bpm { get; set; }
        public string Quality { get; set; } = null!;
        public string Context { get; set; } = null!;
        public string? Notes { get; set; }
    }
}
