using System.Text.Json;
using HeartCheck.Data;
using HeartCheck.DTOs.Settings;
using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace HeartCheck.Services
{
    public class SettingService : ISettingService
    {
        internal static readonly HashSet<string> ValidCategories = new()
        {
            "alerts", "notifications", "system", "security", "general"
        };

        private readonly ISettingRepository _settingRepository;

        public SettingService(ISettingRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public async Task<List<SettingResponse>> GetSettingsAsync(string? category)
        {
            if (!string.IsNullOrWhiteSpace(category) && !ValidCategories.Contains(category))
            {
                throw new InvalidOperationException($"Invalid setting category: {category}");
            }

            var settings = string.IsNullOrWhiteSpace(category)
                ? await _settingRepository.GetAllAsync()
                : await _settingRepository.GetByCategoryAsync(category);

            return settings.Select(MapToResponse).ToList();
        }

        public async Task<SettingResponse> GetSettingByKeyAsync(string key)
        {
            var setting = await _settingRepository.GetByKeyAsync(key);
            if (setting == null)
            {
                throw new KeyNotFoundException($"Setting not found: {key}");
            }

            return MapToResponse(setting);
        }

        public async Task<SettingResponse> CreateSettingAsync(ObjectId userId, CreateSettingRequest request)
        {
            ValidateRequest(request.Key, request.Category, request.Value);

            var existing = await _settingRepository.GetByKeyAsync(request.Key);
            if (existing != null)
            {
                throw new InvalidOperationException($"Setting already exists: {request.Key}");
            }

            var setting = new Setting
            {
                Key = request.Key,
                Value = ToBsonValue(request.Value),
                Description = request.Description,
                Category = request.Category,
                UpdatedBy = userId,
                UpdatedAt = DateTime.UtcNow
            };

            await _settingRepository.CreateAsync(setting);
            return MapToResponse(setting);
        }

        public async Task<SettingResponse> UpdateSettingAsync(ObjectId userId, string key, UpdateSettingRequest request)
        {
            ValidateRequest(key, request.Category, request.Value);

            var setting = await _settingRepository.GetByKeyAsync(key);
            if (setting == null)
            {
                throw new KeyNotFoundException($"Setting not found: {key}");
            }

            setting.Value = ToBsonValue(request.Value);
            setting.Description = request.Description;
            setting.Category = request.Category;
            setting.UpdatedBy = userId;
            setting.UpdatedAt = DateTime.UtcNow;

            await _settingRepository.UpdateAsync(setting);
            return MapToResponse(setting);
        }

        private static void ValidateRequest(string key, string category, JsonElement value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("Key is required");
            }

            if (!ValidCategories.Contains(category))
            {
                throw new InvalidOperationException($"Invalid setting category: {category}");
            }

            if (value.ValueKind == JsonValueKind.Undefined || value.ValueKind == JsonValueKind.Null)
            {
                throw new InvalidOperationException("Value is required");
            }
        }

        private static BsonValue ToBsonValue(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => new BsonString(value.GetString()),
                JsonValueKind.Number => value.TryGetInt32(out var i)
                    ? new BsonInt32(i)
                    : new BsonDouble(value.GetDouble()),
                JsonValueKind.True => new BsonBoolean(true),
                JsonValueKind.False => new BsonBoolean(false),
                JsonValueKind.Object or JsonValueKind.Array =>
                    BsonSerializer.Deserialize<BsonValue>(value.GetRawText()),
                _ => throw new InvalidOperationException("Unsupported setting value type")
            };
        }

        private static object? FromBsonValue(BsonValue value)
        {
            if (value == null || value.IsBsonNull)
            {
                return null;
            }

            return value.BsonType switch
            {
                BsonType.Int32 => value.AsInt32,
                BsonType.Int64 => value.AsInt64,
                BsonType.Double => value.AsDouble,
                BsonType.Decimal128 => Decimal128.ToDecimal(value.AsDecimal128),
                BsonType.String => value.AsString,
                BsonType.Boolean => value.AsBoolean,
                BsonType.ObjectId => value.AsObjectId.ToString(),
                BsonType.DateTime => value.ToUniversalTime(),
                BsonType.Array => value.AsBsonArray.Select(FromBsonValue).ToList(),
                BsonType.Document => value.AsBsonDocument.ToDictionary(
                    element => element.Name,
                    element => FromBsonValue(element.Value)),
                _ => value.ToString()
            };
        }

        private static SettingResponse MapToResponse(Setting setting)
        {
            return new SettingResponse
            {
                Id = setting.Id.ToString(),
                Key = setting.Key,
                Value = FromBsonValue(setting.Value),
                Category = setting.Category,
                Description = setting.Description,
                UpdatedBy = setting.UpdatedBy?.ToString(),
                UpdatedAt = setting.UpdatedAt
            };
        }
    }
}
