using aspnetapp.Dao.RepositoryInterface.Web;

namespace aspnetapp.Controllers.Web {
    public class VehicleControllerWeb : Controller, IVehicleRepositoryWeb {
        private readonly MyDbContext _context;
        private readonly ILogger<VehicleControllerWeb> _logger;

        public VehicleControllerWeb(MyDbContext context, ILogger<VehicleControllerWeb> logger) {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 根据 ID 获取车辆信息
        /// </summary>
        public async Task<Vehicle?> GetById(int id) {
            return await _context.Vehicle.FindAsync(id);
        }

        /// <summary>
        /// 获取分页车辆列表
        /// </summary>
        public async Task<List<Vehicle>> GetTablePage(int limit, int pageIndex) {
            return await _context.Vehicle
                .OrderBy(v => v.Id) // 根据主键排序
                .Skip((pageIndex - 1) * limit)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// 获取车辆表的结构信息
        /// </summary>
        public async Task<List<string>> GetTableStructure() {
            var properties = typeof(Vehicle).GetProperties();
            List<string> structure = properties.Select(prop => $"{prop.Name} ({prop.PropertyType.Name})").ToList();
            return await Task.FromResult(structure);
        }

        /// <summary>
        /// 添加车辆
        /// </summary>
        public async Task<int> AddVehicle(Vehicle vehicle) {
            _context.Vehicle.Add(vehicle);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 更新车辆状态
        /// </summary>
        public async Task<int> ChangeVehicleStatus(int vehicleId, Vehicle.Estates status) {
            var vehicle = await _context.Vehicle.FindAsync(vehicleId);
            if (vehicle == null) {
                _logger.LogWarning("Vehicle with ID {VehicleId} not found", vehicleId);
                return 0;
            }

            vehicle.State = status;
            _context.Vehicle.Update(vehicle);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 更新车辆信息
        /// </summary>
        public async Task<int> UpdateVehicle(Vehicle vehicle) {
            var existingVehicle = await _context.Vehicle.FindAsync(vehicle.Id);
            if (existingVehicle == null) {
                _logger.LogWarning("Vehicle with ID {VehicleId} not found", vehicle.Id);
                return 0;
            }

            existingVehicle.TheOriginalStore = vehicle.TheOriginalStore;
            existingVehicle.TheCurrentStore = vehicle.TheCurrentStore;
            existingVehicle.PlateNumber = vehicle.PlateNumber;
            existingVehicle.FrameNumber = vehicle.FrameNumber;
            existingVehicle.Certificate = vehicle.Certificate;
            existingVehicle.Invoice = vehicle.Invoice;
            existingVehicle.Drivinglicense = vehicle.Drivinglicense;
            existingVehicle.PurchaseRegistrationTime = vehicle.PurchaseRegistrationTime;
            existingVehicle.Owner = vehicle.Owner;
            existingVehicle.VehicleIntroduction = vehicle.VehicleIntroduction;
            existingVehicle.Pictures = vehicle.Pictures;
            existingVehicle.State = vehicle.State;
            existingVehicle.IsCase = vehicle.IsCase;
            existingVehicle.UpdatedAt = DateTime.Now;

            _context.Entry(existingVehicle).Property("RowVersion").OriginalValue = vehicle.RowVersion;
            try {
                _context.Vehicle.Update(existingVehicle);
                return await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                _logger.LogWarning("Concurrency conflict while updating vehicle with ID {VehicleId}", vehicle.Id);
                throw;
            }
        }

        /// <summary>
        /// 删除车辆
        /// </summary>
        public async Task<int> DeleteVehicle(int id) {
            var vehicle = await _context.Vehicle.FindAsync(id);
            if (vehicle == null) {
                _logger.LogWarning("Vehicle with ID {VehicleId} not found", id);
                return 0;
            }

            _context.Vehicle.Remove(vehicle);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 搜索车辆
        /// </summary>
        public async Task<List<Vehicle>> SearchVehicles() {
            return new List<Vehicle>();
        }
    }
}
