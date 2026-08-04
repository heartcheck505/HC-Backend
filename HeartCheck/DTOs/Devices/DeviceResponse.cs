namespace HeartCheck.DTOs.Devices
{
    public class DeviceResponse
    {
        public string Id { get; set; } = null!;
        public string PatientId { get; set; } = null!;
        public string DeviceIdentifier { get; set; } = null!;
        public string DeviceModel { get; set; } = null!;
        public string FirmwareVersion { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime LastSync { get; set; }
        public int BatteryLevel { get; set; }
        public DateTime PairedAt { get; set; }
        public DateTime? UnpairedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
