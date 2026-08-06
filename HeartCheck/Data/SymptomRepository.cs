using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class SymptomRepository : ISymptomRepository
    {
        private readonly MongoDbContext _context;

        public SymptomRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Symptom?> GetByIdAsync(ObjectId id)
        {
            return await _context.Symptoms
                .Find(s => s.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Symptom>> GetByPatientIdAsync(ObjectId patientId)
        {
            return await _context.Symptoms
                .Find(s => s.PatientId == patientId)
                .SortByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Symptom>> GetByMeasurementIdAsync(ObjectId measurementId)
        {
            return await _context.Symptoms
                .Find(s => s.MeasurementId == measurementId)
                .SortByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateAsync(Symptom symptom)
        {
            await _context.Symptoms.InsertOneAsync(symptom);
        }
    }
}
