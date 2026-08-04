using FluentAssertions;
using Moq;
using Xunit;
using HeartCheck.Data;
using HeartCheck.DTOs.Measurements;
using HeartCheck.Models;
using HeartCheck.Services;

namespace HeartCheck.UnitTest
{
    public class MeasurementServiceTests
    {
        private readonly Mock<IPatientRepository> _patientRepositoryMock;
        private readonly Mock<IDeviceRepository> _deviceRepositoryMock;
        private readonly Mock<IMeasurementRepository> _measurementRepositoryMock;
        private readonly Mock<IAlertRepository> _alertRepositoryMock;
        private readonly MeasurementService _measurementService;

        public MeasurementServiceTests()
        {
            _patientRepositoryMock = new Mock<IPatientRepository>();
            _deviceRepositoryMock = new Mock<IDeviceRepository>();
            _measurementRepositoryMock = new Mock<IMeasurementRepository>();
            _alertRepositoryMock = new Mock<IAlertRepository>();

            _measurementService = new MeasurementService(
                _patientRepositoryMock.Object,
                _deviceRepositoryMock.Object,
                _measurementRepositoryMock.Object,
                _alertRepositoryMock.Object
            );
        }

        [Fact]
        public async Task CreateAsync_NormalRest_ReturnsIsNormalTrue()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var deviceId = ObjectId.GenerateNewId();
            var device = new Device
            {
                Id = deviceId,
                PatientId = patientId,
                DeviceIdentifier = "AA:BB:CC:DD:EE:FF",
                Status = "active"
            };

            _deviceRepositoryMock
                .Setup(x => x.GetByIdAsync(deviceId))
                .ReturnsAsync(device);

            var request = new CreateMeasurementRequest
            {
                DeviceId = deviceId.ToString(),
                Bpm = 80,
                Quality = "good",
                Context = "rest"
            };

            var measurementId = ObjectId.GenerateNewId();
            var measurement = new HeartRateMeasurement
            {
                Id = measurementId,
                Bpm = request.Bpm,
                Quality = request.Quality,
                Context = request.Context,
                IsNormal = true
            };

            _measurementRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<HeartRateMeasurement>()))
                .Returns(Task.CompletedTask);

            var result = await _measurementService.CreateAsync(userId, request);

            result.Bpm.Should().Be(80);
            result.IsNormal.Should().BeTrue();

            _alertRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Alert>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_AbnormallyHighInRest_ReturnsIsNormalFalse_AndCreatesAlert()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var deviceId = ObjectId.GenerateNewId();
            var device = new Device
            {
                Id = deviceId,
                PatientId = patientId,
                DeviceIdentifier = "AA:BB:CC:DD:EE:FF",
                Status = "active"
            };

            _deviceRepositoryMock
                .Setup(x => x.GetByIdAsync(deviceId))
                .ReturnsAsync(device);

            var request = new CreateMeasurementRequest
            {
                DeviceId = deviceId.ToString(),
                Bpm = 120,
                Quality = "good",
                Context = "rest"
            };

            var measurementId = ObjectId.GenerateNewId();
            var measurement = new HeartRateMeasurement
            {
                Id = measurementId,
                Bpm = request.Bpm,
                Quality = request.Quality,
                Context = request.Context,
                IsNormal = false
            };

            _measurementRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<HeartRateMeasurement>()))
                .Callback<HeartRateMeasurement>(m => m.Id = ObjectId.GenerateNewId())
                .Returns(Task.CompletedTask);

            var result = await _measurementService.CreateAsync(userId, request);

            result.Bpm.Should().Be(120);
            result.IsNormal.Should().BeFalse();

            _alertRepositoryMock.Verify(x => x.CreateAsync(It.Is<Alert>(a =>
                a.BpmValue == 120 &&
                a.Type == "high_bpm" &&
                a.Threshold == 100 &&
                a.Severity == "medium"
            )), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_DifferentContexts_ReturnsCorrectIsNormal()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var deviceId = ObjectId.GenerateNewId();
            var device = new Device
            {
                Id = deviceId,
                PatientId = patientId,
                DeviceIdentifier = "AA:BB:CC:DD:EE:FF",
                Status = "active"
            };

            _deviceRepositoryMock
                .Setup(x => x.GetByIdAsync(deviceId))
                .ReturnsAsync(device);

            var normalTestCases = new List<(int bpm, string context)>
            {
                (60, "rest"),
                (100, "rest"),
                (80, "active"),
                (40, "sleep")
            };

            foreach (var (bpm, context) in normalTestCases)
            {
                var request = new CreateMeasurementRequest
                {
                    DeviceId = deviceId.ToString(),
                    Bpm = bpm,
                    Quality = "good",
                    Context = context
                };

                _measurementRepositoryMock.Setup(x => x.AddAsync(It.IsAny<HeartRateMeasurement>())).Returns(Task.CompletedTask);

                var result = await _measurementService.CreateAsync(userId, request);

                result.Bpm.Should().Be(bpm);
                result.IsNormal.Should().BeTrue();

                _alertRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Alert>()), Times.Never);
            }
        }

        [Fact]
        public async Task CreateAsync_AbnormalBpm_CreatesAlert()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var deviceId = ObjectId.GenerateNewId();
            var device = new Device
            {
                Id = deviceId,
                PatientId = patientId,
                DeviceIdentifier = "AA:BB:CC:DD:EE:FF",
                Status = "active"
            };

            _deviceRepositoryMock
                .Setup(x => x.GetByIdAsync(deviceId))
                .ReturnsAsync(device);

            var abnormalTestCases = new List<(int bpm, string context, string expectedType)>
            {
                (101, "rest", "high_bpm"),
                (161, "active", "high_bpm"),
                (81, "sleep", "high_bpm"),
                (50, "rest", "low_bpm"),
                (70, "active", "low_bpm"),
                (30, "sleep", "low_bpm")
            };

            foreach (var (bpm, context, expectedType) in abnormalTestCases)
            {
                _measurementRepositoryMock.Setup(x => x.AddAsync(It.IsAny<HeartRateMeasurement>())).Returns(Task.CompletedTask);

                var request = new CreateMeasurementRequest
                {
                    DeviceId = deviceId.ToString(),
                    Bpm = bpm,
                    Quality = "good",
                    Context = context
                };

                var result = await _measurementService.CreateAsync(userId, request);

                result.Bpm.Should().Be(bpm);
                result.IsNormal.Should().BeFalse();

                _alertRepositoryMock.Verify(x => x.CreateAsync(It.Is<Alert>(a =>
                    a.BpmValue == bpm &&
                    a.Type == expectedType
                )), Times.Once);

                _alertRepositoryMock.Reset();
            }
        }
    }
}
