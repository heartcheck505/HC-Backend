using HeartCheck.Data;
using HeartCheck.DTOs.Measurements;
using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public class MeasurementService : IMeasurementService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IMeasurementRepository _measurementRepository;
        private readonly IAlertRepository _alertRepository;
        private readonly ISymptomService _symptomService;
        private readonly IPredictionService _predictionService;

        public MeasurementService(
            IPatientRepository patientRepository,
            IDeviceRepository deviceRepository,
            IMeasurementRepository measurementRepository,
            IAlertRepository alertRepository,
            ISymptomService symptomService,
            IPredictionService predictionService)
        {
            _patientRepository = patientRepository;
            _deviceRepository = deviceRepository;
            _measurementRepository = measurementRepository;
            _alertRepository = alertRepository;
            _symptomService = symptomService;
            _predictionService = predictionService;
        }

        public async Task<MeasurementResponse> CreateAsync(ObjectId userId, CreateMeasurementRequest request)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            if (patient == null)
            {
                throw new KeyNotFoundException("Patient profile not found");
            }

            var deviceId = ObjectId.Parse(request.DeviceId);
            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null || device.PatientId != patient.Id)
            {
                throw new InvalidOperationException("Device not found or not associated with this patient");
            }

            if (device.Status != "active")
            {
                throw new InvalidOperationException("Device is not active");
            }

            var isNormal = CalculateIsNormal(request.Bpm, request.Context);

            var measurement = new HeartRateMeasurement
            {
                Timestamp = DateTime.UtcNow,
                Metadata = new MeasurementMetadata
                {
                    PatientId = patient.Id,
                    DeviceId = device.Id
                },
                Bpm = request.Bpm,
                Quality = request.Quality,
                Context = request.Context,
                IsNormal = isNormal,
                Notes = request.Notes
            };

            await _measurementRepository.AddAsync(measurement);

            if (!isNormal)
            {
                var (alertType, threshold) = CalculateAlertData(request.Bpm, request.Context);

                var severity = CalculateSeverity(request.Bpm, threshold);

                var alert = new Alert
                {
                    MeasurementId = measurement.Id,
                    PatientId = patient.Id,
                    DeviceId = device.Id,
                    Type = alertType,
                    Severity = severity,
                    BpmValue = request.Bpm,
                    Threshold = threshold,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                };

                await _alertRepository.CreateAsync(alert);

                await _symptomService.CreateAutomaticAsync(patient.Id, measurement);
            }

            var riskAssessment = EvaluateRisk(request, patient);

            return MapToResponse(measurement, riskAssessment);
        }

        public async Task<List<MeasurementResponse>> GetHistoryAsync(
            ObjectId userId, DateTime? from, DateTime? to)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            if (patient == null)
            {
                throw new KeyNotFoundException("Patient profile not found");
            }

            var measurements = await _measurementRepository
                .GetByPatientIdAndRangeAsync(patient.Id, from, to);

            return measurements.Select(m => MapToResponse(m)).ToList();
        }

        private static (string alertType, int threshold) CalculateAlertData(int bpm, string context)
        {
            var (low, high) = BpmThresholds.Get(context);
            return bpm > high ? ("high_bpm", high) : ("low_bpm", low);
        }

        private static string CalculateSeverity(int bpm, int threshold)
        {
            var gap = Math.Abs(bpm - threshold);
            var pct = threshold > 0 ? (double)gap / threshold : 0;

            return pct switch
            {
                > 0.40 => "critical",
                > 0.25 => "high",
                > 0.10 => "medium",
                _ => "low"
            };
        }

        private static bool CalculateIsNormal(int bpm, string context)
        {
            var (low, high) = BpmThresholds.Get(context);
            return bpm >= low && bpm <= high;
        }

        private RiskAssessmentDto? EvaluateRisk(CreateMeasurementRequest request, Patient patient)
        {
            var age = CalculateAge(patient.DateOfBirth);
            if (age == 0)
            {
                age = 30;
            }

            var hasSymptoms = request.Symptoms != null && request.Symptoms.Any();

            try
            {
                return _predictionService.PredictRisk(
                    request.Bpm,
                    request.Context,
                    age,
                    hasSymptoms);
            }
            catch
            {
                return null;
            }
        }

        private static int CalculateAge(DateTime dateOfBirth)
        {
            if (dateOfBirth == default)
            {
                return 0;
            }

            var today = DateTime.UtcNow;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age < 0 ? 0 : age;
        }

        private static MeasurementResponse MapToResponse(
            HeartRateMeasurement measurement,
            RiskAssessmentDto? riskAssessment = null)
        {
            return new MeasurementResponse
            {
                Timestamp = measurement.Timestamp,
                PatientId = measurement.Metadata.PatientId.ToString(),
                DeviceId = measurement.Metadata.DeviceId.ToString(),
                Bpm = measurement.Bpm,
                Quality = measurement.Quality,
                Context = measurement.Context,
                IsNormal = measurement.IsNormal,
                Notes = measurement.Notes,
                RiskAssessment = riskAssessment
            };
        }
    }
}
