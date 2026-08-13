namespace HeartCheck.DTOs.Patients
{
    public class PatientResponse
    {
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? SecondLastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = null!;
        public double Weight { get; set; }
        public double Height { get; set; }
        public string BloodType { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? PhotoUrl { get; set; }
        public string? Observations { get; set; }
        public string Status { get; set; } = null!;
        public List<EmergencyContactDto> EmergencyContacts { get; set; } = new();
        public List<string> Medications { get; set; } = new();
        public string? InitialDiagnosis { get; set; }
        public string? AssignedDoctor { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
