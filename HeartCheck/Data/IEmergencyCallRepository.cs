using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface IEmergencyCallRepository
    {
        Task<EmergencyCall?> GetByIdAsync(ObjectId id);
        Task<List<EmergencyCall>> GetByPatientIdAsync(ObjectId patientId);
        Task<List<EmergencyCall>> GetByAlertIdAsync(ObjectId alertId);
        Task CreateAsync(EmergencyCall call);
        Task UpdateAsync(EmergencyCall call);
    }
}
