using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class PatientRepository : IPatientRepository
    {
        private readonly MongoDbContext _context;

        public PatientRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Patient?> GetByUserIdAsync(ObjectId userId)
        {
            return await _context.Patients
                .Find(p => p.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Patient patient)
        {
            await _context.Patients.InsertOneAsync(patient);
        }

        public async Task UpdateAsync(Patient patient)
        {
            await _context.Patients.ReplaceOneAsync(
                p => p.Id == patient.Id, patient);
        }
    }
}
