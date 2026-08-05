namespace HeartCheck.DTOs.Statistics
{
    public class DailyStatisticResponse
    {
        public string Id { get; set; } = null!;
        public string PatientId { get; set; } = null!;
        public DateTime Date { get; set; }
        public double AverageBpm { get; set; }
        public int MinBpm { get; set; }
        public int MaxBpm { get; set; }
        public int TotalMeasurements { get; set; }
        public int NormalMeasurements { get; set; }
        public int AbnormalMeasurements { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}