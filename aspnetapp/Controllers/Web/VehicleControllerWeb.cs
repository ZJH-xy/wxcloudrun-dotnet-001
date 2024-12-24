using aspnetapp.Dao.RepositoryInterface.Web;
using aspnetapp.Pages;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;
using System.Collections.Generic;

namespace aspnetapp.Controllers.Web {
    public class VehicleControllerWeb : Controller {
        private readonly MyDbContext _context;
        private readonly ILogger<VehicleControllerWeb> _logger;
        private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

        public VehicleControllerWeb(MyDbContext context, ILogger<VehicleControllerWeb> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
            _context = context;
            _logger = logger;
            _wxSetting = wxSetting;
        }

        public async Task<List<Vehicle>> GetTablePage(int limit, int pageIndex) {
            return await _context.Vehicle
                .Where(s => !s.IsDelete)
                .OrderBy(u => u.Id) // 根据主键排序，确保分页顺序一致
                .Skip((pageIndex - 1) * limit) // 跳过前面页的数据
                .Take(limit) // 获取当前页的数据
                .ToListAsync();
        }

        // 获取所有车辆列表
        public async Task<List<Vehicle>> GetAllList() {
            return await _context.Vehicle.Where(s => !s.IsDelete).ToListAsync();
        }

        // 根据ID获取单个车辆
        public async Task<Vehicle?> GetById(int id) {
            return await _context.Vehicle.FindAsync(id);
        }
        public async Task<int> GetPageSum() {
            return await _context.Vehicle.Where(s => !s.IsDelete).CountAsync();
        }

		/// <summary>
        /// 删除（逻辑删除）
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
		public async Task<IActionResult> DeleteStore(int storeId) {
			var store = await _context.Vehicle.FindAsync(storeId);
			if (store == null) {
				_logger.LogWarning("StoreMenu with ID {StoreId} not found", storeId);
				return NotFound("StoreMenu not found.");
			}

			store.IsDelete = true;

			try {
				_context.Vehicle.Update(store);
				await _context.SaveChangesAsync();
				_logger.LogInformation("StoreMenu with ID {StoreId} deleted successfully", storeId);
				return Ok("StoreMenu deleted successfully.");
			} catch (Exception ex) {
				_logger.LogError(ex, "Error deleting StoreMenu with ID {StoreId}", storeId);
				return StatusCode(500, "Error deleting StoreMenu.");
			}
		}

		// 添加
		public async Task<IActionResult> AddVehicleAsync(Vehicle newVehicle) {
            if (!ModelState.IsValid) {
                return BadRequest("车辆信息无效");
            }

            try {
                newVehicle.CreatedAt = DateTime.Now;
                newVehicle.UpdatedAt = DateTime.Now;

                // 这里保存车辆到数据库
                _context.Vehicle.Add(newVehicle);
                await _context.SaveChangesAsync();

                return Ok("车辆添加成功");
            } catch (Exception ex) {
                return StatusCode(500, $"添加车辆时发生错误: {ex.Message}");
            }
        }

        // 更新车辆信息
        public async Task<IActionResult> UpdateVehicle(Vehicle updatedVehicle) {
            _logger.LogInformation("正在启动ID为{VehicleId}的车辆更新过程", updatedVehicle.Id);

            var vehicle = await _context.Vehicle.FindAsync(updatedVehicle.Id);
            if (vehicle == null) {
                _logger.LogWarning("Vehicle with ID {VehicleId} not found", updatedVehicle.Id);
                return NotFound("Vehicle not found.");
            }

            // 更新车辆属性
            vehicle.PlateNumber = updatedVehicle.PlateNumber;
            vehicle.FrameNumber = updatedVehicle.FrameNumber;
            vehicle.Certificate = updatedVehicle.Certificate;
            vehicle.Invoice = updatedVehicle.Invoice;
            vehicle.Drivinglicense = updatedVehicle.Drivinglicense;
            vehicle.PurchaseRegistrationTime = updatedVehicle.PurchaseRegistrationTime;
            vehicle.Owner = updatedVehicle.Owner;
            vehicle.VehicleIntroduction = updatedVehicle.VehicleIntroduction;
            vehicle.Model = updatedVehicle.Model;
            vehicle.State = updatedVehicle.State;
            vehicle.IsCase = updatedVehicle.IsCase;
            vehicle.StateUpdatedAt = DateTime.Now;
            vehicle.UpdatedAt = DateTime.Now;

            // 设置并发标记
            _context.Entry(vehicle).Property("RowVersion").OriginalValue = updatedVehicle.RowVersion;

            try {
                _context.Vehicle.Update(vehicle);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Vehicle with ID {VehicleId} updated successfully", updatedVehicle.Id);
                return Ok("Vehicle updated successfully.");
            } catch (DbUpdateConcurrencyException) {
                _logger.LogWarning("使用ID更新车辆时发生并发冲突 {VehicleId}", updatedVehicle.Id);
                return Conflict("Update failed due to concurrent changes.");
            } catch (DbUpdateException ex) {
                _logger.LogError(ex, "Error updating vehicle with ID {VehicleId}", updatedVehicle.Id);
                return StatusCode(500, "Error updating vehicle.");
            } catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error while updating vehicle with ID {VehicleId}", updatedVehicle.Id);
                return StatusCode(500, "Unexpected error updating vehicle.");
            }
        }

        // 查询车辆
        public async Task<(List<Vehicle>, int sum)> SearchVehicles(int limit, int pageIndex, string? plateNumber = null, string? owner = null,  string sortField = "Id", string sortOrder = "asc") {
            if (plateNumber.IsNullOrEmpty() && owner.IsNullOrEmpty()) {
                return (await GetTablePage(limit, pageIndex), limit);
            }

            var query = _context.Vehicle.AsQueryable();
            
            // 取消跟踪实体
            query.AsNoTracking();

            // 过滤条件
            if (!string.IsNullOrEmpty(plateNumber)) {
                query = query.Where(v => v.PlateNumber.Contains(plateNumber));
            }
            if (!string.IsNullOrEmpty(owner)) {
                query = query.Where(v => v.Owner.Contains(owner));
            }

            // 排序逻辑
            query = sortField.ToLower() switch {
                "platenumber" => sortOrder == "asc" ? query.OrderBy(v => v.PlateNumber) : query.OrderByDescending(v => v.PlateNumber),
                "owner" => sortOrder == "asc" ? query.OrderBy(v => v.Owner) : query.OrderByDescending(v => v.Owner),
                "createdat" => sortOrder == "asc" ? query.OrderBy(v => v.CreatedAt) : query.OrderByDescending(v => v.CreatedAt),
                "updatedat" => sortOrder == "asc" ? query.OrderBy(v => v.UpdatedAt) : query.OrderByDescending(v => v.UpdatedAt),
                "stateupdatedat" => sortOrder == "asc" ? query.OrderBy(v => v.StateUpdatedAt) : query.OrderByDescending(v => v.StateUpdatedAt),
                _ => sortOrder == "asc" ? query.OrderBy(v => v.Id) : query.OrderByDescending(v => v.Id),
            };

            // 过滤掉已删除的记录
            query = query.Where(s => !s.IsDelete);

            int sum = await query.CountAsync();

            List<Vehicle> results = await query
                .Skip((pageIndex - 1) * limit) // 跳过前面页的数据
                .Take(limit) // 获取当前页的数据
                .ToListAsync();

            return (results, sum);
        }

        // 更新车辆图片信息
        public async Task<int> PutImagePath(int vehicleId, string fileId) {
            var vehicle = await _context.Vehicle.SingleOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle is null)
                return -1;

            // 删除原图片逻辑（如果有）
            if (!string.IsNullOrEmpty(vehicle.Pictures)) {
                var appId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
                var appSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;
                var envId = _wxSetting.Value.Env;

                List<string> fileidList = new() { vehicle.Pictures };

                // 删除原图片
                var re = await TcbApi.BatchDeleteFileAsync(appId, envId, fileidList);
                if (re.errcode != ReturnCode.请求成功) {
                    _logger.LogError("删除车辆{VehicleId}原图片{ImageId}", vehicle.Id, vehicle.Pictures);
                }
            }

            // 更新图片路径
            vehicle.Pictures = fileId;
            vehicle.UpdatedAt = DateTime.Now;
            try {
                _context.Vehicle.Update(vehicle);
                await _context.SaveChangesAsync();
            } catch (Exception e) {
                _logger.LogError(e, "保存车辆{VehicleId}图片路径", vehicle);
                return -2;
            }

            return 0;
        }
    }
}
