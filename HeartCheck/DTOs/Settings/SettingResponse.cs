namespace HeartCheck.DTOs.Settings
{
    public class SettingResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public object? Value { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
