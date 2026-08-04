namespace HeartCheck.DTOs.Patients
{
    public class CreatePatientRequest
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = null!;
        public double Weight { get; set; }
        public double Height { get; set; }
        public string BloodType { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? PhotoUrl { get; set; }
        public string? Observations { get; set; }
        public List<EmergencyContactDto> EmergencyContacts { get; set; } = new();
    }
}
