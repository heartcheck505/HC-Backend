using FluentAssertions;
using Moq;
using Xunit;
using HeartCheck.Data;
using HeartCheck.DTOs.Devices;
using HeartCheck.Models;
using HeartCheck.Services;

namespace HeartCheck.UnitTest
{
    public class DeviceServiceTests
    {
        private readonly Mock<IPatientRepository> _patientRepositoryMock;
        private readonly Mock<IDeviceRepository> _deviceRepositoryMock;
        private readonly DeviceService _deviceService;

        public DeviceServiceTests()
        {
            _patientRepositoryMock = new Mock<IPatientRepository>();
            _deviceRepositoryMock = new Mock<IDeviceRepository>();
            _deviceService = new DeviceService(_patientRepositoryMock.Object, _deviceRepositoryMock.Object);
        }

        [Fact]
        public async Task PairAsync_Success_ReturnsDeviceResponse()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId, FirstName = "Jane" };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var request = new PairDeviceRequest
            {
                DeviceIdentifier = "AA:BB:CC:DD:EE:FF",
                DeviceModel = "Samsung Galaxy Watch 6",
                FirmwareVersion = "1.2.3",
                BatteryLevel = 85
            };

            _deviceRepositoryMock
                .Setup(x => x.GetByIdentifierAsync(request.DeviceIdentifier))
                .ReturnsAsync((Device?)null);

            var deviceId = ObjectId.GenerateNewId();
            var device = new Device
            {
                Id = deviceId,
                PatientId = patientId,
                DeviceIdentifier = request.DeviceIdentifier,
                DeviceModel = request.DeviceModel,
                FirmwareVersion = request.FirmwareVersion,
                Status = "active"
            };

            _deviceRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            var result = await _deviceService.PairAsync(userId, request);

            result.PatientId.Should().Be(patientId.ToString());
            result.DeviceIdentifier.Should().Be(request.DeviceIdentifier);
            result.DeviceModel.Should().Be(request.DeviceModel);

            _deviceRepositoryMock.Verify(x => x.CreateAsync(It.Is<Device>(d => d.PatientId == patientId)), Times.Once);
        }

        [Fact]
        public async Task PairAsync_DuplicateDeviceIdentifier_ThrowsInvalidOperationException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var request = new PairDeviceRequest
            {
                DeviceIdentifier = "AA:BB:CC:DD:EE:FF"
            };

            var existingDevice = new Device { DeviceIdentifier = request.DeviceIdentifier };
            _deviceRepositoryMock
                .Setup(x => x.GetByIdentifierAsync(request.DeviceIdentifier))
                .ReturnsAsync(existingDevice);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _deviceService.PairAsync(userId, request)
            );

            _deviceRepositoryMock.Verify(x => x.GetByIdentifierAsync(request.DeviceIdentifier), Times.Once);
            _deviceRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Device>()), Times.Never);
        }

        [Fact]
        public async Task PairAsync_DeviceNotFoundForMeasurement_ThrowsInvalidOperationException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };
            var deviceId = ObjectId.GenerateNewId();

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var request = new PairDeviceRequest
            {
                DeviceIdentifier = "AA:BB:CC:DD:EE:FF",
                DeviceModel = "TestModel",
                FirmwareVersion = "1.0",
                BatteryLevel = 90
            };

            var device = new Device
            {
                Id = deviceId,
                PatientId = patientId,
                DeviceIdentifier = request.DeviceIdentifier,
                DeviceModel = request.DeviceModel,
                FirmwareVersion = request.FirmwareVersion,
                Status = "inactive"
            };

            _deviceRepositoryMock
                .Setup(x => x.GetByIdentifierAsync(request.DeviceIdentifier))
                .ReturnsAsync((Device?)null);

            var measurementRepoMock = new Mock<IMeasurementRepository>();
            var alertRepoMock = new Mock<IAlertRepository>();

            var measurementService = new MeasurementService(
                _patientRepositoryMock.Object,
                _deviceRepositoryMock.Object,
                measurementRepoMock.Object,
                alertRepoMock.Object
            );

            var deviceRequest = new HeartCheck.DTOs.Measurements.CreateMeasurementRequest
            {
                DeviceId = deviceId.ToString(),
                Bpm = 120,
                Quality = "good",
                Context = "rest",
                Notes = null
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => measurementService.CreateAsync(userId, deviceRequest)
            );
        }
    }
}
