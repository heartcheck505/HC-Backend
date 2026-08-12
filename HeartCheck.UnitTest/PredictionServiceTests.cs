using HeartCheck.Services;

namespace HeartCheck.UnitTest
{
    public class PredictionServiceTests
    {
        [Fact]
        public void PredictRisk_WithModel_ReturnsRiskAssessment()
        {
            var modelPath = Path.Combine(AppContext.BaseDirectory, "HeartCheckML.zip");
            var service = new PredictionService(modelPath);

            var result = service.PredictRisk(72, "rest", 30, false);

            result.Should().NotBeNull();
            result.RiskLevel.Should().BeOneOf("low", "moderate", "critical");
            result.Score.Should().BeGreaterThanOrEqualTo(0);
            result.Recommendation.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void PredictRisk_ModelNotAvailable_FallsBackToThresholdAssessment()
        {
            var service = new PredictionService((string?)null);

            var normalResult = service.PredictRisk(80, "rest", 30, false);
            normalResult.RiskLevel.Should().Be("low");
            normalResult.Score.Should().Be(0.2f);

            var abnormalResult = service.PredictRisk(150, "rest", 30, false);
            abnormalResult.RiskLevel.Should().Be("moderate");
            abnormalResult.Score.Should().Be(0.6f);
        }

        [Fact]
        public void PredictRisk_ModelNotAvailable_UnknownContext_DefaultsToRestThresholds()
        {
            var service = new PredictionService((string?)null);

            var result = service.PredictRisk(50, "unknown", 30, false);

            result.RiskLevel.Should().Be("moderate");
            result.Score.Should().Be(0.6f);
        }

        [Theory]
        [InlineData("rest", "rest")]
        [InlineData("sleep", "sleep")]
        [InlineData("active", "exercise")]
        [InlineData("exercise", "exercise")]
        [InlineData("REST", "rest")]
        [InlineData("Exercise", "exercise")]
        [InlineData("unknown", "rest")]
        public void NormalizeContext_ReturnsExpectedContext(string input, string expected)
        {
            PredictionService.NormalizeContext(input).Should().Be(expected);
        }

        [Theory]
        [InlineData("critical", "critical")]
        [InlineData("moderate", "moderate")]
        [InlineData("low", "low")]
        [InlineData("CRITICAL", "critical")]
        [InlineData("Medium", "low")]
        [InlineData("", "low")]
        [InlineData(null, "low")]
        public void NormalizeLabel_ReturnsExpectedLabel(string? input, string expected)
        {
            PredictionService.NormalizeLabel(input).Should().Be(expected);
        }
    }
}
