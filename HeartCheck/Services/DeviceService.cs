using HeartCheck.Data;
using HeartCheck.DTOs.Devices;
using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IDeviceRepository _deviceRepository;

        public DeviceService(
            IPatientRepository patientRepository,
            IDeviceRepository deviceRepository)
        {
            _patientRepository = patientRepository;
            _deviceRepository = deviceRepository;
        }

        public async Task<DeviceResponse> PairAsync(ObjectId userId, PairDeviceRequest request)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            if (patient == null)
            {
                throw new KeyNotFoundException("Patient profile not found. Create a patient profile first.");
            }

            var existing = await _deviceRepository.GetByIdentifierAsync(request.DeviceIdentifier);
            if (existing != null)
            {
                throw new InvalidOperationException("Device identifier already registered");
            }

            var device = new Device
            {
                PatientId = patient.Id,
                DeviceIdentifier = request.DeviceIdentifier,
                DeviceModel = request.DeviceModel,
                FirmwareVersion = request.FirmwareVersion,
                Status = "active",
                LastSync = DateTime.UtcNow,
                BatteryLevel = request.BatteryLevel,
                PairedAt = DateTime.UtcNow,
                UnpairedAt = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _deviceRepository.CreateAsync(device);
            return MapToResponse(device);
        }

        public async Task<List<DeviceResponse>> GetByUserIdAsync(ObjectId userId)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            if (patient == null)
            {
                throw new KeyNotFoundException("Patient profile not found");
            }

            var devices = await _deviceRepository.GetByPatientIdAsync(patient.Id);
            return devices.Select(MapToResponse).ToList();
        }

        public async Task UnpairAsync(ObjectId userId, ObjectId deviceId)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            if (patient == null)
            {
                throw new KeyNotFoundException("Patient profile not found");
            }

            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null || device.PatientId != patient.Id)
            {
                throw new KeyNotFoundException("Device not found or not associated with this patient");
            }

            device.Status = "inactive";
            device.UnpairedAt = DateTime.UtcNow;
            device.UpdatedAt = DateTime.UtcNow;

            await _deviceRepository.UpdateAsync(device);
        }

        private static DeviceResponse MapToResponse(Device device)
        {
            return new DeviceResponse
            {
                Id = device.Id.ToString(),
                PatientId = device.PatientId.ToString(),
                DeviceIdentifier = device.DeviceIdentifier,
                DeviceModel = device.DeviceModel,
                FirmwareVersion = device.FirmwareVersion,
                Status = device.Status,
                LastSync = device.LastSync,
                BatteryLevel = device.BatteryLevel,
                PairedAt = device.PairedAt,
                UnpairedAt = device.UnpairedAt,
                CreatedAt = device.CreatedAt,
                UpdatedAt = device.UpdatedAt
            };
        }
    }
}
