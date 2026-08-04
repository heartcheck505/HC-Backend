using HeartCheck.Data;
using HeartCheck.DTOs.Alerts;
using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public class AlertService : IAlertService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IAlertRepository _alertRepository;
        private readonly IEventRepository _eventRepository;

        public AlertService(
            IPatientRepository patientRepository,
            IAlertRepository alertRepository,
            IEventRepository eventRepository)
        {
            _patientRepository = patientRepository;
            _alertRepository = alertRepository;
            _eventRepository = eventRepository;
        }

        public async Task<List<AlertResponse>> GetActiveAlertsByPatientAsync(ObjectId userId)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            if (patient == null)
            {
                throw new KeyNotFoundException("Patient profile not found");
            }

            var alerts = await _alertRepository.GetActiveByPatientIdAsync(patient.Id);
            return alerts.Select(MapToResponse).ToList();
        }

        public async Task AcknowledgeAlertAsync(ObjectId userId, ObjectId alertId, string? userResponse)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            if (patient == null)
            {
                throw new KeyNotFoundException("Patient profile not found");
            }

            var alert = await _alertRepository.GetByIdAsync(alertId);
            if (alert == null || alert.PatientId != patient.Id)
            {
                throw new KeyNotFoundException("Alert not found or not associated with this patient");
            }

            alert.Status = "acknowledged";
            alert.AcknowledgedAt = DateTime.UtcNow;
            await _alertRepository.UpdateAsync(alert);

            var evt = new Event
            {
                AlertId = alert.Id,
                PatientId = patient.Id,
                Type = "response_received",
                Description = "Alert acknowledged by patient",
                UserResponse = userResponse,
                RespondedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await _eventRepository.CreateAsync(evt);
        }

        public async Task ResolveAlertAsync(ObjectId userId, ObjectId alertId)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            if (patient == null)
            {
                throw new KeyNotFoundException("Patient profile not found");
            }

            var alert = await _alertRepository.GetByIdAsync(alertId);
            if (alert == null || alert.PatientId != patient.Id)
            {
                throw new KeyNotFoundException("Alert not found or not associated with this patient");
            }

            alert.Status = "resolved";
            alert.ResolvedAt = DateTime.UtcNow;
            await _alertRepository.UpdateAsync(alert);
        }

        private static AlertResponse MapToResponse(Alert alert)
        {
            return new AlertResponse
            {
                Id = alert.Id.ToString(),
                MeasurementId = alert.MeasurementId.ToString(),
                PatientId = alert.PatientId.ToString(),
                DeviceId = alert.DeviceId.ToString(),
                Type = alert.Type,
                Severity = alert.Severity,
                BpmValue = alert.BpmValue,
                Threshold = alert.Threshold,
                Status = alert.Status,
                AcknowledgedAt = alert.AcknowledgedAt,
                ResolvedAt = alert.ResolvedAt,
                CreatedAt = alert.CreatedAt
            };
        }
    }
}
