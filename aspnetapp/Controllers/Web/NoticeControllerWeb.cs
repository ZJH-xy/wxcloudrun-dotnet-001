namespace aspnetapp.Controllers.Web {
	public class NoticeControllerWeb : Controller {
		private readonly MyDbContext _context;
		private readonly ILogger<NoticeControllerWeb> _logger;

		public NoticeControllerWeb(MyDbContext context, ILogger<NoticeControllerWeb> logger) {
			_context = context;
			_logger = logger;
		}

		/// <summary>
		/// 分页获取
		/// </summary>
		/// <param name="limit"></param>
		/// <param name="pageIndex"></param>
		/// <returns></returns>
		public async Task<List<Notice>> GetTablePageAsync(int limit, int pageIndex) {
			return await _context.Notice
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
		public async Task<List<Notice>> GetAllListAsync() {
			return await _context.Notice.Where(s => !s.IsDelete).ToListAsync();
		}

		/// <summary>
		/// 根据ID获取单个
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<Notice?> GetByIdAsync(int id) {
			return await _context.Notice.FindAsync(id);
		}
		public async Task<int> GetPageSum() {
			return await _context.Notice.Where(s => !s.IsDelete).CountAsync();
		}

		/// <summary>
		/// 添加
		/// </summary>
		/// <param name="newAdvertisement"></param>
		/// <returns></returns>
		public async Task<IActionResult> AddAsync(Notice newAdvertisement) {
			if (!ModelState.IsValid) {
				return BadRequest("公告信息无效");
			}

			try {
				newAdvertisement.CreatedAt = DateTime.Now;
				newAdvertisement.UpdatedAt = DateTime.Now;

				// 这里保存公告到数据库
				_context.Notice.Add(newAdvertisement);
				await _context.SaveChangesAsync();

				return Ok("公告添加成功");
			} catch (Exception ex) {
				return StatusCode(500, $"添加公告时发生错误: {ex.Message}");
			}
		}

		// 更新公告信息
		public async Task<IActionResult> UpdateAsync(Notice updatedAdvertisement) {
			_logger.LogInformation("正在启动ID为{VehicleId}的公告更新过程", updatedAdvertisement.Id);

			var advertisement = await _context.Notice.FindAsync(updatedAdvertisement.Id);
			if (advertisement == null) {
				_logger.LogWarning("Notice with ID {VehicleId} not found", updatedAdvertisement.Id);
				return NotFound("Notice not found.");
			}

			// 更新公告属性
			advertisement.Title = updatedAdvertisement.Title;
			advertisement.Content = updatedAdvertisement.Content;
			advertisement.UpdatedAt = DateTime.Now;

			// 设置并发标记
			_context.Entry(advertisement).Property("RowVersion").OriginalValue = updatedAdvertisement.RowVersion;

			try {
				_context.Notice.Update(advertisement);
				await _context.SaveChangesAsync();
				_logger.LogInformation("Notice with ID {VehicleId} updated successfully", updatedAdvertisement.Id);
				return Ok("Notice updated successfully.");
			} catch (DbUpdateConcurrencyException) {
				_logger.LogWarning("使用ID更新公告时发生并发冲突 {VehicleId}", updatedAdvertisement.Id);
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
		public async Task<(List<Notice>, int sum)> SearchAsync(int limit, int pageIndex, string? title = null, string? content = null, string sortField = "Id", string sortOrder = "asc") {
			if (title.IsNullOrEmpty() && content.IsNullOrEmpty() && sortField != "Id" && sortOrder != "asc") {
				return (await GetTablePageAsync(limit, pageIndex), limit);
			}

			var query = _context.Notice.AsQueryable();

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

			List<Notice> results = await query
				.Skip((pageIndex - 1) * limit) // 跳过前面页的数据
				.Take(limit) // 获取当前页的数据
				.ToListAsync();

			return (results, sum);
		}
	}
}
