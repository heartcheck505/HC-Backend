using System.Text.Json;

namespace HeartCheck.DTOs.Settings
{
    public class CreateSettingRequest
    {
        public string Key { get; set; } = string.Empty;
        public JsonElement Value { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
