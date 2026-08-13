using HeartCheck.DTOs.Patients;

namespace HeartCheck.DTOs.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? SecondLastName { get; set; }
        public string? Phone { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }
        public string? BloodType { get; set; }
        public string? Address { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Observations { get; set; }
        public List<EmergencyContactDto>? EmergencyContacts { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyRelationship { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? EmergencyEmail { get; set; }
        public bool SetAsPrimaryEmergency { get; set; }
        public List<string>? Medications { get; set; }
        public string? InitialDiagnosis { get; set; }
        public string? AssignedDoctor { get; set; }
    }
}
