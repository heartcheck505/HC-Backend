using FluentAssertions;
using Moq;
using Xunit;
using HeartCheck.Data;
using HeartCheck.Models;
using HeartCheck.Services;

namespace HeartCheck.UnitTest
{
    public class AlertServiceTests
    {
        private readonly Mock<IPatientRepository> _patientRepositoryMock;
        private readonly Mock<IAlertRepository> _alertRepositoryMock;
        private readonly Mock<IEventRepository> _eventRepositoryMock;
        private readonly AlertService _alertService;

        public AlertServiceTests()
        {
            _patientRepositoryMock = new Mock<IPatientRepository>();
            _alertRepositoryMock = new Mock<IAlertRepository>();
            _eventRepositoryMock = new Mock<IEventRepository>();

            _alertService = new AlertService(
                _patientRepositoryMock.Object,
                _alertRepositoryMock.Object,
                _eventRepositoryMock.Object
            );
        }

        [Fact]
        public async Task GetActiveAlertsByPatientAsync_Success_ReturnsAlerts()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var alertId = ObjectId.GenerateNewId();
            var alert = new Alert
            {
                Id = alertId,
                PatientId = patientId,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            var alerts = new List<Alert> { alert };

            _alertRepositoryMock
                .Setup(x => x.GetActiveByPatientIdAsync(patientId))
                .ReturnsAsync(alerts);

            var result = await _alertService.GetActiveAlertsByPatientAsync(userId);

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(alertId.ToString());
            result[0].Status.Should().Be("active");
        }

        [Fact]
        public async Task AcknowledgeAlertAsync_Success_UpdatesAlertAndCreatesEvent()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var alertId = ObjectId.GenerateNewId();
            var measurementId = ObjectId.GenerateNewId();
            var alert = new Alert
            {
                Id = alertId,
                PatientId = patientId,
                MeasurementId = measurementId,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            _alertRepositoryMock
                .Setup(x => x.GetByIdAsync(alertId))
                .ReturnsAsync(alert);

            _alertRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Alert>()))
                .Returns(Task.CompletedTask);

            _eventRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Event>()))
                .Returns(Task.CompletedTask);

            await _alertService.AcknowledgeAlertAsync(userId, alertId, "Feeling better");

            alert.Status.Should().Be("acknowledged");
            alert.AcknowledgedAt.Should().NotBeNull();

            _alertRepositoryMock.Verify(x => x.GetByIdAsync(alertId), Times.Once);
            _alertRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Alert>(a => a.Status == "acknowledged" && a.AcknowledgedAt.HasValue)), Times.Once);
            _eventRepositoryMock.Verify(x => x.CreateAsync(It.Is<Event>(e =>
                e.AlertId == alertId &&
                e.Type == "response_received" &&
                e.UserResponse == "Feeling better"
            )), Times.Once);
        }

        [Fact]
        public async Task ResolveAlertAsync_Success_UpdatesAlertStatus()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var alertId = ObjectId.GenerateNewId();
            var alert = new Alert
            {
                Id = alertId,
                PatientId = patientId,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            _alertRepositoryMock
                .Setup(x => x.GetByIdAsync(alertId))
                .ReturnsAsync(alert);

            _alertRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Alert>()))
                .Returns(Task.CompletedTask);

            await _alertService.ResolveAlertAsync(userId, alertId);

            alert.Status.Should().Be("resolved");
            alert.ResolvedAt.Should().NotBeNull();

            _alertRepositoryMock.Verify(x => x.GetByIdAsync(alertId), Times.Once);
            _alertRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Alert>(a => a.Status == "resolved" && a.ResolvedAt.HasValue)), Times.Once);
        }

        [Fact]
        public async Task ResolveAlertAsync_NonExistentAlert_ThrowsKeyNotFoundException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var alertId = ObjectId.GenerateNewId();
            _alertRepositoryMock
                .Setup(x => x.GetByIdAsync(alertId))
                .ReturnsAsync((Alert?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _alertService.ResolveAlertAsync(userId, alertId)
            );

            _alertRepositoryMock.Verify(x => x.GetByIdAsync(alertId), Times.Once);
        }
    }
}
