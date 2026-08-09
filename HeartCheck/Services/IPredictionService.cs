using HeartCheck.DTOs.Measurements;

namespace HeartCheck.Services
{
    public interface IPredictionService
    {
        RiskAssessmentDto PredictRisk(float bpm, string context, int age, bool hasSymptoms);
    }
}