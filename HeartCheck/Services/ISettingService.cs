using HeartCheck.DTOs.Settings;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public interface ISettingService
    {
        Task<List<SettingResponse>> GetSettingsAsync(string? category);
        Task<SettingResponse> GetSettingByKeyAsync(string key);
        Task<SettingResponse> CreateSettingAsync(ObjectId userId, CreateSettingRequest request);
        Task<SettingResponse> UpdateSettingAsync(ObjectId userId, string key, UpdateSettingRequest request);
    }
}
