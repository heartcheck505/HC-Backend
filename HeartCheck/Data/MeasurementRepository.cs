using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class MeasurementRepository : IMeasurementRepository
    {
        private readonly MongoDbContext _context;

        public MeasurementRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<HeartRateMeasurement?> GetByIdAsync(ObjectId id)
        {
            return await _context.HeartRateMeasurements
                .Find(m => m.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(HeartRateMeasurement measurement)
        {
            await _context.HeartRateMeasurements.InsertOneAsync(measurement);
        }

        public async Task<List<HeartRateMeasurement>> GetByPatientIdAndRangeAsync(
            ObjectId patientId, DateTime? from, DateTime? to)
        {
            var filter = Builders<HeartRateMeasurement>.Filter
                .Eq("metadata.patientId", patientId);

            if (from.HasValue || to.HasValue)
            {
                var timestampFilter = Builders<HeartRateMeasurement>.Filter.Empty;

                if (from.HasValue)
                    timestampFilter &= Builders<HeartRateMeasurement>.Filter
                        .Gte(m => m.Timestamp, from.Value);

                if (to.HasValue)
                    timestampFilter &= Builders<HeartRateMeasurement>.Filter
                        .Lte(m => m.Timestamp, to.Value);

                filter &= timestampFilter;
            }

            return await _context.HeartRateMeasurements
                .Find(filter)
                .SortByDescending(m => m.Timestamp)
                .ToListAsync();
        }
    }
}
