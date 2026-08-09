namespace HeartCheck.DTOs.Measurements
{
    public class RiskAssessmentDto
    {
        public string RiskLevel { get; set; } = null!;
        public float Score { get; set; }
        public string Recommendation { get; set; } = null!;
    }
}