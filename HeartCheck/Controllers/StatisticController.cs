using System.Security.Claims;
using HeartCheck.DTOs.Statistics;
using HeartCheck.Data;
using HeartCheck.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace HeartCheck.Controllers
{
    [ApiController]
    [Route("api/statistics")]
    [Authorize]
    public class StatisticController : ControllerBase
    {
        private readonly IDailyStatisticService _dailyStatisticService;
        private readonly IPatientRepository _patientRepository;

        public StatisticController(
            IDailyStatisticService dailyStatisticService,
            IPatientRepository patientRepository)
        {
            _dailyStatisticService = dailyStatisticService;
            _patientRepository = patientRepository;
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyStatistics(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                var userId = GetUserId();
                var patient = await _patientRepository.GetByUserIdAsync(userId);
                if (patient == null)
                {
                    return NotFound(new { message = "Patient profile not found" });
                }

                var statistics = await _dailyStatisticService
                    .GetPatientStatisticsAsync(patient.Id, from, to);

                return Ok(statistics);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private ObjectId GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return ObjectId.Parse(userIdClaim);
        }
    }
}