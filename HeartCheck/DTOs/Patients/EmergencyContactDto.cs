namespace HeartCheck.DTOs.Patients
{
    public class EmergencyContactDto
    {
        public string Name { get; set; } = null!;
        public string Relationship { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public bool IsPrimary { get; set; }
    }
}
