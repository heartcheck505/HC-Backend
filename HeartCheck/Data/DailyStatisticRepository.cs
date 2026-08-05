using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class DailyStatisticRepository : IDailyStatisticRepository
    {
        private readonly MongoDbContext _context;

        public DailyStatisticRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<List<DailyStatistic>> GetByPatientIdAndDateRangeAsync(
            ObjectId patientId, DateTime fromDate, DateTime toDate)
        {
            var filter = Builders<DailyStatistic>.Filter
                .Eq(ds => ds.PatientId, patientId);

            var dateFilter = Builders<DailyStatistic>.Filter
                .Gte(ds => ds.Date, fromDate) &
                Builders<DailyStatistic>.Filter
                .Lte(ds => ds.Date, toDate);

            filter &= dateFilter;

            return await _context.DailyStatistics
                .Find(filter)
                .SortBy(ds => ds.Date)
                .ToListAsync();
        }

        public async Task UpsertAsync(DailyStatistic statistic)
        {
            var existing = await _context.DailyStatistics
                .Find(ds => ds.PatientId == statistic.PatientId && ds.Date == statistic.Date)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                statistic.Id = existing.Id;
                await _context.DailyStatistics.ReplaceOneAsync(
                    ds => ds.Id == existing.Id, statistic);
            }
            else
            {
                await _context.DailyStatistics.InsertOneAsync(statistic);
            }
        }
    }
}