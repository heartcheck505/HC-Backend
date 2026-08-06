using HeartCheck.Data;
using HeartCheck.DTOs.Symptoms;
using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public class SymptomService : ISymptomService
    {
        private static readonly HashSet<string> ValidTypes = new()
        {
            "tachycardia", "bradycardia", "arrhythmia", "irregular_pattern"
        };

        private readonly IPatientRepository _patientRepository;
        private readonly IMeasurementRepository _measurementRepository;
        private readonly ISymptomRepository _symptomRepository;

        public SymptomService(
            IPatientRepository patientRepository,
            IMeasurementRepository measurementRepository,
            ISymptomRepository symptomRepository)
        {
            _patientRepository = patientRepository;
            _measurementRepository = measurementRepository;
            _symptomRepository = symptomRepository;
        }

        public async Task<SymptomResponse> CreateSymptomAsync(
            ObjectId userId, CreateSymptomRequest request)
        {
            if (!ValidTypes.Contains(request.Type))
            {
                throw new InvalidOperationException($"Invalid symptom type: {request.Type}");
            }

            if (request.Confidence < 0 || request.Confidence > 100)
            {
                throw new InvalidOperationException("Confidence must be between 0 and 100");
            }

            var patient = await GetPatientByUserIdAsync(userId);

            var measurement = await _measurementRepository.GetByIdAsync(ObjectId.Parse(request.MeasurementId));
            if (measurement == null || measurement.Metadata.PatientId != patient.Id)
            {
                throw new KeyNotFoundException("Measurement not found or not associated with this patient");
            }

            var symptom = new Symptom
            {
                MeasurementId = measurement.Id,
                PatientId = patient.Id,
                Type = request.Type,
                Confidence = request.Confidence,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _symptomRepository.CreateAsync(symptom);
            return MapToResponse(symptom);
        }

        public async Task<List<SymptomResponse>> GetUserSymptomsAsync(ObjectId userId)
        {
            var patient = await GetPatientByUserIdAsync(userId);

            var symptoms = await _symptomRepository.GetByPatientIdAsync(patient.Id);
            return symptoms.Select(MapToResponse).ToList();
        }

        public async Task<SymptomResponse> GetSymptomByIdAsync(ObjectId userId, ObjectId symptomId)
        {
            var patient = await GetPatientByUserIdAsync(userId);

            var symptom = await _symptomRepository.GetByIdAsync(symptomId);
            if (symptom == null || symptom.PatientId != patient.Id)
            {
                throw new KeyNotFoundException("Symptom not found or not associated with this patient");
            }

            return MapToResponse(symptom);
        }

        public async Task<List<SymptomResponse>> GetByMeasurementIdAsync(ObjectId userId, ObjectId measurementId)
        {
            var patient = await GetPatientByUserIdAsync(userId);

            var measurement = await _measurementRepository.GetByIdAsync(measurementId);
            if (measurement == null || measurement.Metadata.PatientId != patient.Id)
            {
                throw new KeyNotFoundException("Measurement not found or not associated with this patient");
            }

            var symptoms = await _symptomRepository.GetByMeasurementIdAsync(measurementId);
            return symptoms.Select(MapToResponse).ToList();
        }

        private async Task<Patient> GetPatientByUserIdAsync(ObjectId userId)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            if (patient == null)
            {
                throw new KeyNotFoundException("Patient profile not found");
            }

            return patient;
        }

        private static SymptomResponse MapToResponse(Symptom symptom)
        {
            return new SymptomResponse
            {
                Id = symptom.Id.ToString(),
                MeasurementId = symptom.MeasurementId.ToString(),
                PatientId = symptom.PatientId.ToString(),
                Type = symptom.Type,
                Confidence = symptom.Confidence,
                Description = symptom.Description,
                CreatedAt = symptom.CreatedAt
            };
        }
    }
}
