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
        public async Task<int> ChangeVehicleStatus(Vehicle vehicle) {

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

            existingVehicle.Name = vehicle.Name;
            existingVehicle.Model = vehicle.Model;
            existingVehicle.RegistrationNumber = vehicle.RegistrationNumber;
            existingVehicle.Status = vehicle.Status;
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
        public async Task<List<Vehicle>> SearchVehicles(string? name = null, string? model = null, string? registrationNumber = null, string sortField = "Id", string sortOrder = "asc") {
            _logger.LogInformation("Starting vehicle search with filters - Name: {Name}, Model: {Model}, RegistrationNumber: {RegistrationNumber}, SortField: {SortField}, SortOrder: {SortOrder}",
                                   name, model, registrationNumber, sortField, sortOrder);

            var query = _context.Vehicle.AsQueryable();

            if (!string.IsNullOrEmpty(name)) {
                query = query.Where(v => v.Name.Contains(name));
            }
            if (!string.IsNullOrEmpty(model)) {
                query = query.Where(v => v.Model.Contains(model));
            }
            if (!string.IsNullOrEmpty(registrationNumber)) {
                query = query.Where(v => v.RegistrationNumber.Contains(registrationNumber));
            }

            query = sortField.ToLower() switch {
                "name" => sortOrder == "asc" ? query.OrderBy(v => v.Name) : query.OrderByDescending(v => v.Name),
                "model" => sortOrder == "asc" ? query.OrderBy(v => v.Model) : query.OrderByDescending(v => v.Model),
                "registrationnumber" => sortOrder == "asc" ? query.OrderBy(v => v.RegistrationNumber) : query.OrderByDescending(v => v.RegistrationNumber),
                _ => sortOrder == "asc" ? query.OrderBy(v => v.Id) : query.OrderByDescending(v => v.Id),
            };

            var results = await query.ToListAsync();
            _logger.LogInformation("Found {Count} vehicles with given filters and sorting", results.Count);

            return results;
        }

        public Task<int> ChangeVehicleStatus(int vehicleId, Vehicle.Emodel status) {
            throw new NotImplementedException();
        }
    }
}
