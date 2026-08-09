namespace HeartCheck.DTOs.Measurements
{
    public class MeasurementResponse
    {
        public DateTime Timestamp { get; set; }
        public string PatientId { get; set; } = null!;
        public string DeviceId { get; set; } = null!;
        public int Bpm { get; set; }
        public string Quality { get; set; } = null!;
        public string Context { get; set; } = null!;
        public bool IsNormal { get; set; }
        public string? Notes { get; set; }
        public RiskAssessmentDto? RiskAssessment { get; set; }
    }
}
