using HeartCheck.Data;
using HeartCheck.DTOs.Plans;
using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _planRepository;
        private readonly IUserPlanRepository _userPlanRepository;

        public PlanService(
            IPlanRepository planRepository,
            IUserPlanRepository userPlanRepository)
        {
            _planRepository = planRepository;
            _userPlanRepository = userPlanRepository;
        }

        public async Task<List<PlanResponse>> GetActivePlansAsync()
        {
            var plans = await _planRepository.GetAllActiveAsync();
            return plans.Select(MapToResponse).ToList();
        }

        public async Task<UserPlanResponse> AssignPlanToUserAsync(ObjectId userId, AssignUserPlanRequest request)
        {
            var plan = await _planRepository.GetByIdAsync(ObjectId.Parse(request.PlanId));
            if (plan == null || plan.Status != "active")
            {
                throw new KeyNotFoundException("Plan not found or not active");
            }

            var existingActive = await _userPlanRepository.GetActiveByUserIdAsync(userId);
            if (existingActive != null)
            {
                existingActive.Status = "cancelled";
                existingActive.EndDate = DateTime.UtcNow;
                await _userPlanRepository.UpdateAsync(existingActive);
            }

            var userPlan = new UserPlan
            {
                UserId = userId,
                PlanId = plan.Id,
                StartDate = DateTime.UtcNow,
                EndDate = null,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            await _userPlanRepository.CreateAsync(userPlan);
            return MapToResponse(userPlan, plan);
        }

        public async Task<UserPlanResponse?> GetUserActivePlanAsync(ObjectId userId)
        {
            var userPlan = await _userPlanRepository.GetActiveByUserIdAsync(userId);
            if (userPlan == null) return null;

            var plan = await _planRepository.GetByIdAsync(userPlan.PlanId);
            return MapToResponse(userPlan, plan);
        }

        private static PlanResponse MapToResponse(Plan plan)
        {
            return new PlanResponse
            {
                Id = plan.Id.ToString(),
                Name = plan.Name,
                Description = plan.Description,
                Price = plan.Price,
                MaxDevices = plan.MaxDevices,
                MeasurementIntervalMinutes = plan.MeasurementIntervalMinutes,
                IncludesEmergencyCalls = plan.IncludesEmergencyCalls,
                Status = plan.Status,
                CreatedAt = plan.CreatedAt
            };
        }

        private static UserPlanResponse MapToResponse(UserPlan userPlan, Plan? plan)
        {
            return new UserPlanResponse
            {
                Id = userPlan.Id.ToString(),
                UserId = userPlan.UserId.ToString(),
                PlanId = userPlan.PlanId.ToString(),
                PlanName = plan?.Name ?? "Unknown",
                StartDate = userPlan.StartDate,
                EndDate = userPlan.EndDate,
                Status = userPlan.Status,
                CreatedAt = userPlan.CreatedAt
            };
        }
    }
}
