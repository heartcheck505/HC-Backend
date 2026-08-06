using System.Text.Json;

namespace HeartCheck.DTOs.Settings
{
    public class UpdateSettingRequest
    {
        public JsonElement Value { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
