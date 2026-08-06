using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface ISymptomRepository
    {
        Task<Symptom?> GetByIdAsync(ObjectId id);
        Task<List<Symptom>> GetByPatientIdAsync(ObjectId patientId);
        Task<List<Symptom>> GetByMeasurementIdAsync(ObjectId measurementId);
        Task CreateAsync(Symptom symptom);
    }
}
