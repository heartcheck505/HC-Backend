using HeartCheck.DTOs.Measurements;
using Microsoft.ML;

namespace HeartCheck.Services
{
    public class PredictionService : IPredictionService
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer? _model;

        public PredictionService(IWebHostEnvironment environment)
        {
            _mlContext = new MLContext();

            var modelPath = Path.Combine(environment.ContentRootPath, "HeartCheckML.zip");
            if (!File.Exists(modelPath))
            {
                modelPath = Path.Combine(AppContext.BaseDirectory, "HeartCheckML.zip");
            }

            if (File.Exists(modelPath))
            {
                _model = _mlContext.Model.Load(modelPath, out _);
            }
        }

        public RiskAssessmentDto PredictRisk(float bpm, string context, int age, bool hasSymptoms)
        {
            if (_model == null)
            {
                return FallbackAssessment(bpm, context);
            }

            var input = new MLModelInput
            {
                BpmValue = bpm,
                Context = NormalizeContext(context),
                Age = age,
                HasSymptoms = hasSymptoms,
                RiskLevel = "low"
            };

            var predictionEngine = _mlContext.Model
                .CreatePredictionEngine<MLModelInput, MLModelOutput>(_model);

            var prediction = predictionEngine.Predict(input);

            var riskLevel = NormalizeLabel(prediction.PredictedLabel);
            var score = prediction.Score is { Length: > 0 }
                ? prediction.Score.Max()
                : 0f;

            return new RiskAssessmentDto
            {
                RiskLevel = riskLevel,
                Score = score,
                Recommendation = GetRecommendation(riskLevel, bpm, context)
            };
        }

        private static string NormalizeContext(string context)
        {
            return context.ToLowerInvariant() switch
            {
                "rest" or "sleep" => context.ToLowerInvariant(),
                "active" or "exercise" => "exercise",
                _ => "rest"
            };
        }

        private static string NormalizeLabel(string? label)
        {
            return label?.ToLowerInvariant() switch
            {
                "critical" => "critical",
                "moderate" => "moderate",
                _ => "low"
            };
        }

        private static string GetRecommendation(string riskLevel, float bpm, string context)
        {
            return riskLevel switch
            {
                "critical" =>
                    "Tu frecuencia cardíaca es significativamente anormal. " +
                    "Comunícate de inmediato con un profesional de la salud o acude a urgencias.",
                "moderate" =>
                    "Tu frecuencia cardíaca presenta variaciones fuera de lo habitual. " +
                    "Descansa unos minutos, hidrátate y toma una nueva medición en un rato.",
                _ =>
                    "Tu frecuencia cardíaca se encuentra dentro de los valores esperados. " +
                    "Continúa con tu rutina habitual y mantén el monitoreo."
            };
        }

        private static RiskAssessmentDto FallbackAssessment(float bpm, string context)
        {
            var (low, high) = BpmThresholds.Get(context);
            var riskLevel = bpm > high || bpm < low ? "moderate" : "low";
            var score = bpm > high || bpm < low ? 0.6f : 0.2f;

            return new RiskAssessmentDto
            {
                RiskLevel = riskLevel,
                Score = score,
                Recommendation = GetRecommendation(riskLevel, bpm, context)
            };
        }
    }
}