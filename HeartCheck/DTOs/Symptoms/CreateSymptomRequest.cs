namespace HeartCheck.DTOs.Symptoms
{
    public class CreateSymptomRequest
    {
        public string MeasurementId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public double Confidence { get; set; }
        public string? Description { get; set; }
    }
}
