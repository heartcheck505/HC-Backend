namespace HeartCheck.DTOs.Patients
{
    public class UpdatePatientRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }
        public string? BloodType { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Observations { get; set; }
        public List<EmergencyContactDto>? EmergencyContacts { get; set; }
    }
}
