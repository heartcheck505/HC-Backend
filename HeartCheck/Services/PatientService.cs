using HeartCheck.Data;
using HeartCheck.DTOs.Patients;
using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public class PatientService : IPatientService
    {
        private const int MaxEmergencyContacts = 3;

        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<PatientResponse> CreateAsync(ObjectId userId, CreatePatientRequest request)
        {
            var existing = await _patientRepository.GetByUserIdAsync(userId);
            if (existing != null)
            {
                throw new InvalidOperationException("Patient profile already exists for this user");
            }

            if (request.EmergencyContacts.Count > MaxEmergencyContacts)
            {
                throw new InvalidOperationException($"Maximum of {MaxEmergencyContacts} emergency contacts allowed");
            }

            var patient = new Patient
            {
                UserId = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Weight = request.Weight,
                Height = request.Height,
                BloodType = request.BloodType,
                Phone = request.Phone,
                Address = request.Address,
                PhotoUrl = request.PhotoUrl,
                Observations = request.Observations,
                Status = "active",
                EmergencyContacts = request.EmergencyContacts.Select(c => new EmergencyContact
                {
                    Name = c.Name,
                    Relationship = c.Relationship,
                    Phone = c.Phone,
                    Email = c.Email,
                    IsPrimary = c.IsPrimary
                }).ToList(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _patientRepository.CreateAsync(patient);
            return MapToResponse(patient);
        }

        public async Task<PatientResponse> GetByUserIdAsync(ObjectId userId)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            if (patient == null)
            {
                throw new KeyNotFoundException("Patient profile not found");
            }

            return MapToResponse(patient);
        }

        public async Task<PatientResponse> UpdateAsync(ObjectId userId, UpdatePatientRequest request)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            if (patient == null)
            {
                throw new KeyNotFoundException("Patient profile not found");
            }

            if (request.FirstName != null) patient.FirstName = request.FirstName;
            if (request.LastName != null) patient.LastName = request.LastName;
            if (request.DateOfBirth.HasValue) patient.DateOfBirth = request.DateOfBirth.Value;
            if (request.Gender != null) patient.Gender = request.Gender;
            if (request.Weight.HasValue) patient.Weight = request.Weight.Value;
            if (request.Height.HasValue) patient.Height = request.Height.Value;
            if (request.BloodType != null) patient.BloodType = request.BloodType;
            if (request.Phone != null) patient.Phone = request.Phone;
            if (request.Address != null) patient.Address = request.Address;
            if (request.PhotoUrl != null) patient.PhotoUrl = request.PhotoUrl;
            if (request.Observations != null) patient.Observations = request.Observations;

            if (request.EmergencyContacts != null)
            {
                if (request.EmergencyContacts.Count > MaxEmergencyContacts)
                {
                    throw new InvalidOperationException($"Maximum of {MaxEmergencyContacts} emergency contacts allowed");
                }

                patient.EmergencyContacts = request.EmergencyContacts.Select(c => new EmergencyContact
                {
                    Name = c.Name,
                    Relationship = c.Relationship,
                    Phone = c.Phone,
                    Email = c.Email,
                    IsPrimary = c.IsPrimary
                }).ToList();
            }

            patient.UpdatedAt = DateTime.UtcNow;

            await _patientRepository.UpdateAsync(patient);
            return MapToResponse(patient);
        }

        private static PatientResponse MapToResponse(Patient patient)
        {
            return new PatientResponse
            {
                Id = patient.Id.ToString(),
                UserId = patient.UserId.ToString(),
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                Weight = patient.Weight,
                Height = patient.Height,
                BloodType = patient.BloodType,
                Phone = patient.Phone,
                Address = patient.Address,
                PhotoUrl = patient.PhotoUrl,
                Observations = patient.Observations,
                Status = patient.Status,
                EmergencyContacts = patient.EmergencyContacts.Select(c => new EmergencyContactDto
                {
                    Name = c.Name,
                    Relationship = c.Relationship,
                    Phone = c.Phone,
                    Email = c.Email,
                    IsPrimary = c.IsPrimary
                }).ToList(),
                CreatedAt = patient.CreatedAt,
                UpdatedAt = patient.UpdatedAt
            };
        }
    }
}
