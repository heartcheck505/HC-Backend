namespace HeartCheck.DTOs.Plans
{
    public class PlanResponse
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int MaxDevices { get; set; }
        public int MeasurementIntervalMinutes { get; set; }
        public bool IncludesEmergencyCalls { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
