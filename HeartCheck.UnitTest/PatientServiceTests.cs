using FluentAssertions;
using Moq;
using Xunit;
using HeartCheck.Data;
using HeartCheck.DTOs.Patients;
using HeartCheck.Models;
using HeartCheck.Services;

namespace HeartCheck.UnitTest
{
    public class PatientServiceTests
    {
        private readonly Mock<IPatientRepository> _patientRepositoryMock;
        private readonly PatientService _patientService;

        public PatientServiceTests()
        {
            _patientRepositoryMock = new Mock<IPatientRepository>();
            _patientService = new PatientService(_patientRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateAsync_Success_ReturnsPatientResponse()
        {
            var userId = ObjectId.GenerateNewId();
            var request = new CreatePatientRequest
            {
                FirstName = "Jane",
                LastName = "Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Female",
                Weight = 70.5,
                Height = 165.0,
                BloodType = "O+",
                Phone = "1234567890",
                Address = "123 Main St",
                PhotoUrl = "http://example.com/photo.jpg",
                Observations = "No allergies",
                EmergencyContacts = new List<EmergencyContactDto>
                {
                    new EmergencyContactDto { Name = "Bob", Relationship = "Husband", Phone = "0987654321", IsPrimary = true },
                    new EmergencyContactDto { Name = "Alice", Relationship = "Sister", Phone = "1112223333", IsPrimary = false }
                }
            };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync((Patient?)null);

            var result = await _patientService.CreateAsync(userId, request);

            result.FirstName.Should().Be(request.FirstName);
            result.LastName.Should().Be(request.LastName);
            result.EmergencyContacts.Should().HaveCount(2);

            _patientRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
            _patientRepositoryMock.Verify(x => x.CreateAsync(It.Is<Patient>(p => p.UserId == userId)), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ExistingProfile_ThrowsInvalidOperationException()
        {
            var userId = ObjectId.GenerateNewId();
            var existingPatient = new Patient
            {
                UserId = userId,
                FirstName = "Jane",
                LastName = "Doe"
            };

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(existingPatient);

            var request = new CreatePatientRequest
            {
                FirstName = "New Jane",
                LastName = "Smith",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Female",
                Weight = 70.5,
                Height = 165.0,
                BloodType = "O+",
                Phone = "1234567890",
                Address = "123 Main St"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _patientService.CreateAsync(userId, request)
            );

            _patientRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
            _patientRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Patient>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ExceedsEmergencyContacts_ThrowsInvalidOperationException()
        {
            var userId = ObjectId.GenerateNewId();

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync((Patient?)null);

            var request = new CreatePatientRequest
            {
                FirstName = "Jane",
                LastName = "Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Female",
                Weight = 70.5,
                Height = 165.0,
                BloodType = "O+",
                Phone = "1234567890",
                Address = "123 Main St",
                EmergencyContacts = new List<EmergencyContactDto>
                {
                    new EmergencyContactDto { Name = "Contact1", Relationship = "R1", Phone = "111", IsPrimary = true },
                    new EmergencyContactDto { Name = "Contact2", Relationship = "R2", Phone = "222", IsPrimary = false },
                    new EmergencyContactDto { Name = "Contact3", Relationship = "R3", Phone = "333", IsPrimary = false },
                    new EmergencyContactDto { Name = "Contact4", Relationship = "R4", Phone = "444", IsPrimary = false },
                    new EmergencyContactDto { Name = "Contact5", Relationship = "R5", Phone = "555", IsPrimary = false }
                }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _patientService.CreateAsync(userId, request)
            );

            _patientRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
            _patientRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Patient>()), Times.Never);
        }
    }
}
