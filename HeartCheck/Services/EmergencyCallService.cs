using HeartCheck.Data;
using HeartCheck.DTOs.EmergencyCalls;
using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public class EmergencyCallService : IEmergencyCallService
    {
        private static readonly HashSet<string> ValidStatuses = new()
        {
            "pending", "initiated", "answered", "no_answer", "voicemail", "completed", "cancelled"
        };

        private static readonly HashSet<string> TerminalStatuses = new()
        {
            "completed", "cancelled", "no_answer", "voicemail"
        };

        private readonly IPatientRepository _patientRepository;
        private readonly IAlertRepository _alertRepository;
        private readonly IEmergencyCallRepository _emergencyCallRepository;

        public EmergencyCallService(
            IPatientRepository patientRepository,
            IAlertRepository alertRepository,
            IEmergencyCallRepository emergencyCallRepository)
        {
            _patientRepository = patientRepository;
            _alertRepository = alertRepository;
            _emergencyCallRepository = emergencyCallRepository;
        }

        public async Task<EmergencyCallResponse> CreateEmergencyCallAsync(
            ObjectId userId, CreateEmergencyCallRequest request)
        {
            var patient = await GetPatientByUserIdAsync(userId);

            var alert = await _alertRepository.GetByIdAsync(ObjectId.Parse(request.AlertId));
            if (alert == null || alert.PatientId != patient.Id)
            {
                throw new KeyNotFoundException("Alert not found or not associated with this patient");
            }

            var contact = patient.EmergencyContacts
                .FirstOrDefault(c => c.Id == ObjectId.Parse(request.EmergencyContactId));
            if (contact == null)
            {
                throw new KeyNotFoundException("Emergency contact not found for this patient");
            }

            var call = new EmergencyCall
            {
                AlertId = alert.Id,
                PatientId = patient.Id,
                EmergencyContactId = contact.Id,
                ContactName = contact.Name,
                PhoneNumber = contact.Phone,
                Status = "initiated",
                InitiatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _emergencyCallRepository.CreateAsync(call);
            return MapToResponse(call);
        }

        public async Task<List<EmergencyCallResponse>> GetUserCallsAsync(ObjectId userId)
        {
            var patient = await GetPatientByUserIdAsync(userId);

            var calls = await _emergencyCallRepository.GetByPatientIdAsync(patient.Id);
            return calls.Select(MapToResponse).ToList();
        }

        public async Task<EmergencyCallResponse> GetCallByIdAsync(ObjectId userId, ObjectId callId)
        {
            var patient = await GetPatientByUserIdAsync(userId);

            var call = await _emergencyCallRepository.GetByIdAsync(callId);
            if (call == null || call.PatientId != patient.Id)
            {
                throw new KeyNotFoundException("Emergency call not found or not associated with this patient");
            }

            return MapToResponse(call);
        }

        public async Task UpdateCallStatusAsync(
            ObjectId userId, ObjectId callId, UpdateEmergencyCallStatusRequest request)
        {
            if (!ValidStatuses.Contains(request.Status))
            {
                throw new InvalidOperationException($"Invalid emergency call status: {request.Status}");
            }

            var patient = await GetPatientByUserIdAsync(userId);

            var call = await _emergencyCallRepository.GetByIdAsync(callId);
            if (call == null || call.PatientId != patient.Id)
            {
                throw new KeyNotFoundException("Emergency call not found or not associated with this patient");
            }

            call.Status = request.Status;

            if (call.Status == "initiated" && !call.InitiatedAt.HasValue)
            {
                call.InitiatedAt = DateTime.UtcNow;
            }

            if (TerminalStatuses.Contains(call.Status) && !call.CompletedAt.HasValue)
            {
                call.CompletedAt = DateTime.UtcNow;
            }

            if (request.Duration.HasValue)
            {
                call.Duration = request.Duration.Value;
            }

            if (request.Result != null)
            {
                call.Result = request.Result;
            }

            await _emergencyCallRepository.UpdateAsync(call);
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

        private static EmergencyCallResponse MapToResponse(EmergencyCall call)
        {
            return new EmergencyCallResponse
            {
                Id = call.Id.ToString(),
                AlertId = call.AlertId.ToString(),
                PatientId = call.PatientId.ToString(),
                EmergencyContactId = call.EmergencyContactId.ToString(),
                ContactName = call.ContactName,
                PhoneNumber = call.PhoneNumber,
                Status = call.Status,
                Duration = call.Duration,
                Result = call.Result,
                InitiatedAt = call.InitiatedAt,
                CompletedAt = call.CompletedAt,
                CreatedAt = call.CreatedAt
            };
        }
    }
}
