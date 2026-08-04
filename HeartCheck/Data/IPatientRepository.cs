using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface IPatientRepository
    {
        Task<Patient?> GetByUserIdAsync(ObjectId userId);
        Task CreateAsync(Patient patient);
        Task UpdateAsync(Patient patient);
    }
}
