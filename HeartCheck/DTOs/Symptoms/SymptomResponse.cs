namespace HeartCheck.DTOs.Symptoms
{
    public class SymptomResponse
    {
        public string Id { get; set; } = null!;
        public string MeasurementId { get; set; } = null!;
        public string PatientId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public double Confidence { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
