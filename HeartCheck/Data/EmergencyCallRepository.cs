using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class EmergencyCallRepository : IEmergencyCallRepository
    {
        private readonly MongoDbContext _context;

        public EmergencyCallRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<EmergencyCall?> GetByIdAsync(ObjectId id)
        {
            return await _context.EmergencyCalls
                .Find(c => c.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<EmergencyCall>> GetByPatientIdAsync(ObjectId patientId)
        {
            return await _context.EmergencyCalls
                .Find(c => c.PatientId == patientId)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<EmergencyCall>> GetByAlertIdAsync(ObjectId alertId)
        {
            return await _context.EmergencyCalls
                .Find(c => c.AlertId == alertId)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateAsync(EmergencyCall call)
        {
            await _context.EmergencyCalls.InsertOneAsync(call);
        }

        public async Task UpdateAsync(EmergencyCall call)
        {
            await _context.EmergencyCalls.ReplaceOneAsync(
                c => c.Id == call.Id, call);
        }
    }
}
