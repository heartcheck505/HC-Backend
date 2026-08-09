namespace HeartCheck.DTOs.Measurements
{
    public class MLModelInput
    {
        public float BpmValue { get; set; }
        public string Context { get; set; } = null!;
        public float Age { get; set; }
        public bool HasSymptoms { get; set; }
        public string RiskLevel { get; set; } = null!;
    }
}