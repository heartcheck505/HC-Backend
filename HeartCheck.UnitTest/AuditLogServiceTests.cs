using FluentAssertions;
using Moq;
using Xunit;
using HeartCheck.Data;
using HeartCheck.Models;
using HeartCheck.Services;
using MongoDB.Bson;

namespace HeartCheck.UnitTest
{
    public class AuditLogServiceTests
    {
        private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
        private readonly AuditLogService _auditLogService;

        public AuditLogServiceTests()
        {
            _auditLogRepositoryMock = new Mock<IAuditLogRepository>();

            _auditLogService = new AuditLogService(
                _auditLogRepositoryMock.Object
            );
        }

        private static AuditLog CreateAuditLog(ObjectId id, string action, string entity)
        {
            return new AuditLog
            {
                Id = id,
                UserId = ObjectId.GenerateNewId(),
                Action = action,
                Entity = entity,
                EntityId = ObjectId.GenerateNewId(),
                IpAddress = "127.0.0.1",
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task LogAsync_Success_CreatesLog()
        {
            var userId = ObjectId.GenerateNewId();
            var entityId = ObjectId.GenerateNewId();

            await _auditLogService.LogAsync(
                userId, "UPDATE", "setting", entityId,
                "127.0.0.1", "test-agent");

            _auditLogRepositoryMock.Verify(x => x.CreateAsync(It.Is<AuditLog>(l =>
                l.UserId == userId &&
                l.Action == "UPDATE" &&
                l.Entity == "setting" &&
                l.EntityId == entityId &&
                l.IpAddress == "127.0.0.1" &&
                l.UserAgent == "test-agent" &&
                l.PreviousValues == null &&
                l.NewValues == null &&
                l.CreatedAt != default
            )), Times.Once);
        }

        [Fact]
        public async Task LogAsync_WithValues_StoresBsonDocuments()
        {
            var userId = ObjectId.GenerateNewId();
            var entityId = ObjectId.GenerateNewId();
            var previous = new Dictionary<string, object> { ["status"] = "active" };
            var updated = new Dictionary<string, object> { ["status"] = "resolved" };

            await _auditLogService.LogAsync(
                userId, "UPDATE", "alert", entityId,
                "127.0.0.1", null, previous, updated);

            _auditLogRepositoryMock.Verify(x => x.CreateAsync(It.Is<AuditLog>(l =>
                l.PreviousValues != null &&
                l.PreviousValues["status"].AsString == "active" &&
                l.NewValues != null &&
                l.NewValues["status"].AsString == "resolved"
            )), Times.Once);
        }

        [Fact]
        public async Task LogAsync_InvalidAction_ThrowsInvalidOperationException()
        {
            var userId = ObjectId.GenerateNewId();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _auditLogService.LogAsync(
                    userId, "PUBLISH", "setting", ObjectId.GenerateNewId(), "127.0.0.1", null)
            );

            _auditLogRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<AuditLog>()), Times.Never);
        }

        [Fact]
        public async Task LogAsync_InvalidEntity_ThrowsInvalidOperationException()
        {
            var userId = ObjectId.GenerateNewId();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _auditLogService.LogAsync(
                    userId, "CREATE", "billing", ObjectId.GenerateNewId(), "127.0.0.1", null)
            );

            _auditLogRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<AuditLog>()), Times.Never);
        }

        [Fact]
        public async Task GetAuditLogsAsync_Success_ReturnsLogs()
        {
            var logs = new List<AuditLog>
            {
                CreateAuditLog(ObjectId.GenerateNewId(), "LOGIN", "user")
            };

            _auditLogRepositoryMock
                .Setup(x => x.GetFilteredAsync(It.IsAny<ObjectId?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<ObjectId?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(logs);

            var result = await _auditLogService.GetAuditLogsAsync(null, "LOGIN", null, null, null, null);

            result.Should().HaveCount(1);
            result[0].Action.Should().Be("LOGIN");
            result[0].Entity.Should().Be("user");
            result[0].IpAddress.Should().Be("127.0.0.1");
        }

        [Fact]
        public async Task GetAuditLogsAsync_InvalidActionFilter_ThrowsInvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _auditLogService.GetAuditLogsAsync(null, "PUBLISH", null, null, null, null)
            );

            _auditLogRepositoryMock.Verify(x => x.GetFilteredAsync(
                It.IsAny<ObjectId?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<ObjectId?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()), Times.Never);
        }

        [Fact]
        public async Task GetAuditLogByIdAsync_Success_ReturnsLog()
        {
            var logId = ObjectId.GenerateNewId();
            var log = CreateAuditLog(logId, "EXPORT", "report");

            _auditLogRepositoryMock
                .Setup(x => x.GetByIdAsync(logId))
                .ReturnsAsync(log);

            var result = await _auditLogService.GetAuditLogByIdAsync(logId);

            result.Id.Should().Be(logId.ToString());
            result.Action.Should().Be("EXPORT");
            result.Entity.Should().Be("report");
        }

        [Fact]
        public async Task GetAuditLogByIdAsync_NotFound_ThrowsKeyNotFoundException()
        {
            var logId = ObjectId.GenerateNewId();

            _auditLogRepositoryMock
                .Setup(x => x.GetByIdAsync(logId))
                .ReturnsAsync((AuditLog?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _auditLogService.GetAuditLogByIdAsync(logId)
            );
        }
    }
}
