namespace aspnetapp.Controllers.Web {
	public class StatisticsDataControllerWeb {
		private readonly MyDbContext _context;
		private readonly ILogger<StatisticsDataControllerWeb> _logger;

		public StatisticsDataControllerWeb(MyDbContext context, ILogger<StatisticsDataControllerWeb> logger) {
			_context = context;
			_logger = logger;
		}

		/// <summary>
		/// 获取用户总数
		/// </summary>
		/// <returns></returns>
		public async Task<int> GetUserSumAsync() {
			return await _context.User.CountAsync();
		}

		/// <summary>
		/// 获取车辆总数
		/// </summary>
		/// <returns></returns>
		public async Task<int> GetVehicleSumAsync() {
			return await _context.Vehicle.CountAsync(v => v.IsDelete == false);
		}

		/// <summary>
		/// 获取门店总数
		/// </summary>
		/// <returns></returns>
		public async Task<int> GetStoreSumAsync() {
			return await _context.Store.CountAsync(s => s.IsDelete == false);
		}

		/// <summary>
		/// 获取订单总数
		/// </summary>
		/// <returns></returns>
		public async Task<int> GetOrderSumAsync() {
			return await _context.Order.CountAsync();
		}

		/// <summary>
		/// 获取最近任意个月的订单趋势（按月统计）
		/// </summary>
		/// <param name="months">最近的月数（包含当前月）</param>
		/// <returns>每月订单数量的列表，按月份降序排序</returns>
		public async Task<List<(string Month, int OrderCount)>> GetOrderTrendsAsync(int months) {
			if (months <= 0) {
				throw new ArgumentException("月份数量必须大于0", nameof(months));
			}

			try {
				// 计算统计范围的起始日期
				var startDate = DateTime.Now.AddMonths(-months + 1).Date;

				// 查询数据库，仅筛选和分组统计
				var trends = await _context.Order
					.Where(o => o.CreatedAt >= startDate)
					.GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
					.Select(g => new {
						Year = g.Key.Year,
						Month = g.Key.Month,
						OrderCount = g.Count()
					})
					.OrderByDescending(g => g.Year)
					.ThenByDescending(g => g.Month)
					.ToListAsync();

				// 将分组结果转换为所需格式
				return trends
					.Select(t => ($"{t.Year}-{t.Month:00}", t.OrderCount))
					.ToList();
			} catch (Exception ex) {
				_logger.LogError(ex, "获取订单趋势数据失败");
				throw;
			}
		}


		/// <summary>
		/// 获取最近任意个月的用户增长趋势（按月统计）
		/// </summary>
		/// <param name="months">最近的月数（包含当前月）</param>
		/// <returns>每月新增用户数量的列表，按月份降序排序</returns>
		public async Task<List<(string Month, int UserCount)>> GetUserTrendsAsync(int months) {
			if (months <= 0) {
				throw new ArgumentException("月份数量必须大于0", nameof(months));
			}

			try {
				// 计算统计范围的起始日期
				var startDate = DateTime.Now.AddMonths(-months + 1).Date;

				// 查询数据库，仅筛选和分组统计
				var trends = await _context.User
					.Where(u => u.CreatedAt >= startDate)
					.GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
					.Select(g => new {
						Year = g.Key.Year,
						Month = g.Key.Month,
						UserCount = g.Count()
					})
					.OrderByDescending(g => g.Year)
					.ThenByDescending(g => g.Month)
					.ToListAsync();

				// 将分组结果转换为所需格式
				return trends
					.Select(t => ($"{t.Year}-{t.Month:00}", t.UserCount))
					.ToList();
			} catch (Exception ex) {
				_logger.LogError(ex, "获取用户趋势数据失败");
				throw;
			}
		}

		/// <summary>
		/// 统计车辆状态分布
		/// </summary>
		/// <returns>每种状态下车辆总数的列表</returns>
		public async Task<List<(string State, int VehicleCount)>> GetVehicleStateDistributionAsync() {
			try {
				// 查询并分组统计车辆状态
				var stateDistribution = await _context.Vehicle
					.Where(v => !v.IsDelete) // 排除已删除的车辆
					.GroupBy(v => v.State)
					.Select(g => new {
						State = g.Key.ToString(), // 将状态枚举转换为字符串
						VehicleCount = g.Count()
					})
					.OrderByDescending(g => g.VehicleCount) // 按数量降序排列
					.ToListAsync();

				// 转换为返回类型
				return stateDistribution.Select(sd => (sd.State, sd.VehicleCount)).ToList();
			} catch (Exception ex) {
				_logger.LogError(ex, "获取车辆状态分布失败");
				throw;
			}
		}
	}
}
