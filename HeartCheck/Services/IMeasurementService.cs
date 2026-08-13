using HeartCheck.DTOs.Measurements;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public interface IMeasurementService
    {
        Task<MeasurementResponse> CreateAsync(ObjectId userId, CreateMeasurementRequest request);
        Task<List<MeasurementResponse>> GetHistoryAsync(
            ObjectId userId, DateTime? from, DateTime? to,
            int page = 1, int pageSize = 10);
    }
}
