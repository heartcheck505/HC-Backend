namespace HeartCheck.DTOs.Devices
{
    public class PairDeviceRequest
    {
        public string DeviceIdentifier { get; set; } = null!;
        public string DeviceModel { get; set; } = null!;
        public string FirmwareVersion { get; set; } = null!;
        public int BatteryLevel { get; set; }
    }
}
