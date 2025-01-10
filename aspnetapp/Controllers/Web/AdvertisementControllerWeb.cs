using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;

namespace aspnetapp.Controllers.Web {
	public class AdvertisementControllerWeb : Controller {
		private readonly MyDbContext _context;
		private readonly ILogger<AdvertisementControllerWeb> _logger;
		private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

		public AdvertisementControllerWeb(MyDbContext context, ILogger<AdvertisementControllerWeb> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
			_context = context;
			_logger = logger;
			_wxSetting = wxSetting;
		}

		/// <summary>
		/// 分页获取
		/// </summary>
		/// <param name="limit"></param>
		/// <param name="pageIndex"></param>
		/// <returns></returns>
		public async Task<List<Advertisement>> GetTablePageAsync(int limit, int pageIndex) {
			return await _context.Advertisement
				.Where(s => !s.IsDelete)
				.OrderBy(u => u.Id) // 根据主键排序，确保分页顺序一致
				.Skip((pageIndex - 1) * limit) // 跳过前面页的数据
				.Take(limit) // 获取当前页的数据
				.ToListAsync();
		}

		/// <summary>
		/// 获取所有列表
		/// </summary>
		/// <returns></returns>
		public async Task<List<Advertisement>> GetAllListAsync() {
			return await _context.Advertisement.Where(s => !s.IsDelete).ToListAsync();
		}

		/// <summary>
		/// 根据ID获取单个
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<Advertisement?> GetByIdAsync(int id) {
			return await _context.Advertisement.FindAsync(id);
		}
		public async Task<int> GetPageSum() {
			return await _context.Advertisement.Where(s => !s.IsDelete).CountAsync();
		}

		/// <summary>
		/// 添加
		/// </summary>
		/// <param name="newAdvertisement"></param>
		/// <returns></returns>
		public async Task<IActionResult> AddAsync(Advertisement newAdvertisement) {
			if (!ModelState.IsValid) {
				return BadRequest("车辆信息无效");
			}

			try {
				newAdvertisement.CreatedAt = DateTime.Now;
				newAdvertisement.UpdatedAt = DateTime.Now;

				// 这里保存车辆到数据库
				_context.Advertisement.Add(newAdvertisement);
				await _context.SaveChangesAsync();

				return Ok("车辆添加成功");
			} catch (Exception ex) {
				return StatusCode(500, $"添加车辆时发生错误: {ex.Message}");
			}
		}

		// 更新车辆信息
		public async Task<IActionResult> UpdateAsync(Advertisement updatedAdvertisement) {
			_logger.LogInformation("正在启动ID为{VehicleId}的车辆更新过程", updatedAdvertisement.Id);

			var advertisement = await _context.Advertisement.FindAsync(updatedAdvertisement.Id);
			if (advertisement == null) {
				_logger.LogWarning("Vehicle with ID {VehicleId} not found", updatedAdvertisement.Id);
				return NotFound("Vehicle not found.");
			}

			// 更新车辆属性
			advertisement.Title = updatedAdvertisement.Title;
			advertisement.Content = updatedAdvertisement.Content;
			advertisement.Src = updatedAdvertisement.Src;
			advertisement.Pictures = updatedAdvertisement.Pictures;
			advertisement.UpdatedAt = DateTime.Now;

			// 设置并发标记
			_context.Entry(advertisement).Property("RowVersion").OriginalValue = updatedAdvertisement.RowVersion;

			try {
				_context.Advertisement.Update(advertisement);
				await _context.SaveChangesAsync();
				_logger.LogInformation("Vehicle with ID {VehicleId} updated successfully", updatedAdvertisement.Id);
				return Ok("Vehicle updated successfully.");
			} catch (DbUpdateConcurrencyException) {
				_logger.LogWarning("使用ID更新车辆时发生并发冲突 {VehicleId}", updatedAdvertisement.Id);
				return Conflict("Update failed due to concurrent changes.");
			} catch (DbUpdateException ex) {
				_logger.LogError(ex, "Error updating vehicle with ID {VehicleId}", updatedAdvertisement.Id);
				return StatusCode(500, "Error updating vehicle.");
			} catch (Exception ex) {
				_logger.LogError(ex, "Unexpected error while updating vehicle with ID {VehicleId}", updatedAdvertisement.Id);
				return StatusCode(500, "Unexpected error updating vehicle.");
			}
		}

		/// <summary>
		/// 查询
		/// </summary>
		/// <param name="limit"></param>
		/// <param name="pageIndex"></param>
		/// <param name="title"></param>
		/// <param name="content"></param>
		/// <param name="sortField"></param>
		/// <param name="sortOrder"></param>
		/// <returns></returns>
		public async Task<(List<Advertisement>, int sum)> SearchAsync(int limit, int pageIndex, string? title = null, string? content = null, string sortField = "Id", string sortOrder = "asc") {
			if (title.IsNullOrEmpty() && content.IsNullOrEmpty() && sortField == "Id" && sortOrder == "asc") {
				return (await GetTablePageAsync(limit, pageIndex), limit);
			}

			var query = _context.Advertisement.AsQueryable();

			// 取消跟踪实体
			query.AsNoTracking();

			// 过滤条件
			if (!string.IsNullOrEmpty(title)) {
				query = query.Where(v => v.Title.Contains(title));
			}
			if (!string.IsNullOrEmpty(content)) {
				query = query.Where(v => v.Content.Contains(content));
			}

			// 排序逻辑
			query = sortField.ToLower() switch {
				"title" => sortOrder == "asc" ? query.OrderBy(v => v.Title) : query.OrderByDescending(v => v.Title),
				"content" => sortOrder == "asc" ? query.OrderBy(v => v.Content) : query.OrderByDescending(v => v.Content),
				"createdat" => sortOrder == "asc" ? query.OrderBy(v => v.CreatedAt) : query.OrderByDescending(v => v.CreatedAt),
				"updatedat" => sortOrder == "asc" ? query.OrderBy(v => v.UpdatedAt) : query.OrderByDescending(v => v.UpdatedAt),
				_ => sortOrder == "asc" ? query.OrderBy(v => v.Id) : query.OrderByDescending(v => v.Id),
			};

			// 过滤掉已删除的记录
			query = query.Where(s => !s.IsDelete);

			int sum = await query.CountAsync();

			List<Advertisement> results = await query
				.Skip((pageIndex - 1) * limit) // 跳过前面页的数据
				.Take(limit) // 获取当前页的数据
				.ToListAsync();

			return (results, sum);
		}

		/// <summary>
		/// 更新图片信息
		/// </summary>
		/// <param name="vehicleId"></param>
		/// <param name="fileId"></param>
		/// <returns></returns>
		public async Task<int> PutImagePath(int vehicleId, string fileId) {
			var vehicle = await _context.Advertisement.SingleOrDefaultAsync(v => v.Id == vehicleId);

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
				_context.Advertisement.Update(vehicle);
				await _context.SaveChangesAsync();
			} catch (Exception e) {
				_logger.LogError(e, "保存车辆{VehicleId}图片路径", vehicle);
				return -2;
			}

			return 0;
		}
	}
}
