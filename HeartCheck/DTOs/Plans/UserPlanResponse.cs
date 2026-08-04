namespace HeartCheck.DTOs.Plans
{
    public class UserPlanResponse
    {
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string PlanId { get; set; } = null!;
        public string PlanName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
