namespace HeartCheck.DTOs.Measurements
{
    public class MLModelOutput
    {
        public string PredictedLabel { get; set; } = null!;
        public float[] Score { get; set; } = null!;
    }
}