using FluentAssertions;
using Moq;
using Xunit;
using HeartCheck.Data;
using HeartCheck.Models;
using HeartCheck.Services;

namespace HeartCheck.UnitTest
{
    public class EventServiceTests
    {
        private readonly Mock<IPatientRepository> _patientRepositoryMock;
        private readonly Mock<IAlertRepository> _alertRepositoryMock;
        private readonly Mock<IEventRepository> _eventRepositoryMock;
        private readonly EventService _eventService;

        public EventServiceTests()
        {
            _patientRepositoryMock = new Mock<IPatientRepository>();
            _alertRepositoryMock = new Mock<IAlertRepository>();
            _eventRepositoryMock = new Mock<IEventRepository>();

            _eventService = new EventService(
                _patientRepositoryMock.Object,
                _alertRepositoryMock.Object,
                _eventRepositoryMock.Object
            );
        }

        [Fact]
        public async Task GetByAlertIdAsync_Success_ReturnsEvents()
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
                Status = "active"
            };

            _alertRepositoryMock
                .Setup(x => x.GetByIdAsync(alertId))
                .ReturnsAsync(alert);

            var eventId = ObjectId.GenerateNewId();
            var evt = new Event
            {
                Id = eventId,
                AlertId = alertId,
                PatientId = patientId,
                Type = "response_received",
                Description = "Test event",
                CreatedAt = DateTime.UtcNow
            };

            var events = new List<Event> { evt };

            _eventRepositoryMock
                .Setup(x => x.GetByAlertIdAsync(alertId))
                .ReturnsAsync(events);

            var result = await _eventService.GetByAlertIdAsync(userId, alertId);

            result.Should().HaveCount(1);
            result[0].AlertId.Should().Be(alertId.ToString());
            result[0].Type.Should().Be("response_received");
        }

        [Fact]
        public async Task GetByAlertIdAsync_NonExistentAlert_ThrowsKeyNotFoundException()
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
                Status = "active"
            };

            _alertRepositoryMock
                .Setup(x => x.GetByIdAsync(alertId))
                .ReturnsAsync(alert);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _eventService.GetByAlertIdAsync(ObjectId.GenerateNewId(), alertId)
            );
        }

        [Fact]
        public async Task GetByAlertIdAsync_AlertNotAssociatedWithPatient_ThrowsKeyNotFoundException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var alertId = ObjectId.GenerateNewId();
            var otherPatientId = ObjectId.GenerateNewId();
            var alert = new Alert
            {
                Id = alertId,
                PatientId = otherPatientId,
                Status = "active"
            };

            _alertRepositoryMock
                .Setup(x => x.GetByIdAsync(alertId))
                .ReturnsAsync(alert);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _eventService.GetByAlertIdAsync(userId, alertId)
            );
        }
    }
}