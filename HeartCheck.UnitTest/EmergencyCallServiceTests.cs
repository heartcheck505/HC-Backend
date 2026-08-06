using FluentAssertions;
using Moq;
using Xunit;
using HeartCheck.Data;
using HeartCheck.Models;
using HeartCheck.Services;
using MongoDB.Bson;

namespace HeartCheck.UnitTest
{
    public class EmergencyCallServiceTests
    {
        private readonly Mock<IPatientRepository> _patientRepositoryMock;
        private readonly Mock<IAlertRepository> _alertRepositoryMock;
        private readonly Mock<IEmergencyCallRepository> _emergencyCallRepositoryMock;
        private readonly EmergencyCallService _emergencyCallService;

        public EmergencyCallServiceTests()
        {
            _patientRepositoryMock = new Mock<IPatientRepository>();
            _alertRepositoryMock = new Mock<IAlertRepository>();
            _emergencyCallRepositoryMock = new Mock<IEmergencyCallRepository>();

            _emergencyCallService = new EmergencyCallService(
                _patientRepositoryMock.Object,
                _alertRepositoryMock.Object,
                _emergencyCallRepositoryMock.Object
            );
        }

        [Fact]
        public async Task CreateEmergencyCallAsync_Success_ReturnsCreatedCall()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var alertId = ObjectId.GenerateNewId();
            var contactId = ObjectId.GenerateNewId();

            var patient = new Patient
            {
                Id = patientId,
                UserId = userId,
                EmergencyContacts = new List<EmergencyContact>
                {
                    new()
                    {
                        Id = contactId,
                        Name = "María López",
                        Relationship = "Esposa",
                        Phone = "+5215512345678"
                    }
                }
            };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

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

            var request = new CreateEmergencyCallRequest
            {
                AlertId = alertId.ToString(),
                EmergencyContactId = contactId.ToString()
            };

            var result = await _emergencyCallService.CreateEmergencyCallAsync(userId, request);

            result.AlertId.Should().Be(alertId.ToString());
            result.PatientId.Should().Be(patientId.ToString());
            result.EmergencyContactId.Should().Be(contactId.ToString());
            result.ContactName.Should().Be("María López");
            result.PhoneNumber.Should().Be("+5215512345678");
            result.Status.Should().Be("initiated");
            result.InitiatedAt.Should().NotBeNull();

            _emergencyCallRepositoryMock.Verify(x => x.CreateAsync(It.Is<EmergencyCall>(c =>
                c.AlertId == alertId &&
                c.PatientId == patientId &&
                c.EmergencyContactId == contactId &&
                c.ContactName == "María López" &&
                c.Status == "initiated"
            )), Times.Once);
        }

        [Fact]
        public async Task CreateEmergencyCallAsync_PatientNotFound_ThrowsKeyNotFoundException()
        {
            var userId = ObjectId.GenerateNewId();

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync((Patient?)null);

            var request = new CreateEmergencyCallRequest
            {
                AlertId = ObjectId.GenerateNewId().ToString(),
                EmergencyContactId = ObjectId.GenerateNewId().ToString()
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _emergencyCallService.CreateEmergencyCallAsync(userId, request)
            );
        }

        [Fact]
        public async Task CreateEmergencyCallAsync_AlertNotOwned_ThrowsKeyNotFoundException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var alertId = ObjectId.GenerateNewId();
            var otherPatientId = ObjectId.GenerateNewId();

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(new Patient { Id = patientId, UserId = userId });

            _alertRepositoryMock
                .Setup(x => x.GetByIdAsync(alertId))
                .ReturnsAsync(new Alert
                {
                    Id = alertId,
                    PatientId = otherPatientId,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                });

            var request = new CreateEmergencyCallRequest
            {
                AlertId = alertId.ToString(),
                EmergencyContactId = ObjectId.GenerateNewId().ToString()
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _emergencyCallService.CreateEmergencyCallAsync(userId, request)
            );
        }

        [Fact]
        public async Task CreateEmergencyCallAsync_ContactNotFound_ThrowsKeyNotFoundException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var alertId = ObjectId.GenerateNewId();
            var contactId = ObjectId.GenerateNewId();

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(new Patient
                {
                    Id = patientId,
                    UserId = userId,
                    EmergencyContacts = new List<EmergencyContact>
                    {
                        new() { Id = ObjectId.GenerateNewId(), Name = "Otro", Phone = "123" }
                    }
                });

            _alertRepositoryMock
                .Setup(x => x.GetByIdAsync(alertId))
                .ReturnsAsync(new Alert
                {
                    Id = alertId,
                    PatientId = patientId,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                });

            var request = new CreateEmergencyCallRequest
            {
                AlertId = alertId.ToString(),
                EmergencyContactId = contactId.ToString()
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _emergencyCallService.CreateEmergencyCallAsync(userId, request)
            );
        }

        [Fact]
        public async Task GetUserCallsAsync_Success_ReturnsCalls()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var callId = ObjectId.GenerateNewId();
            var calls = new List<EmergencyCall>
            {
                new()
                {
                    Id = callId,
                    PatientId = patientId,
                    Status = "initiated",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _emergencyCallRepositoryMock
                .Setup(x => x.GetByPatientIdAsync(patientId))
                .ReturnsAsync(calls);

            var result = await _emergencyCallService.GetUserCallsAsync(userId);

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(callId.ToString());
            result[0].Status.Should().Be("initiated");
        }

        [Fact]
        public async Task GetCallByIdAsync_Success_ReturnsCall()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var callId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var call = new EmergencyCall
            {
                Id = callId,
                PatientId = patientId,
                Status = "answered",
                CreatedAt = DateTime.UtcNow
            };

            _emergencyCallRepositoryMock
                .Setup(x => x.GetByIdAsync(callId))
                .ReturnsAsync(call);

            var result = await _emergencyCallService.GetCallByIdAsync(userId, callId);

            result.Id.Should().Be(callId.ToString());
            result.Status.Should().Be("answered");
        }

        [Fact]
        public async Task GetCallByIdAsync_NotOwned_ThrowsKeyNotFoundException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var otherPatientId = ObjectId.GenerateNewId();
            var callId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var call = new EmergencyCall
            {
                Id = callId,
                PatientId = otherPatientId,
                Status = "initiated",
                CreatedAt = DateTime.UtcNow
            };

            _emergencyCallRepositoryMock
                .Setup(x => x.GetByIdAsync(callId))
                .ReturnsAsync(call);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _emergencyCallService.GetCallByIdAsync(userId, callId)
            );
        }

        [Fact]
        public async Task UpdateCallStatusAsync_Success_UpdatesStatusAndTimestamps()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var callId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var call = new EmergencyCall
            {
                Id = callId,
                PatientId = patientId,
                Status = "initiated",
                InitiatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _emergencyCallRepositoryMock
                .Setup(x => x.GetByIdAsync(callId))
                .ReturnsAsync(call);

            _emergencyCallRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<EmergencyCall>()))
                .Returns(Task.CompletedTask);

            var request = new UpdateEmergencyCallStatusRequest
            {
                Status = "completed",
                Duration = 45,
                Result = "Contactado correctamente"
            };

            await _emergencyCallService.UpdateCallStatusAsync(userId, callId, request);

            call.Status.Should().Be("completed");
            call.Duration.Should().Be(45);
            call.Result.Should().Be("Contactado correctamente");
            call.CompletedAt.Should().NotBeNull();

            _emergencyCallRepositoryMock.Verify(x => x.GetByIdAsync(callId), Times.Once);
            _emergencyCallRepositoryMock.Verify(x => x.UpdateAsync(It.Is<EmergencyCall>(c =>
                c.Status == "completed" &&
                c.Duration == 45 &&
                c.CompletedAt.HasValue
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateCallStatusAsync_InvalidStatus_ThrowsInvalidOperationException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var callId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var request = new UpdateEmergencyCallStatusRequest { Status = "invalid_status" };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _emergencyCallService.UpdateCallStatusAsync(userId, callId, request)
            );

            _emergencyCallRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ObjectId>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCallStatusAsync_CallNotFound_ThrowsKeyNotFoundException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var callId = ObjectId.GenerateNewId();
            var patient = new Patient { Id = patientId, UserId = userId };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            _emergencyCallRepositoryMock
                .Setup(x => x.GetByIdAsync(callId))
                .ReturnsAsync((EmergencyCall?)null);

            var request = new UpdateEmergencyCallStatusRequest { Status = "completed" };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _emergencyCallService.UpdateCallStatusAsync(userId, callId, request)
            );
        }
    }
}
