using aspnetapp.Dao.RepositoryInterface.Web;

namespace aspnetapp.Controllers.Web {
    public class UserControllerWeb : Controller, IUserRepositoryWeb {

        private readonly MyDbContext _context;
        private readonly ILogger<UserControllerWeb> _logger;

        public UserControllerWeb(MyDbContext context, ILogger<UserControllerWeb> logger) {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetById(int id) {
            return await _context.User.FindAsync(id);
        }

        public async Task<List<User>> GetTablePage(int limit, int pageIndex) {
            return await _context.User
                .OrderBy(u => u.Id) // 根据主键排序，确保分页顺序一致
                .Skip((pageIndex - 1) * limit) // 跳过前面页的数据
                .Take(limit) // 获取当前页的数据
                .ToListAsync();
        }

        public async Task<List<string>> GetTableStructure() {
            var properties = typeof(User).GetProperties();
            List<string> structure = properties.Select(prop => $"{prop.Name} ({prop.PropertyType.Name})").ToList();
            return await Task.FromResult(structure);
        }

        /// <summary>
        /// 更新车辆状态
        /// </summary>
        /// <param name="vehicle">包含车辆 ID 和状态的车辆对象</param>
        /// <returns>更新操作结果</returns>
        public async Task<IActionResult> ChangeVehicleStatus(Vehicle vehicle) {
            _logger.LogInformation("Starting status update process for vehicle with ID {VehicleId}", vehicle.Id);

            // 查找车辆
            var existingVehicle = await _context.Vehicle.FindAsync(vehicle.Id);
            if (existingVehicle == null) {
                _logger.LogWarning("Vehicle with ID {VehicleId} not found", vehicle.Id);
                return NotFound("Vehicle not found.");
            }

            // 更新车辆状态
            existingVehicle.State = vehicle.State;

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

            // 设置并发标记
            _context.Entry(existingVehicle).Property("RowVersion").OriginalValue = vehicle.RowVersion;

            try {
                _context.Vehicle.Update(existingVehicle);
                await _context.SaveChangesAsync();
                _logger.LogInformation("ID为{VehicleId}的车辆已成功更新", vehicle.Id);
                return Ok("车辆状态更新成功。");
            } catch (DbUpdateConcurrencyException) {
                _logger.LogWarning("更新ID{VehicleId}的车辆状态时发生并发冲突", vehicle.Id);
                return Conflict("由于并发更改，更新失败。");
            } catch (DbUpdateException ex) {
                _logger.LogError(ex, "更新ID{VehicleId}的车辆状态时出错", vehicle.Id);
                return StatusCode(500, "更新车辆状态时出错。");
            } catch (Exception ex) {
                _logger.LogError(ex, "更新ID{VehicleId}的车辆状态时发生意外错误", vehicle.Id);
                return StatusCode(500, "更新车辆状态时发生意外错误。");
            }
        }

        /// <summary>
        /// 查询用户
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="name"></param>
        /// <param name="nickname"></param>
        /// <returns></returns>
        public async Task<List<User>> SearchUsers(string? phone = null, string? name = null, string? nickname = null) {
            _logger.LogInformation("Starting search with filters - Phone: {Phone}, Name: {Name}, Nickname: {Nickname}", phone, name, nickname);

            // 构建查询的基础对象
            var query = _context.User.AsQueryable();

            // 根据传入的参数动态添加条件
            if (!string.IsNullOrEmpty(phone)) {
                query = query.Where(u => u.Phone.Contains(phone));
            }
            if (!string.IsNullOrEmpty(name)) {
                query = query.Where(u => u.Name.Contains(name));
            }
            if (!string.IsNullOrEmpty(nickname)) {
                query = query.Where(u => u.Nickname.Contains(nickname));
            }

            var results = await query.ToListAsync();
            _logger.LogInformation("Found {Count} users with given filters", results.Count);

            return results;
        }

        public async Task<List<User>> SearchUsers(string? phone = null, string? name = null, string? nickname = null, string sortField = "Id", string sortOrder = "asc") {
            _logger.LogInformation("Starting search with filters - Phone: {Phone}, Name: {Name}, Nickname: {Nickname}, SortField: {SortField}, SortOrder: {SortOrder}",
                                   phone, name, nickname, sortField, sortOrder);

            var query = _context.User.AsQueryable();

            // 过滤条件
            if (!string.IsNullOrEmpty(phone)) {
                query = query.Where(u => u.Phone.Contains(phone));
            }
            if (!string.IsNullOrEmpty(name)) {
                query = query.Where(u => u.Name.Contains(name));
            }
            if (!string.IsNullOrEmpty(nickname)) {
                query = query.Where(u => u.Nickname.Contains(nickname));
            }

            // 排序逻辑
            query = sortField.ToLower() switch {
                "phone" => sortOrder == "asc" ? query.OrderBy(u => u.Phone) : query.OrderByDescending(u => u.Phone),
                "name" => sortOrder == "asc" ? query.OrderBy(u => u.Name) : query.OrderByDescending(u => u.Name),
                "createdat" => sortOrder == "asc" ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                _ => sortOrder == "asc" ? query.OrderBy(u => u.Id) : query.OrderByDescending(u => u.Id),
            };

            var results = await query.ToListAsync();
            _logger.LogInformation("Found {Count} users with given filters and sorting", results.Count);

            return results;
        }
    }
}
