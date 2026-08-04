using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class PlanRepository : IPlanRepository
    {
        private readonly MongoDbContext _context;

        public PlanRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Plan?> GetByIdAsync(ObjectId id)
        {
            return await _context.Plans
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Plan>> GetAllActiveAsync()
        {
            return await _context.Plans
                .Find(p => p.Status == "active")
                .ToListAsync();
        }

        public async Task CreateAsync(Plan plan)
        {
            await _context.Plans.InsertOneAsync(plan);
        }

        public async Task SeedDefaultPlansAsync()
        {
            var count = await _context.Plans.CountDocumentsAsync(_ => true);
            if (count > 0) return;

            var defaultPlans = new List<Plan>
            {
                new Plan
                {
                    Name = "Basic",
                    Description = "Plan básico con acceso esencial",
                    Price = 0m,
                    MaxDevices = 1,
                    MeasurementIntervalMinutes = 30,
                    IncludesEmergencyCalls = false,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                },
                new Plan
                {
                    Name = "Premium",
                    Description = "Plan premium con monitoreo avanzado",
                    Price = 19.99m,
                    MaxDevices = 3,
                    MeasurementIntervalMinutes = 15,
                    IncludesEmergencyCalls = true,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                },
                new Plan
                {
                    Name = "Gold",
                    Description = "Plan gold con cobertura completa",
                    Price = 49.99m,
                    MaxDevices = 5,
                    MeasurementIntervalMinutes = 5,
                    IncludesEmergencyCalls = true,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Plans.InsertManyAsync(defaultPlans);
        }
    }
}
