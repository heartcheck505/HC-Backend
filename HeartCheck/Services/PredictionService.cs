using HeartCheck.DTOs.Measurements;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.ML;

namespace HeartCheck.Services
{
    public class PredictionService : IPredictionService
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer? _model;
        private readonly ILogger<PredictionService> _logger;

        public PredictionService(IHostEnvironment environment, ILogger<PredictionService> logger)
            : this(ResolveModelPath(environment), logger)
        {
        }

        internal PredictionService(string? modelPath, ILogger<PredictionService>? logger = null)
        {
            _mlContext = new MLContext();
            _logger = logger ?? NullLogger<PredictionService>.Instance;

            if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
            {
                _logger.LogWarning(
                    "Modelo ML 'HeartCheckML.zip' no encontrado en la ruta '{ModelPath}'. " +
                    "Se activa el modo fallback por umbrales.",
                    modelPath);
                return;
            }

            try
            {
                _model = _mlContext.Model.Load(modelPath, out _);
                _logger.LogInformation("Modelo ML cargado correctamente desde '{ModelPath}'.", modelPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "No se pudo cargar el modelo ML desde '{ModelPath}'. Se activa el modo fallback por umbrales.",
                    modelPath);
            }
        }

        private static string? ResolveModelPath(IHostEnvironment environment)
        {
            var modelPath = Path.Combine(environment.ContentRootPath, "HeartCheckML.zip");
            if (!File.Exists(modelPath))
            {
                modelPath = Path.Combine(AppContext.BaseDirectory, "HeartCheckML.zip");
            }

            return modelPath;
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

        internal static string NormalizeContext(string context)
        {
            return context.ToLowerInvariant() switch
            {
                "rest" or "sleep" => context.ToLowerInvariant(),
                "active" or "exercise" => "exercise",
                _ => "rest"
            };
        }

        internal static string NormalizeLabel(string? label)
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