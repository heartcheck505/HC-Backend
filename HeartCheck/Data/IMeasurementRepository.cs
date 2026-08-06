using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface IMeasurementRepository
    {
        Task<HeartRateMeasurement?> GetByIdAsync(ObjectId id);
        Task AddAsync(HeartRateMeasurement measurement);
        Task<List<HeartRateMeasurement>> GetByPatientIdAndRangeAsync(
            ObjectId patientId, DateTime? from, DateTime? to);
    }
}
