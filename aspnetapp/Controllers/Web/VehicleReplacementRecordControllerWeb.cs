namespace aspnetapp.Controllers.Web {
	public class VehicleReplacementRecordControllerWeb : Controller {

		private readonly MyDbContext _context;
		private readonly ILogger<VehicleReplacementRecordControllerWeb> _logger;

		public VehicleReplacementRecordControllerWeb(MyDbContext context, ILogger<VehicleReplacementRecordControllerWeb> logger) {
			_context = context;
			_logger = logger;
		}

		/// <summary>
		/// 获取所有换车记录
		/// </summary>
		public async Task<List<VehicleReplacementRecord>> GetAllReplacementRecords() {
			return await _context.VehicleReplacementRecord.ToListAsync();
		}

		/// <summary>
		/// 根据月份获取换车记录列表
		/// </summary>
		/// <param name="startOfMonth"></param>
		/// <param name="endOfMonth"></param>
		/// <returns></returns>
		public async Task<List<VehicleReplacementRecord>> GetAllReplacementRecordsByMonth(DateTime startOfMonth, DateTime endOfMonth) {
			// 查询该月份内的所有换车记录
			var ReplacementRecordsInMonth = await _context.VehicleReplacementRecord
				.Where(o => o.CreatedAt >= startOfMonth && o.CreatedAt <= endOfMonth)
				.ToListAsync();

			return ReplacementRecordsInMonth;
		}

		/// <summary>
		/// 根据ID获取换车记录
		/// </summary>
		public async Task<VehicleReplacementRecord?> GetReplacementRecordById(int id) {
			return await _context.VehicleReplacementRecord.FindAsync(id);
		}

		/// <summary>
		/// 获取换车记录总数
		/// </summary>
		public async Task<int> GetReplacementRecordCount() {
			return await _context.VehicleReplacementRecord.CountAsync();
		}

		/// <summary>
		/// 分页查询换车记录
		/// </summary>
		public async Task<List<VehicleReplacementRecord>> GetTablePage(int limit, int pageIndex) {
			var ReplacementRecords = await _context.VehicleReplacementRecord
				.OrderBy(o => o.Id) // 根据换车记录ID排序，确保分页顺序一致
				.Skip((pageIndex - 1) * limit) // 跳过前面页的数据
				.Take(limit) // 获取当前页的数据
				.ToListAsync();

			// 更新名称
			await FillReplacementRecordNamesAsync(ReplacementRecords);

			return ReplacementRecords;
		}

		/// <summary>
		/// 查询换车记录（支持多字段搜索）
		/// </summary>
		public async Task<(List<VehicleReplacementRecord>, int sum)> SearchReplacementRecords(int limit, int pageIndex, string? status = null, string? theOrder = null, string? theStore = null, string sortField = "Id", string sortReplacementRecord = "asc") {
			if (theOrder.IsNullOrEmpty() && status.IsNullOrEmpty() && theStore.IsNullOrEmpty() && sortField == "Id" && sortReplacementRecord == "asc") {
				var list = await GetTablePage(limit, pageIndex);
				return (list, list is null ? 0 : list.Count);
				//return (await GetTablePage(limit, pageIndex), limit);
			}

			var query = _context.VehicleReplacementRecord.AsQueryable();

			// 取消跟踪实体
			query.AsNoTracking();

			// 过滤条件
			if (!string.IsNullOrEmpty(theOrder) && int.TryParse(theOrder, out var orderId)) {
				query = query.Where(o => o.TheOrder == orderId); // 过滤订单编号
			}

			if (!string.IsNullOrEmpty(status)) {
				if (Enum.TryParse<VehicleReplacementRecord.Estates>(status, out var parsedStatus)) {
					query = query.Where(o => o.State == parsedStatus); // 过滤状态
				}
			}

			if (!string.IsNullOrEmpty(theStore) && int.TryParse(theStore, out var storeId)) {
				query = query.Where(o => o.TheStore == storeId); // 过滤门店
			}

			// 排序逻辑
			query = sortField.ToLower() switch {
				"createdat" => sortReplacementRecord == "asc" ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt),
				"updatedat" => sortReplacementRecord == "asc" ? query.OrderBy(o => o.UpdatedAt) : query.OrderByDescending(o => o.UpdatedAt),
				_ => sortReplacementRecord == "asc" ? query.OrderBy(o => o.Id) : query.OrderByDescending(o => o.Id),
			};

			int sum = await query.CountAsync();

			List<VehicleReplacementRecord> results = await query
				.Skip((pageIndex - 1) * limit) // 跳过前面页的数据
				.Take(limit) // 获取当前页的数据
				.ToListAsync();

			// 更新门店名称
			await FillReplacementRecordNamesAsync(results);

			return (results, sum);
		}

		/// <summary>
		/// 更新换车记录信息
		/// </summary>
		public async Task<IActionResult> UpdateReplacementRecord(VehicleReplacementRecord updatedReplacementRecord) {
			_logger.LogInformation("正在更新换车记录，换车记录ID：{ReplacementRecordId}", updatedReplacementRecord.Id);

			var ReplacementRecord = await _context.VehicleReplacementRecord.FindAsync(updatedReplacementRecord.Id);
			if (ReplacementRecord == null) {
				_logger.LogWarning("未找到换车记录，换车记录ID：{ReplacementRecordId}", updatedReplacementRecord.Id);
				return NotFound("未找到指定的换车记录。");
			}

			// 更新换车记录属性
			ReplacementRecord.UpdatedAt = DateTime.Now;

			// 设置并发标记
			_context.Entry(ReplacementRecord).Property("RowVersion").OriginalValue = updatedReplacementRecord.RowVersion;

			try {
				_context.VehicleReplacementRecord.Update(ReplacementRecord);
				await _context.SaveChangesAsync();
				_logger.LogInformation("换车记录更新成功，换车记录ID：{ReplacementRecordId}", updatedReplacementRecord.Id);
				return Ok("换车记录更新成功。");
			} catch (DbUpdateConcurrencyException) {
				_logger.LogWarning("更新换车记录时发生并发冲突，换车记录ID：{ReplacementRecordId}", updatedReplacementRecord.Id);
				return Conflict("更新失败，记录已被其他用户修改。");
			} catch (DbUpdateException ex) {
				_logger.LogError(ex, "更新换车记录时发生数据库错误，换车记录ID：{ReplacementRecordId}", updatedReplacementRecord.Id);
				return StatusCode(500, "更新换车记录时发生数据库错误。");
			} catch (Exception ex) {
				_logger.LogError(ex, "更新换车记录时发生未知错误，换车记录ID：{ReplacementRecordId}", updatedReplacementRecord.Id);
				return StatusCode(500, "更新换车记录时发生未知错误。");
			}
		}

		/// <summary>
		/// 填充换车记录中的相关名称（换车门店、旧车辆、新车辆）
		/// </summary>
		/// <param name="replacementRecords">换车记录列表</param>
		public async Task FillReplacementRecordNamesAsync(List<VehicleReplacementRecord> replacementRecords) {
			// 获取所有涉及的门店、旧车辆和新车辆的ID
			var storeIds = replacementRecords.Select(r => r.TheStore).Distinct().ToList();
			var vehicleIds = replacementRecords
				.SelectMany(r => new[] { r.TheOldVehicles, r.TheNewVehicles })
				.Distinct()
				.ToList();

			// 查询所有门店名称
			var stores = await _context.Store
				.Where(s => storeIds.Contains(s.Id))
				.ToDictionaryAsync(s => s.Id, s => s.Name);

			// 查询所有车辆的车牌号
			var vehicles = await _context.Vehicle
				.Where(v => vehicleIds.Contains(v.Id))
				.ToDictionaryAsync(v => v.Id, v => v.PlateNumber);

			// 遍历每个换车记录并填充名称
			foreach (var record in replacementRecords) {
				// 填充换车门店名称
				if (stores.TryGetValue(record.TheStore, out var storeName)) {
					record.StoreName = storeName;
				}

				// 填充旧车辆车牌号
				if (vehicles.TryGetValue(record.TheOldVehicles, out var oldVehiclePlate)) {
					record.OldVehiclePlateNumber = oldVehiclePlate;
				}

				// 填充新车辆车牌号
				if (vehicles.TryGetValue(record.TheNewVehicles, out var newVehiclePlate)) {
					record.NewVehiclePlateNumber = newVehiclePlate;
				}
			}
		}
	}
}
