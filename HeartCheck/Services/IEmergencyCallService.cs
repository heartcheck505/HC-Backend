using HeartCheck.DTOs.EmergencyCalls;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public interface IEmergencyCallService
    {
        Task<EmergencyCallResponse> CreateEmergencyCallAsync(ObjectId userId, CreateEmergencyCallRequest request);
        Task<List<EmergencyCallResponse>> GetUserCallsAsync(ObjectId userId);
        Task<EmergencyCallResponse> GetCallByIdAsync(ObjectId userId, ObjectId callId);
        Task UpdateCallStatusAsync(ObjectId userId, ObjectId callId, UpdateEmergencyCallStatusRequest request);
    }
}
