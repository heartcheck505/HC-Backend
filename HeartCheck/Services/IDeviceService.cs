using HeartCheck.DTOs.Devices;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public interface IDeviceService
    {
        Task<DeviceResponse> PairAsync(ObjectId userId, PairDeviceRequest request);
        Task<List<DeviceResponse>> GetByUserIdAsync(ObjectId userId);
        Task UnpairAsync(ObjectId userId, ObjectId deviceId);
    }
}
