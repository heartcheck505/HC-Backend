using HeartCheck.Data;
using HeartCheck.DTOs.Statistics;
using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public class DailyStatisticService : IDailyStatisticService
    {
        private readonly IDailyStatisticRepository _dailyStatisticRepository;
        private readonly IMeasurementRepository _measurementRepository;

        public DailyStatisticService(
            IDailyStatisticRepository dailyStatisticRepository,
            IMeasurementRepository measurementRepository)
        {
            _dailyStatisticRepository = dailyStatisticRepository;
            _measurementRepository = measurementRepository;
        }

        public async Task<List<DailyStatisticResponse>> GetPatientStatisticsAsync(
            ObjectId patientId, DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var to = toDate ?? DateTime.UtcNow;

            var statistics = await _dailyStatisticRepository
                .GetByPatientIdAndDateRangeAsync(patientId, from, to);

            return statistics.Select(MapToResponse).ToList();
        }

        public async Task<DailyStatisticResponse> RecalculateDailyStatisticAsync(
            ObjectId patientId, DateTime date)
        {
            var startOfDay = new DateTime(
                date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
            var endOfDay = startOfDay.AddDays(1);

            var measurements = await _measurementRepository
                .GetByPatientIdAndRangeAsync(patientId, startOfDay, endOfDay);

            var statistic = new DailyStatistic
            {
                PatientId = patientId,
                Date = startOfDay,
                AverageBpm = 0,
                MinBpm = 0,
                MaxBpm = 0,
                TotalMeasurements = measurements.Count,
                NormalMeasurements = measurements.Count(m => m.IsNormal),
                AbnormalMeasurements = measurements.Count(m => !m.IsNormal),
                UpdatedAt = DateTime.UtcNow
            };

            if (measurements.Count > 0)
            {
                statistic.AverageBpm = measurements.Average(m => m.Bpm);
                statistic.MinBpm = measurements.Min(m => m.Bpm);
                statistic.MaxBpm = measurements.Max(m => m.Bpm);
            }

            await _dailyStatisticRepository.UpsertAsync(statistic);
            return MapToResponse(statistic);
        }

        private static DailyStatisticResponse MapToResponse(DailyStatistic statistic)
        {
            return new DailyStatisticResponse
            {
                Id = statistic.Id.ToString(),
                PatientId = statistic.PatientId.ToString(),
                Date = statistic.Date,
                AverageBpm = statistic.AverageBpm,
                MinBpm = statistic.MinBpm,
                MaxBpm = statistic.MaxBpm,
                TotalMeasurements = statistic.TotalMeasurements,
                NormalMeasurements = statistic.NormalMeasurements,
                AbnormalMeasurements = statistic.AbnormalMeasurements,
                UpdatedAt = statistic.UpdatedAt
            };
        }
    }
}