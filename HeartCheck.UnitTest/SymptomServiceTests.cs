using FluentAssertions;
using Moq;
using Xunit;
using HeartCheck.Data;
using HeartCheck.Models;
using HeartCheck.Services;
using MongoDB.Bson;

namespace HeartCheck.UnitTest
{
    public class SymptomServiceTests
    {
        private readonly Mock<IPatientRepository> _patientRepositoryMock;
        private readonly Mock<IMeasurementRepository> _measurementRepositoryMock;
        private readonly Mock<ISymptomRepository> _symptomRepositoryMock;
        private readonly SymptomService _symptomService;

        public SymptomServiceTests()
        {
            _patientRepositoryMock = new Mock<IPatientRepository>();
            _measurementRepositoryMock = new Mock<IMeasurementRepository>();
            _symptomRepositoryMock = new Mock<ISymptomRepository>();

            _symptomService = new SymptomService(
                _patientRepositoryMock.Object,
                _measurementRepositoryMock.Object,
                _symptomRepositoryMock.Object
            );
        }

        private static Patient CreatePatient(ObjectId userId, ObjectId patientId)
        {
            return new Patient { Id = patientId, UserId = userId };
        }

        private static HeartRateMeasurement CreateMeasurement(ObjectId measurementId, ObjectId patientId)
        {
            return new HeartRateMeasurement
            {
                Id = measurementId,
                Timestamp = DateTime.UtcNow,
                Metadata = new MeasurementMetadata { PatientId = patientId },
                Bpm = 120,
                Quality = "good",
                Context = "rest",
                IsNormal = false
            };
        }

        [Fact]
        public async Task CreateSymptomAsync_Success_ReturnsCreatedSymptom()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var measurementId = ObjectId.GenerateNewId();

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(CreatePatient(userId, patientId));

            _measurementRepositoryMock
                .Setup(x => x.GetByIdAsync(measurementId))
                .ReturnsAsync(CreateMeasurement(measurementId, patientId));

            var request = new CreateSymptomRequest
            {
                MeasurementId = measurementId.ToString(),
                Type = "tachycardia",
                Confidence = 85.5,
                Description = "Frecuencia elevada en reposo"
            };

            var result = await _symptomService.CreateSymptomAsync(userId, request);

            result.MeasurementId.Should().Be(measurementId.ToString());
            result.PatientId.Should().Be(patientId.ToString());
            result.Type.Should().Be("tachycardia");
            result.Confidence.Should().Be(85.5);
            result.Description.Should().Be("Frecuencia elevada en reposo");

            _symptomRepositoryMock.Verify(x => x.CreateAsync(It.Is<Symptom>(s =>
                s.MeasurementId == measurementId &&
                s.PatientId == patientId &&
                s.Type == "tachycardia" &&
                s.Confidence == 85.5
            )), Times.Once);
        }

        [Fact]
        public async Task CreateSymptomAsync_PatientNotFound_ThrowsKeyNotFoundException()
        {
            var userId = ObjectId.GenerateNewId();

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync((Patient?)null);

            var request = new CreateSymptomRequest
            {
                MeasurementId = ObjectId.GenerateNewId().ToString(),
                Type = "tachycardia",
                Confidence = 90
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _symptomService.CreateSymptomAsync(userId, request)
            );
        }

        [Fact]
        public async Task CreateSymptomAsync_InvalidType_ThrowsInvalidOperationException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(CreatePatient(userId, patientId));

            var request = new CreateSymptomRequest
            {
                MeasurementId = ObjectId.GenerateNewId().ToString(),
                Type = "unknown_symptom",
                Confidence = 90
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _symptomService.CreateSymptomAsync(userId, request)
            );

            _measurementRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ObjectId>()), Times.Never);
        }

        [Fact]
        public async Task CreateSymptomAsync_InvalidConfidence_ThrowsInvalidOperationException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(CreatePatient(userId, patientId));

            var request = new CreateSymptomRequest
            {
                MeasurementId = ObjectId.GenerateNewId().ToString(),
                Type = "tachycardia",
                Confidence = 150
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _symptomService.CreateSymptomAsync(userId, request)
            );
        }

        [Fact]
        public async Task CreateSymptomAsync_MeasurementNotOwned_ThrowsKeyNotFoundException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var otherPatientId = ObjectId.GenerateNewId();
            var measurementId = ObjectId.GenerateNewId();

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(CreatePatient(userId, patientId));

            _measurementRepositoryMock
                .Setup(x => x.GetByIdAsync(measurementId))
                .ReturnsAsync(CreateMeasurement(measurementId, otherPatientId));

            var request = new CreateSymptomRequest
            {
                MeasurementId = measurementId.ToString(),
                Type = "tachycardia",
                Confidence = 90
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _symptomService.CreateSymptomAsync(userId, request)
            );

            _symptomRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Symptom>()), Times.Never);
        }

        [Fact]
        public async Task CreateAutomaticAsync_TachycardiaRest_CreatesSymptom()
        {
            var patientId = ObjectId.GenerateNewId();
            var measurementId = ObjectId.GenerateNewId();

            var measurement = new HeartRateMeasurement
            {
                Id = measurementId,
                Bpm = 120,
                Context = "rest",
                IsNormal = false
            };

            await _symptomService.CreateAutomaticAsync(patientId, measurement);

            _symptomRepositoryMock.Verify(x => x.CreateAsync(It.Is<Symptom>(s =>
                s.MeasurementId == measurementId &&
                s.PatientId == patientId &&
                s.Type == "tachycardia" &&
                s.Confidence == 20 &&
                s.Description!.Contains("above")
            )), Times.Once);
        }

        [Fact]
        public async Task CreateAutomaticAsync_BradycardiaRest_CreatesSymptom()
        {
            var patientId = ObjectId.GenerateNewId();
            var measurementId = ObjectId.GenerateNewId();

            var measurement = new HeartRateMeasurement
            {
                Id = measurementId,
                Bpm = 50,
                Context = "rest",
                IsNormal = false
            };

            await _symptomService.CreateAutomaticAsync(patientId, measurement);

            _symptomRepositoryMock.Verify(x => x.CreateAsync(It.Is<Symptom>(s =>
                s.MeasurementId == measurementId &&
                s.PatientId == patientId &&
                s.Type == "bradycardia" &&
                s.Confidence == 16.7 &&
                s.Description!.Contains("below")
            )), Times.Once);
        }

        [Fact]
        public async Task CreateAutomaticAsync_NormalBpm_DoesNotCreateSymptom()
        {
            var patientId = ObjectId.GenerateNewId();

            var measurement = new HeartRateMeasurement
            {
                Id = ObjectId.GenerateNewId(),
                Bpm = 80,
                Context = "rest",
                IsNormal = true
            };

            await _symptomService.CreateAutomaticAsync(patientId, measurement);

            _symptomRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Symptom>()), Times.Never);
        }

        [Fact]
        public async Task CreateAutomaticAsync_ActiveHigh_CreatesTachycardia()
        {
            var patientId = ObjectId.GenerateNewId();

            var measurement = new HeartRateMeasurement
            {
                Id = ObjectId.GenerateNewId(),
                Bpm = 170,
                Context = "active",
                IsNormal = false
            };

            await _symptomService.CreateAutomaticAsync(patientId, measurement);

            _symptomRepositoryMock.Verify(x => x.CreateAsync(It.Is<Symptom>(s =>
                s.Type == "tachycardia" &&
                s.Description!.Contains("160")
            )), Times.Once);
        }

        [Fact]
        public async Task CreateAutomaticAsync_SleepHigh_CreatesTachycardia()
        {
            var patientId = ObjectId.GenerateNewId();

            var measurement = new HeartRateMeasurement
            {
                Id = ObjectId.GenerateNewId(),
                Bpm = 90,
                Context = "sleep",
                IsNormal = false
            };

            await _symptomService.CreateAutomaticAsync(patientId, measurement);

            _symptomRepositoryMock.Verify(x => x.CreateAsync(It.Is<Symptom>(s =>
                s.Type == "tachycardia" &&
                s.Description!.Contains("80")
            )), Times.Once);
        }

        [Fact]
        public async Task GetUserSymptomsAsync_Success_ReturnsSymptoms()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var patient = CreatePatient(userId, patientId);

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var symptomId = ObjectId.GenerateNewId();
            var symptoms = new List<Symptom>
            {
                new()
                {
                    Id = symptomId,
                    PatientId = patientId,
                    Type = "bradycardia",
                    Confidence = 70,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _symptomRepositoryMock
                .Setup(x => x.GetByPatientIdAsync(patientId))
                .ReturnsAsync(symptoms);

            var result = await _symptomService.GetUserSymptomsAsync(userId);

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(symptomId.ToString());
            result[0].Type.Should().Be("bradycardia");
        }

        [Fact]
        public async Task GetSymptomByIdAsync_Success_ReturnsSymptom()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var symptomId = ObjectId.GenerateNewId();
            var patient = CreatePatient(userId, patientId);

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var symptom = new Symptom
            {
                Id = symptomId,
                PatientId = patientId,
                Type = "arrhythmia",
                Confidence = 60,
                CreatedAt = DateTime.UtcNow
            };

            _symptomRepositoryMock
                .Setup(x => x.GetByIdAsync(symptomId))
                .ReturnsAsync(symptom);

            var result = await _symptomService.GetSymptomByIdAsync(userId, symptomId);

            result.Id.Should().Be(symptomId.ToString());
            result.Type.Should().Be("arrhythmia");
        }

        [Fact]
        public async Task GetSymptomByIdAsync_NotOwned_ThrowsKeyNotFoundException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var otherPatientId = ObjectId.GenerateNewId();
            var symptomId = ObjectId.GenerateNewId();
            var patient = CreatePatient(userId, patientId);

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var symptom = new Symptom
            {
                Id = symptomId,
                PatientId = otherPatientId,
                Type = "tachycardia",
                Confidence = 80,
                CreatedAt = DateTime.UtcNow
            };

            _symptomRepositoryMock
                .Setup(x => x.GetByIdAsync(symptomId))
                .ReturnsAsync(symptom);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _symptomService.GetSymptomByIdAsync(userId, symptomId)
            );
        }

        [Fact]
        public async Task GetByMeasurementIdAsync_Success_ReturnsSymptoms()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var measurementId = ObjectId.GenerateNewId();
            var patient = CreatePatient(userId, patientId);

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            _measurementRepositoryMock
                .Setup(x => x.GetByIdAsync(measurementId))
                .ReturnsAsync(CreateMeasurement(measurementId, patientId));

            var symptoms = new List<Symptom>
            {
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    MeasurementId = measurementId,
                    PatientId = patientId,
                    Type = "irregular_pattern",
                    Confidence = 55,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _symptomRepositoryMock
                .Setup(x => x.GetByMeasurementIdAsync(measurementId))
                .ReturnsAsync(symptoms);

            var result = await _symptomService.GetByMeasurementIdAsync(userId, measurementId);

            result.Should().HaveCount(1);
            result[0].Type.Should().Be("irregular_pattern");
            result[0].MeasurementId.Should().Be(measurementId.ToString());
        }

        [Fact]
        public async Task GetByMeasurementIdAsync_MeasurementNotOwned_ThrowsKeyNotFoundException()
        {
            var userId = ObjectId.GenerateNewId();
            var patientId = ObjectId.GenerateNewId();
            var otherPatientId = ObjectId.GenerateNewId();
            var measurementId = ObjectId.GenerateNewId();
            var patient = CreatePatient(userId, patientId);

            _patientRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            _measurementRepositoryMock
                .Setup(x => x.GetByIdAsync(measurementId))
                .ReturnsAsync(CreateMeasurement(measurementId, otherPatientId));

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _symptomService.GetByMeasurementIdAsync(userId, measurementId)
            );

            _symptomRepositoryMock.Verify(x => x.GetByMeasurementIdAsync(It.IsAny<ObjectId>()), Times.Never);
        }
    }
}
