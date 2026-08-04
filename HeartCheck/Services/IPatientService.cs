using HeartCheck.DTOs.Patients;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public interface IPatientService
    {
        Task<PatientResponse> CreateAsync(ObjectId userId, CreatePatientRequest request);
        Task<PatientResponse> GetByUserIdAsync(ObjectId userId);
        Task<PatientResponse> UpdateAsync(ObjectId userId, UpdatePatientRequest request);
    }
}
