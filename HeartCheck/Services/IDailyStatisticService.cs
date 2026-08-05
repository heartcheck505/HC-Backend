using HeartCheck.DTOs.Statistics;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public interface IDailyStatisticService
    {
        Task<List<DailyStatisticResponse>> GetPatientStatisticsAsync(
            ObjectId patientId, DateTime? fromDate, DateTime? toDate);
        Task<DailyStatisticResponse> RecalculateDailyStatisticAsync(
            ObjectId patientId, DateTime date);
    }
}