using System.Text.Json;
using FluentAssertions;
using Moq;
using Xunit;
using HeartCheck.Data;
using HeartCheck.DTOs.Settings;
using HeartCheck.Models;
using HeartCheck.Services;
using MongoDB.Bson;

namespace HeartCheck.UnitTest
{
    public class SettingServiceTests
    {
        private readonly Mock<ISettingRepository> _settingRepositoryMock;
        private readonly SettingService _settingService;

        public SettingServiceTests()
        {
            _settingRepositoryMock = new Mock<ISettingRepository>();

            _settingService = new SettingService(
                _settingRepositoryMock.Object
            );
        }

        private static JsonElement Json(object value)
        {
            return JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(value)).RootElement;
        }

        private static Setting CreateSetting(ObjectId id, string key, string category, int intValue)
        {
            return new Setting
            {
                Id = id,
                Key = key,
                Value = new BsonInt32(intValue),
                Category = category,
                UpdatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task CreateSettingAsync_Success_ReturnsCreatedSetting()
        {
            var userId = ObjectId.GenerateNewId();
            var request = new CreateSettingRequest
            {
                Key = "sync_interval_seconds",
                Value = Json(5),
                Category = "system",
                Description = "Intervalo de sincronización"
            };

            var result = await _settingService.CreateSettingAsync(userId, request);

            result.Key.Should().Be("sync_interval_seconds");
            result.Value.Should().Be(5);
            result.Category.Should().Be("system");
            result.Description.Should().Be("Intervalo de sincronización");
            result.UpdatedBy.Should().Be(userId.ToString());

            _settingRepositoryMock.Verify(x => x.CreateAsync(It.Is<Setting>(s =>
                s.Key == "sync_interval_seconds" &&
                s.Value.IsInt32 &&
                s.Value.AsInt32 == 5 &&
                s.Category == "system" &&
                s.UpdatedBy == userId &&
                s.UpdatedAt != default
            )), Times.Once);
        }

        [Fact]
        public async Task CreateSettingAsync_DuplicateKey_ThrowsInvalidOperationException()
        {
            var userId = ObjectId.GenerateNewId();

            _settingRepositoryMock
                .Setup(x => x.GetByKeyAsync("sync_interval_seconds"))
                .ReturnsAsync(CreateSetting(ObjectId.GenerateNewId(), "sync_interval_seconds", "system", 5));

            var request = new CreateSettingRequest
            {
                Key = "sync_interval_seconds",
                Value = Json(5),
                Category = "system"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _settingService.CreateSettingAsync(userId, request)
            );

            _settingRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Setting>()), Times.Never);
        }

        [Fact]
        public async Task CreateSettingAsync_InvalidCategory_ThrowsInvalidOperationException()
        {
            var userId = ObjectId.GenerateNewId();

            var request = new CreateSettingRequest
            {
                Key = "custom_setting",
                Value = Json("value"),
                Category = "unknown"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _settingService.CreateSettingAsync(userId, request)
            );

            _settingRepositoryMock.Verify(x => x.GetByKeyAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateSettingAsync_EmptyKey_ThrowsInvalidOperationException()
        {
            var userId = ObjectId.GenerateNewId();

            var request = new CreateSettingRequest
            {
                Key = " ",
                Value = Json(true),
                Category = "general"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _settingService.CreateSettingAsync(userId, request)
            );
        }

        [Fact]
        public async Task CreateSettingAsync_NullValue_ThrowsInvalidOperationException()
        {
            var userId = ObjectId.GenerateNewId();

            var request = new CreateSettingRequest
            {
                Key = "custom_setting",
                Value = JsonDocument.Parse("null").RootElement,
                Category = "general"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _settingService.CreateSettingAsync(userId, request)
            );
        }

        [Fact]
        public async Task GetSettingsAsync_All_ReturnsAll()
        {
            var settings = new List<Setting>
            {
                CreateSetting(ObjectId.GenerateNewId(), "bpm_threshold_high", "alerts", 100),
                CreateSetting(ObjectId.GenerateNewId(), "sync_interval_seconds", "system", 5)
            };

            _settingRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(settings);

            var result = await _settingService.GetSettingsAsync(null);

            result.Should().HaveCount(2);
            result[0].Key.Should().Be("bpm_threshold_high");
            result[0].Value.Should().Be(100);
        }

        [Fact]
        public async Task GetSettingsAsync_ByCategory_ReturnsFiltered()
        {
            var settings = new List<Setting>
            {
                CreateSetting(ObjectId.GenerateNewId(), "bpm_threshold_high", "alerts", 100)
            };

            _settingRepositoryMock
                .Setup(x => x.GetByCategoryAsync("alerts"))
                .ReturnsAsync(settings);

            var result = await _settingService.GetSettingsAsync("alerts");

            result.Should().HaveCount(1);
            result[0].Category.Should().Be("alerts");

            _settingRepositoryMock.Verify(x => x.GetByCategoryAsync("alerts"), Times.Once);
        }

        [Fact]
        public async Task GetSettingsAsync_InvalidCategory_ThrowsInvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _settingService.GetSettingsAsync("unknown")
            );
        }

        [Fact]
        public async Task GetSettingByKeyAsync_Success_ReturnsSetting()
        {
            var setting = CreateSetting(ObjectId.GenerateNewId(), "bpm_threshold_high", "alerts", 100);

            _settingRepositoryMock
                .Setup(x => x.GetByKeyAsync("bpm_threshold_high"))
                .ReturnsAsync(setting);

            var result = await _settingService.GetSettingByKeyAsync("bpm_threshold_high");

            result.Key.Should().Be("bpm_threshold_high");
            result.Value.Should().Be(100);
        }

        [Fact]
        public async Task GetSettingByKeyAsync_NotFound_ThrowsKeyNotFoundException()
        {
            _settingRepositoryMock
                .Setup(x => x.GetByKeyAsync("missing_key"))
                .ReturnsAsync((Setting?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _settingService.GetSettingByKeyAsync("missing_key")
            );
        }

        [Fact]
        public async Task UpdateSettingAsync_Success_ReturnsUpdatedSetting()
        {
            var userId = ObjectId.GenerateNewId();
            var setting = CreateSetting(ObjectId.GenerateNewId(), "bpm_threshold_high", "alerts", 100);

            _settingRepositoryMock
                .Setup(x => x.GetByKeyAsync("bpm_threshold_high"))
                .ReturnsAsync(setting);

            var request = new UpdateSettingRequest
            {
                Value = Json(110),
                Category = "alerts",
                Description = "Nuevo umbral"
            };

            var result = await _settingService.UpdateSettingAsync(userId, "bpm_threshold_high", request);

            result.Value.Should().Be(110);
            result.Description.Should().Be("Nuevo umbral");
            result.UpdatedBy.Should().Be(userId.ToString());

            _settingRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Setting>(s =>
                s.Key == "bpm_threshold_high" &&
                s.Value.AsInt32 == 110 &&
                s.Category == "alerts" &&
                s.UpdatedBy == userId &&
                s.UpdatedAt != default
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateSettingAsync_NotFound_ThrowsKeyNotFoundException()
        {
            var userId = ObjectId.GenerateNewId();

            _settingRepositoryMock
                .Setup(x => x.GetByKeyAsync("missing_key"))
                .ReturnsAsync((Setting?)null);

            var request = new UpdateSettingRequest
            {
                Value = Json(110),
                Category = "alerts"
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _settingService.UpdateSettingAsync(userId, "missing_key", request)
            );
        }
    }
}
