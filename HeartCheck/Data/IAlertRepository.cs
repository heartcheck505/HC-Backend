using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface IAlertRepository
    {
        Task<Alert?> GetByIdAsync(ObjectId id);
        Task<List<Alert>> GetByPatientIdAsync(ObjectId patientId);
        Task<List<Alert>> GetActiveByPatientIdAsync(ObjectId patientId);
        Task CreateAsync(Alert alert);
        Task UpdateAsync(Alert alert);
    }
}
