using HeartCheck.DTOs.Devices;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public interface IDeviceService
    {
        Task<DeviceResponse> PairAsync(ObjectId userId, PairDeviceRequest request);
        Task<List<DeviceResponse>> GetByUserIdAsync(ObjectId userId,
            int page = 1, int pageSize = 10);
        Task UnpairAsync(ObjectId userId, ObjectId deviceId);
    }
}
