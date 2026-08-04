using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class AlertRepository : IAlertRepository
    {
        private readonly MongoDbContext _context;

        public AlertRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Alert?> GetByIdAsync(ObjectId id)
        {
            return await _context.Alerts
                .Find(a => a.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Alert>> GetByPatientIdAsync(ObjectId patientId)
        {
            return await _context.Alerts
                .Find(a => a.PatientId == patientId)
                .SortByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Alert>> GetActiveByPatientIdAsync(ObjectId patientId)
        {
            return await _context.Alerts
                .Find(a => a.PatientId == patientId && a.Status == "active")
                .SortByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateAsync(Alert alert)
        {
            await _context.Alerts.InsertOneAsync(alert);
        }

        public async Task UpdateAsync(Alert alert)
        {
            await _context.Alerts.ReplaceOneAsync(
                a => a.Id == alert.Id, alert);
        }
    }
}
