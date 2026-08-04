using HeartCheck.DTOs.Plans;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public interface IPlanService
    {
        Task<List<PlanResponse>> GetActivePlansAsync();
        Task<UserPlanResponse> AssignPlanToUserAsync(ObjectId userId, AssignUserPlanRequest request);
        Task<UserPlanResponse?> GetUserActivePlanAsync(ObjectId userId);
    }
}
