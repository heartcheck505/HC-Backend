using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface IDeviceRepository
    {
        Task<Device?> GetByIdAsync(ObjectId id);
        Task<Device?> GetByIdentifierAsync(string deviceIdentifier);
        Task<List<Device>> GetByPatientIdAsync(ObjectId patientId,
            int page = 1, int pageSize = 10);
        Task CreateAsync(Device device);
        Task UpdateAsync(Device device);
    }
}
