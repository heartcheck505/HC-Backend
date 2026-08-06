using HeartCheck.DTOs.Symptoms;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public interface ISymptomService
    {
        Task<SymptomResponse> CreateSymptomAsync(ObjectId userId, CreateSymptomRequest request);
        Task<List<SymptomResponse>> GetUserSymptomsAsync(ObjectId userId);
        Task<SymptomResponse> GetSymptomByIdAsync(ObjectId userId, ObjectId symptomId);
        Task<List<SymptomResponse>> GetByMeasurementIdAsync(ObjectId userId, ObjectId measurementId);
    }
}
