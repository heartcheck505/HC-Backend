using HeartCheck.Configurations;
using HeartCheck.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> Users =>
            _database.GetCollection<User>("users");

        public IMongoCollection<Role> Roles =>
            _database.GetCollection<Role>("roles");

        public IMongoCollection<UserRole> UserRoles =>
            _database.GetCollection<UserRole>("user_roles");

        public IMongoCollection<Patient> Patients =>
            _database.GetCollection<Patient>("patients");

        public IMongoCollection<Device> Devices =>
            _database.GetCollection<Device>("devices");

        public IMongoCollection<HeartRateMeasurement> HeartRateMeasurements =>
            _database.GetCollection<HeartRateMeasurement>("heart_rate_measurements");

        public IMongoCollection<Alert> Alerts =>
            _database.GetCollection<Alert>("alerts");

        public IMongoCollection<Event> Events =>
            _database.GetCollection<Event>("events");

        public IMongoCollection<Plan> Plans =>
            _database.GetCollection<Plan>("plans");

        public IMongoCollection<UserPlan> UserPlans =>
            _database.GetCollection<UserPlan>("user_plans");

        public IMongoCollection<Notification> Notifications =>
            _database.GetCollection<Notification>("notifications");

        public IMongoCollection<DailyStatistic> DailyStatistics =>
            _database.GetCollection<DailyStatistic>("daily_statistics");

        public IMongoCollection<EmergencyCall> EmergencyCalls =>
            _database.GetCollection<EmergencyCall>("emergency_calls");

        public IMongoCollection<Symptom> Symptoms =>
            _database.GetCollection<Symptom>("symptoms");


        public IMongoCollection<Setting> Settings =>
            _database.GetCollection<Setting>("settings");

        public IMongoCollection<AuditLog> AuditLogs =>
            _database.GetCollection<AuditLog>("audit_logs");
 
    }
}
