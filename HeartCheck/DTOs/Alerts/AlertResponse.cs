namespace HeartCheck.DTOs.Alerts
{
    public class AlertResponse
    {
        public string Id { get; set; } = null!;
        public string MeasurementId { get; set; } = null!;
        public string PatientId { get; set; } = null!;
        public string DeviceId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Severity { get; set; } = null!;
        public int BpmValue { get; set; }
        public int Threshold { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
