using HeartCheck.Data;
using HeartCheck.DTOs.Events;
using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public class EventService : IEventService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IAlertRepository _alertRepository;
        private readonly IEventRepository _eventRepository;

        public EventService(
            IPatientRepository patientRepository,
            IAlertRepository alertRepository,
            IEventRepository eventRepository)
        {
            _patientRepository = patientRepository;
            _alertRepository = alertRepository;
            _eventRepository = eventRepository;
        }

        public async Task<List<EventResponse>> GetByAlertIdAsync(ObjectId userId, ObjectId alertId)
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

            var events = await _eventRepository.GetByAlertIdAsync(alertId);
            return events.Select(MapToResponse).ToList();
        }

        private static EventResponse MapToResponse(Event evt)
        {
            return new EventResponse
            {
                Id = evt.Id.ToString(),
                AlertId = evt.AlertId.ToString(),
                PatientId = evt.PatientId.ToString(),
                Type = evt.Type,
                Description = evt.Description,
                UserResponse = evt.UserResponse,
                RespondedAt = evt.RespondedAt,
                CreatedAt = evt.CreatedAt
            };
        }
    }
}
