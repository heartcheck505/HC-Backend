using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface IDailyStatisticRepository
    {
        Task<List<DailyStatistic>> GetByPatientIdAndDateRangeAsync(
            ObjectId patientId, DateTime fromDate, DateTime toDate);
        Task UpsertAsync(DailyStatistic statistic);
    }
}