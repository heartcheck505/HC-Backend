using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly MongoDbContext _context;

        public DeviceRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Device?> GetByIdAsync(ObjectId id)
        {
            return await _context.Devices
                .Find(d => d.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Device?> GetByIdentifierAsync(string deviceIdentifier)
        {
            return await _context.Devices
                .Find(d => d.DeviceIdentifier == deviceIdentifier)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Device>> GetByPatientIdAsync(ObjectId patientId)
        {
            return await _context.Devices
                .Find(d => d.PatientId == patientId)
                .ToListAsync();
        }

        public async Task CreateAsync(Device device)
        {
            await _context.Devices.InsertOneAsync(device);
        }

        public async Task UpdateAsync(Device device)
        {
            await _context.Devices.ReplaceOneAsync(
                d => d.Id == device.Id, device);
        }
    }
}
